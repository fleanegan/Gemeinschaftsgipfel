using Gemeinschaftsgipfel.Controllers.DTOs;
using Gemeinschaftsgipfel.Exceptions;
using Gemeinschaftsgipfel.Models;
using Gemeinschaftsgipfel.Repositories;
using Gemeinschaftsgipfel.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Moq;

namespace Tests.Services;

public class RideShareServiceTest
{
    private static readonly DateTime DefaultDepartureTime = new(2025, 12, 31, 10, 0, 0);

    [Theory]
    [InlineData("Berlin to Munich", "", 3, "Berlin", "Munich")]
    [InlineData("Road Trip", null, 1, "Hamburg", "Frankfurt")]
    [InlineData("Weekend Trip", "Fun ride with music", 5, "Cologne", "Dusseldorf")]
    public async Task Test_add_GIVEN_correct_input_THEN_store_in_db(string title, string? description, int availableSeats, string from, string to)
    {
        const string loggedInUserName = "Fake User";
        var instance = await CreateInstance([loggedInUserName]);

        await instance.Service.AddRideShare(
            new RideShareCreationDto(title, availableSeats, from, to, DefaultDepartureTime, description, null),
            loggedInUserName);

        var result = (await instance.Repository.GetAll()).ToArray()[0];
        Assert.NotNull(result);
        Assert.Equal(title, result.Title);
        Assert.Equal(description ?? "", result.Description);
        Assert.Equal(availableSeats, result.AvailableSeats);
        Assert.Equal(from, result.From);
        Assert.Equal(to, result.To);
        Assert.Equal(DefaultDepartureTime, result.DepartureTime);
        Assert.Equal(loggedInUserName, result.Driver.UserName);
        Assert.Equal(RideShareStatus.Active, result.Status);
    }

    [Fact]
    public async Task Test_getById_GIVEN_non_existing_id_THEN_throw_exception()
    {
        const string nonExistingId = "nonExistingId";
        var instance = await CreateInstance([]);

        async Task Action()
        {
            await instance.Service.GetRideShareById(nonExistingId);
        }

        await Assert.ThrowsAsync<RideShareNotFoundException>(Action);
    }

    [Fact]
    public async Task Test_getById_GIVEN_existing_id_THEN_return_result()
    {
        const string loggedInUserName = "Fake User";
        var instance = await CreateInstance([loggedInUserName]);
        var dto = new RideShareCreationDto("Berlin to Munich", 3, "Berlin", "Munich", DefaultDepartureTime, "description", null);
        var expectedResult = await instance.Service.AddRideShare(dto, loggedInUserName);

        var actualResult = await instance.Service.GetRideShareById(expectedResult.Id);

        Assert.Equal(expectedResult.Id, actualResult.Id);
        Assert.Equal(expectedResult.Description, actualResult.Description);
        Assert.Equal(expectedResult.Title, actualResult.Title);
    }

    [Fact]
    public async Task Test_update_GIVEN_non_existing_id_THEN_throw_exception()
    {
        const string loggedInUserName = "Fake User";
        var nonExistingId = "the original ride share does not exist";
        var updatedRideShare = new RideShareUpdateDto(nonExistingId, "title", 3, "Berlin", "Munich", DefaultDepartureTime, "description", null);
        var instance = await CreateInstance([loggedInUserName]);

        async Task Action()
        {
            await instance.Service.UpdateRideShare(updatedRideShare, loggedInUserName);
        }

        await Assert.ThrowsAsync<RideShareNotFoundException>(Action);
    }

    [Fact]
    public async Task Test_update_GIVEN_UserName_different_from_driver_THEN_throw_exception()
    {
        const string loggedInUserName = "Fake User";
        var instance = await CreateInstance([loggedInUserName]);
        var originalRideShare = await instance.Service.AddRideShare(
            new RideShareCreationDto("original title", 3, "Berlin", "Munich", DefaultDepartureTime, "", null),
            "anotherUserName");
        var updatedRideShare = new RideShareUpdateDto(originalRideShare.Id, "updated title", 5, "Hamburg", "Frankfurt", DefaultDepartureTime, "updated description", null);

        async Task Action()
        {
            await instance.Service.UpdateRideShare(updatedRideShare, loggedInUserName);
        }

        await Assert.ThrowsAsync<UnauthorizedRideShareModificationException>(Action);
    }

    [Theory]
    [InlineData("New title", "")]
    [InlineData("New title", null)]
    [InlineData("New title", "New description")]
    public async Task Test_update_GIVEN_authorized_user_and_existing_ride_share_WHEN_passing_with_new_values_THEN_update(
        string newTitle, string? newDescription)
    {
        const string loggedInUserName = "Fake User";
        var instance = await CreateInstance([loggedInUserName]);
        var originalRideShare = await instance.Service.AddRideShare(
            new RideShareCreationDto("original title", 3, "Berlin", "Munich", DefaultDepartureTime, "", null),
            loggedInUserName);
        var newDepartureTime = DefaultDepartureTime.AddDays(1);
        var updatedRideShare = new RideShareUpdateDto(originalRideShare.Id, newTitle, 5, "Hamburg", "Frankfurt", newDepartureTime, newDescription, "Stop in Hannover");

        await instance.Service.UpdateRideShare(updatedRideShare, loggedInUserName);

        var result = await instance.Repository.FetchBy(originalRideShare.Id);
        Assert.Equal(updatedRideShare.Title, result!.Title);
        Assert.Equal(updatedRideShare.AvailableSeats, result.AvailableSeats);
        Assert.Equal(updatedRideShare.From, result.From);
        Assert.Equal(updatedRideShare.To, result.To);
        Assert.Equal(newDepartureTime, result.DepartureTime);
        Assert.Equal(updatedRideShare.Stops, result.Stops);
    }

    [Fact]
    public async Task Test_fetchAllExceptLoggedIn_GIVEN_zero_ride_shares_THEN_return_empty()
    {
        var loggedInUserName = "loggedInUserName";
        var instance = await CreateInstance([loggedInUserName]);

        var result = await instance.Service.FetchAllExceptLoggedIn(loggedInUserName);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Test_fetchAllExceptLoggedIn_GIVEN_two_ride_shares_by_other_users_THEN_return_them_all()
    {
        var otherUserName = "otherUserName";
        var firstDto = new RideShareCreationDto("first title", 3, "Berlin", "Munich", DefaultDepartureTime, "first description", null);
        var secondDto = new RideShareCreationDto("second title", 2, "Hamburg", "Frankfurt", DefaultDepartureTime, "second description", null);
        var instance = await CreateInstance([otherUserName]);
        await instance.Service.AddRideShare(firstDto, otherUserName);
        await instance.Service.AddRideShare(secondDto, otherUserName);

        var result = await instance.Service.FetchAllExceptLoggedIn("loggedInUserName");

        var enumerable = result as RideShare[] ?? result.ToArray();
        Assert.Equal(2, enumerable.Length);
        Assert.Contains(enumerable, rs => rs.Title == firstDto.Title && rs.Description == firstDto.Description);
        Assert.Contains(enumerable, rs => rs.Title == secondDto.Title && rs.Description == secondDto.Description);
    }

    [Fact]
    public async Task Test_fetchAllExceptLoggedIn_GIVEN_ride_share_logged_in_with_different_case_THEN_do_not_show_as_foreign()
    {
        var nameDuringCreation = "loggedInUserName";
        var nameDuringRetrieval = "LOGGEDINUSERNAME";
        var instance = await CreateInstance([nameDuringCreation]);
        var ownRideShare = new RideShareCreationDto("first title", 3, "Berlin", "Munich", DefaultDepartureTime, "first description", null);
        await instance.Service.AddRideShare(ownRideShare, nameDuringCreation);

        var result = await instance.Service.FetchAllExceptLoggedIn(nameDuringRetrieval);

        var collection = result as RideShare[] ?? result.ToArray();
        Assert.Empty(collection);
    }

    [Fact]
    public async Task Test_fetchAllExceptLoggedIn_GIVEN_two_ride_shares_by_two_different_users_THEN_return_only_those_by_other_user()
    {
        var loggedInUserName = "loggedInUserName";
        var otherUserName = "otherUserName";
        var instance = await CreateInstance([loggedInUserName, otherUserName]);
        var firstDto = new RideShareCreationDto("first title", 3, "Berlin", "Munich", DefaultDepartureTime, "first description", null);
        await instance.Service.AddRideShare(firstDto, otherUserName);
        var secondDto = new RideShareCreationDto("second title", 2, "Hamburg", "Frankfurt", DefaultDepartureTime, "second description", null);
        await instance.Service.AddRideShare(secondDto, loggedInUserName);

        var result = await instance.Service.FetchAllExceptLoggedIn(loggedInUserName);

        var collection = result as RideShare[] ?? result.ToArray();
        Assert.Single(collection);
        Assert.Contains(collection, rs => rs.Title == firstDto.Title && rs.Description == firstDto.Description);
        Assert.DoesNotContain(collection, rs => rs.Title == secondDto.Title && rs.Description == secondDto.Description);
    }

    [Fact]
    public async Task Test_fetchAllOfLoggedIn_GIVEN_zero_ride_shares_THEN_return_empty()
    {
        var loggedInUserName = "loggedInUserName";
        var instance = await CreateInstance([loggedInUserName]);

        var result = await instance.Service.FetchAllOfLoggedIn(loggedInUserName);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Test_fetchAllOfLoggedIn_GIVEN_two_ride_shares_by_other_users_THEN_return_empty()
    {
        var otherUserName = "otherUserName";
        var firstDto = new RideShareCreationDto("first title", 3, "Berlin", "Munich", DefaultDepartureTime, "first description", null);
        var secondDto = new RideShareCreationDto("second title", 2, "Hamburg", "Frankfurt", DefaultDepartureTime, "second description", null);
        var instance = await CreateInstance([otherUserName]);
        await instance.Service.AddRideShare(firstDto, otherUserName);
        await instance.Service.AddRideShare(secondDto, otherUserName);

        var result = await instance.Service.FetchAllOfLoggedIn("loggedInUserName");

        var enumerable = result as RideShare[] ?? result.ToArray();
        Assert.Empty(enumerable);
    }

    [Fact]
    public async Task Test_fetchAllOfLoggedIn_GIVEN_two_ride_shares_by_two_different_users_THEN_return_only_those_by_current_user()
    {
        var loggedInUserName = "loggedInUserName";
        var otherUserName = "otherUserName";
        var instance = await CreateInstance([loggedInUserName, otherUserName]);
        var firstDto = new RideShareCreationDto("first title", 3, "Berlin", "Munich", DefaultDepartureTime, "first description", null);
        await instance.Service.AddRideShare(firstDto, otherUserName);
        var secondDto = new RideShareCreationDto("second title", 2, "Hamburg", "Frankfurt", DefaultDepartureTime, "second description", null);
        await instance.Service.AddRideShare(secondDto, loggedInUserName);

        var result = await instance.Service.FetchAllOfLoggedIn(loggedInUserName);

        var collection = result as RideShare[] ?? result.ToArray();
        Assert.Single(collection);
        Assert.DoesNotContain(collection, rs => rs.Title == firstDto.Title && rs.Description == firstDto.Description);
        Assert.Contains(collection, rs => rs.Title == secondDto.Title && rs.Description == secondDto.Description);
    }

    [Fact]
    public async Task Test_fetchAllOfLoggedIn_GIVEN_case_difference_THEN_return_only_those_by_current_user()
    {
        var loggedInUserName = "loggedInUserName";
        var otherUserName = "otherUserName";
        var instance = await CreateInstance([loggedInUserName, otherUserName]);
        var firstDto = new RideShareCreationDto("first title", 3, "Berlin", "Munich", DefaultDepartureTime, "first description", null);
        await instance.Service.AddRideShare(firstDto, otherUserName);
        var secondDto = new RideShareCreationDto("second title", 2, "Hamburg", "Frankfurt", DefaultDepartureTime, "second description", null);
        await instance.Service.AddRideShare(secondDto, loggedInUserName);

        var result = await instance.Service.FetchAllOfLoggedIn(loggedInUserName.ToUpper());

        var collection = result as RideShare[] ?? result.ToArray();
        Assert.Single(collection);
        Assert.DoesNotContain(collection, rs => rs.Title == firstDto.Title && rs.Description == firstDto.Description);
        Assert.Contains(collection, rs => rs.Title == secondDto.Title && rs.Description == secondDto.Description);
    }

    [Fact]
    public async Task Test_removing_ride_share_GIVEN_non_existing_ride_share_THEN_throw_error()
    {
        const string loggedInUserName = "loggedInUserName";
        const string nonExistingId = "nonExistingId";
        var instance = await CreateInstance([loggedInUserName]);

        async Task Action()
        {
            await instance.Service.RemoveRideShare(nonExistingId, loggedInUserName);
        }

        await Assert.ThrowsAsync<RideShareNotFoundException>(Action);
    }

    [Fact]
    public async Task Test_removing_ride_share_GIVEN_someone_else_s_ride_share_THEN_throw_error()
    {
        const string loggedInUserName = "loggedInUserName";
        const string creatorUserName = "creatorUserName";
        var instance = await CreateInstance([loggedInUserName, creatorUserName]);
        var rideShareToDelete = await instance.Service.AddRideShare(
            new RideShareCreationDto("title", 3, "Berlin", "Munich", DefaultDepartureTime, "description", null),
            creatorUserName);

        async Task Action()
        {
            await instance.Service.RemoveRideShare(rideShareToDelete.Id, loggedInUserName);
        }

        var exception = await Assert.ThrowsAsync<Exception>(Action);
        Assert.Equal("Not your RideShare", exception.Message);
    }

    [Fact]
    public async Task Test_removing_ride_share_GIVEN_one_s_own_ride_share_THEN_remove_ride_share()
    {
        const string loggedInUserName = "loggedInUserName";
        var instance = await CreateInstance([loggedInUserName]);
        var rideShareToDelete = await instance.Service.AddRideShare(
            new RideShareCreationDto("title", 3, "Berlin", "Munich", DefaultDepartureTime, "description", null),
            loggedInUserName);

        await instance.Service.RemoveRideShare(rideShareToDelete.Id, loggedInUserName);

        Assert.Empty(await instance.Repository.GetAll());
    }

    [Fact]
    public async Task Test_canceling_ride_share_GIVEN_non_existing_ride_share_THEN_throw_error()
    {
        const string loggedInUserName = "loggedInUserName";
        const string nonExistingId = "nonExistingId";
        var instance = await CreateInstance([loggedInUserName]);

        async Task Action()
        {
            await instance.Service.CancelRideShare(nonExistingId, loggedInUserName);
        }

        await Assert.ThrowsAsync<RideShareNotFoundException>(Action);
    }

    [Fact]
    public async Task Test_canceling_ride_share_GIVEN_someone_else_s_ride_share_THEN_throw_error()
    {
        const string loggedInUserName = "loggedInUserName";
        const string creatorUserName = "creatorUserName";
        var instance = await CreateInstance([loggedInUserName, creatorUserName]);
        var rideShareToCancel = await instance.Service.AddRideShare(
            new RideShareCreationDto("title", 3, "Berlin", "Munich", DefaultDepartureTime, "description", null),
            creatorUserName);

        async Task Action()
        {
            await instance.Service.CancelRideShare(rideShareToCancel.Id, loggedInUserName);
        }

        await Assert.ThrowsAsync<UnauthorizedRideShareModificationException>(Action);
    }

    [Fact]
    public async Task Test_canceling_ride_share_GIVEN_one_s_own_ride_share_THEN_change_status_to_canceled()
    {
        const string loggedInUserName = "loggedInUserName";
        var instance = await CreateInstance([loggedInUserName]);
        var rideShareToCancel = await instance.Service.AddRideShare(
            new RideShareCreationDto("title", 3, "Berlin", "Munich", DefaultDepartureTime, "description", null),
            loggedInUserName);

        await instance.Service.CancelRideShare(rideShareToCancel.Id, loggedInUserName);

        var result = await instance.Repository.FetchBy(rideShareToCancel.Id);
        Assert.NotNull(result);
        Assert.Equal(RideShareStatus.Canceled, result.Status);
    }

    [Fact]
    public async Task Test_adding_reservation_GIVEN_a_non_existing_ride_share_THEN_throw_error()
    {
        const string loggedInUserName = "loggedInUserName";
        var instance = await CreateInstance([loggedInUserName]);
        const string nonExistingId = "non-existing Id";

        async Task Action()
        {
            await instance.Service.AddReservation(nonExistingId, loggedInUserName);
        }

        await Assert.ThrowsAsync<RideShareNotFoundException>(Action);
    }

    [Fact]
    public async Task Test_adding_reservation_GIVEN_an_existing_ride_share_THEN_add_passenger_to_ride_share()
    {
        const string loggedInUserName = "loggedInUserName";
        const string creatorUserName = "otherUserName";
        var instance = await CreateInstance([loggedInUserName, creatorUserName]);
        var dto = new RideShareCreationDto("first title", 3, "Berlin", "Munich", DefaultDepartureTime, "first description", null);
        var rideShare = await instance.Service.AddRideShare(dto, creatorUserName);

        await instance.Service.AddReservation(rideShare.Id, loggedInUserName);

        var result = await instance.Repository.FetchBy(rideShare.Id);
        Assert.Equal(loggedInUserName, result!.Reservations.ToArray()[0].Passenger.UserName);
    }

    [Fact]
    public async Task Test_adding_reservation_GIVEN_reserving_twice_with_same_user_THEN_throw_exception()
    {
        const string loggedInUserName = "loggedInUserName";
        const string creatorUserName = "otherUserName";
        var instance = await CreateInstance([loggedInUserName, creatorUserName]);
        var dto = new RideShareCreationDto("first title", 3, "Berlin", "Munich", DefaultDepartureTime, "first description", null);
        var rideShare = await instance.Service.AddRideShare(dto, creatorUserName);

        await instance.Service.AddReservation(rideShare.Id, loggedInUserName);

        async Task Action()
        {
            await instance.Service.AddReservation(rideShare.Id, loggedInUserName);
        }

        await Assert.ThrowsAsync<ReservationImpossibleException>(Action);
        var result = await instance.Repository.FetchBy(rideShare.Id);
        Assert.Single(result!.Reservations);
    }

    [Fact]
    public async Task Test_adding_reservation_GIVEN_no_seats_available_THEN_throw_exception()
    {
        const string passenger1 = "passenger1";
        const string passenger2 = "passenger2";
        const string creatorUserName = "driver";
        var instance = await CreateInstance([passenger1, passenger2, creatorUserName]);
        var dto = new RideShareCreationDto("first title", 1, "Berlin", "Munich", DefaultDepartureTime, "first description", null);
        var rideShare = await instance.Service.AddRideShare(dto, creatorUserName);
        await instance.Service.AddReservation(rideShare.Id, passenger1);

        async Task Action()
        {
            await instance.Service.AddReservation(rideShare.Id, passenger2);
        }

        await Assert.ThrowsAsync<ReservationImpossibleException>(Action);
        var result = await instance.Repository.FetchBy(rideShare.Id);
        Assert.Single(result!.Reservations);
    }

    [Fact]
    public async Task Test_removing_reservation_GIVEN_a_non_existing_ride_share_THEN_throw_error()
    {
        const string loggedInUserName = "loggedInUserName";
        var instance = await CreateInstance([loggedInUserName]);
        const string nonExistingId = "non-existing Id";

        async Task Action()
        {
            await instance.Service.RemoveReservation(nonExistingId, loggedInUserName);
        }

        await Assert.ThrowsAsync<RideShareNotFoundException>(Action);
    }

    [Fact]
    public async Task Test_removing_reservation_GIVEN_an_existing_reservation_THEN_remove_passenger_from_ride_share()
    {
        const string loggedInUserName = "loggedInUserName";
        const string creatorUserName = "otherUserName";
        var instance = await CreateInstance([loggedInUserName, creatorUserName]);
        var dto = new RideShareCreationDto("first title", 3, "Berlin", "Munich", DefaultDepartureTime, "first description", null);
        var rideShare = await instance.Service.AddRideShare(dto, creatorUserName);
        await instance.Service.AddReservation(rideShare.Id, loggedInUserName);

        await instance.Service.RemoveReservation(rideShare.Id, loggedInUserName);

        var result = await instance.Repository.FetchBy(rideShare.Id);
        Assert.Empty(result!.Reservations.ToArray());
    }

    [Fact]
    public async Task Test_removing_reservation_GIVEN_an_existing_reservation_with_another_case_THEN_remove_passenger_from_ride_share()
    {
        const string loggedInUserNameForReservationCreation = "loggedInUserName";
        const string loggedInUserNameForReservationRemoval = "LOGGEDINUSERNAME";
        const string creatorUserName = "otherUserName";
        var instance = await CreateInstance([loggedInUserNameForReservationCreation, loggedInUserNameForReservationRemoval]);
        var dto = new RideShareCreationDto("first title", 3, "Berlin", "Munich", DefaultDepartureTime, "first description", null);
        var rideShare = await instance.Service.AddRideShare(dto, creatorUserName);
        await instance.Service.AddReservation(rideShare.Id, loggedInUserNameForReservationCreation);

        await instance.Service.RemoveReservation(rideShare.Id, loggedInUserNameForReservationRemoval);

        var result = await instance.Repository.FetchBy(rideShare.Id);
        Assert.Empty(result!.Reservations.ToArray());
    }

    [Fact]
    public async Task Test_removing_reservation_GIVEN_removing_twice_with_same_user_THEN_do_nothing()
    {
        const string loggedInUserName = "loggedInUserName";
        const string creatorUserName = "otherUserName";
        var instance = await CreateInstance([loggedInUserName, creatorUserName]);
        var dto = new RideShareCreationDto("first title", 3, "Berlin", "Munich", DefaultDepartureTime, "first description", null);
        var rideShare = await instance.Service.AddRideShare(dto, creatorUserName);

        await instance.Service.RemoveReservation(rideShare.Id, loggedInUserName);

        var result = await instance.Repository.FetchBy(rideShare.Id);
        Assert.Empty(result!.Reservations);
    }

    [Fact]
    public async Task Test_commentOnRideShare_GIVEN_non_existing_ride_share_THEN_throw_exception()
    {
        const string loggedInUserName = "Fake User";
        var instance = await CreateInstance([loggedInUserName]);
        const string nonExistingId = "non-existing-id";

        async Task Action()
        {
            await instance.Service.CommentOnRideShare(nonExistingId, "Test content", loggedInUserName);
        }

        await Assert.ThrowsAsync<RideShareNotFoundException>(Action);
    }

    [Fact]
    public async Task Test_commentOnRideShare_GIVEN_existing_ride_share_THEN_create_comment()
    {
        const string loggedInUserName = "Fake User";
        var instance = await CreateInstance([loggedInUserName]);
        var rideShare = await instance.Service.AddRideShare(
            new RideShareCreationDto("Test RideShare", 3, "Berlin", "Munich", DefaultDepartureTime, "Test Description", null),
            loggedInUserName);

        await instance.Service.CommentOnRideShare(rideShare.Id, "Test comment content", loggedInUserName);

        var commentsAsArray = await FetchAllCommentsFromRepositoryForRideShare(instance, rideShare);
        Assert.Single(commentsAsArray);
        Assert.Equal("Test comment content", commentsAsArray[0].Content);
        Assert.Equal(loggedInUserName, commentsAsArray[0].Creator.UserName);
        Assert.Equal(rideShare.Id, commentsAsArray[0].RideShare.Id);
    }

    private static async Task<InstanceWrapper> CreateInstance(List<string> availableUserNames)
    {
        var dbContext = TestHelper.GetDbContext<DatabaseContextApplication>();
        var rideShareRepository = new RideShareRepository(dbContext);
        var reservationRepository = new RideShareReservationRepository(dbContext);
        var commentRepository = new RideShareCommentRepository(dbContext);
        var rideShareService = await GetService(dbContext, availableUserNames, rideShareRepository, reservationRepository, commentRepository);
        return new InstanceWrapper(rideShareRepository, commentRepository, rideShareService);
    }

    private static async Task<RideShareService> GetService(
        DatabaseContextApplication dbContext,
        List<string> userNames,
        RideShareRepository repository,
        RideShareReservationRepository reservationRepository,
        RideShareCommentRepository commentRepository)
    {
        var userStore = new UserStore<User>(dbContext);
        var userManager = await CannotInjectUserStoreDirectlySoWrappingInUserManager(userStore, userNames);
        var service = new RideShareService(repository, reservationRepository, commentRepository, userManager.Object);
        return service;
    }

    private static async Task<Mock<UserManager<User>>> CannotInjectUserStoreDirectlySoWrappingInUserManager(
        UserStore<User> userStore, List<string> userNames)
    {
        foreach (var userName in userNames)
            await userStore.CreateAsync(new User
                { AccessFailedCount = 0, UserName = userName, LockoutEnabled = false, EmailConfirmed = false });
        var userManager = TestHelper.GetMockUserManager(userStore);
        userManager.Setup(c => c.FindByNameAsync(It.IsAny<string>()))
            .Returns((string inputUserName) => Task.FromResult(new User { UserName = inputUserName })!);
        return userManager;
    }

    private class InstanceWrapper(
        RideShareRepository rideShareRepository,
        RideShareCommentRepository rideShareCommentRepository,
        RideShareService rideShareService)
    {
        public readonly RideShareRepository Repository = rideShareRepository;
        public readonly RideShareCommentRepository CommentRepository = rideShareCommentRepository;
        public readonly RideShareService Service = rideShareService;
    }

    private static async Task<RideShareComment[]> FetchAllCommentsFromRepositoryForRideShare(InstanceWrapper instance, RideShare rideShare)
    {
        var comments = await instance.CommentRepository.GetCommentsForRideShare(rideShare.Id);
        var commentsAsArray = comments as RideShareComment[] ?? comments.ToArray();
        return commentsAsArray;
    }
}
