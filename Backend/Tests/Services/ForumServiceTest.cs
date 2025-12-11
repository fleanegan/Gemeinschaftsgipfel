using Gemeinschaftsgipfel.Controllers.DTOs;
using Gemeinschaftsgipfel.Exceptions;
using Gemeinschaftsgipfel.Models;
using Gemeinschaftsgipfel.Repositories;
using Gemeinschaftsgipfel.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Tests.Services;

public class ForumServiceTest
{
    [Fact]
    public async Task Test_add_GIVEN_correct_input_THEN_store_in_db()
    {
        const string loggedInUserName = "Fake User";
        var instance = await CreateInstance([loggedInUserName]);

        await instance.Service.AddForumEntry(
            new ForumEntryCreationDto("Test Title", "Test Content"),
            loggedInUserName);

        var result = (await instance.ForumEntryRepository.GetAll()).ToArray()[0];
        Assert.NotNull(result);
        Assert.Equal("Test Title", result.Title);
        Assert.Equal("Test Content", result.Content);
        Assert.Equal(loggedInUserName, result.User.UserName);
    }

    [Fact]
    public async Task Test_getById_GIVEN_non_existing_id_THEN_throw_exception()
    {
        const string nonExistingId = "nonExistingId";
        var instance = await CreateInstance([]);

        async Task Action()
        {
            await instance.Service.GetForumEntryById(nonExistingId);
        }

        await Assert.ThrowsAsync<ForumEntryNotFoundException>(Action);
    }

    [Fact]
    public async Task Test_getById_GIVEN_existing_id_THEN_return_result()
    {
        const string loggedInUserName = "Fake User";
        var instance = await CreateInstance([loggedInUserName]);
        var entryCreationDto = new ForumEntryCreationDto("Test Title", "Test Content");
        var expectedResult = await instance.Service.AddForumEntry(entryCreationDto, loggedInUserName);

        var actualResult = await instance.Service.GetForumEntryById(expectedResult.Id);

        Assert.Equal(expectedResult.Id, actualResult.Id);
        Assert.Equal(expectedResult.Content, actualResult.Content);
        Assert.Equal(expectedResult.Title, actualResult.Title);
    }

    [Fact]
    public async Task Test_update_GIVEN_non_existing_id_THEN_throw_exception()
    {
        const string loggedInUserName = "Fake User";
        var nonExistingId = "the original entry does not exist";
        var updatedEntry = new ForumEntryUpdateDto(nonExistingId, "Updated Title", "Updated Content");
        var instance = await CreateInstance([loggedInUserName]);

        async Task Action()
        {
            await instance.Service.UpdateForumEntry(updatedEntry, loggedInUserName);
        }

        await Assert.ThrowsAsync<ForumEntryNotFoundException>(Action);
    }

    [Fact]
    public async Task Test_update_GIVEN_UserName_different_from_creator_THEN_throw_exception()
    {
        const string loggedInUserName = "Fake User";
        var instance = await CreateInstance([loggedInUserName, "anotherUserName"]);
        var originalEntry = await instance.Service.AddForumEntry(
            new ForumEntryCreationDto("Original Title", "Original Content"),
            "anotherUserName");
        var updatedEntry = new ForumEntryUpdateDto(originalEntry.Id, "Updated Title", "Updated Content");

        async Task Action()
        {
            await instance.Service.UpdateForumEntry(updatedEntry, loggedInUserName);
        }

        await Assert.ThrowsAsync<UnauthorizedForumEntryModificationException>(Action);
    }

    [Fact]
    public async Task Test_update_GIVEN_authorized_user_and_existing_entry_WHEN_passing_with_new_values_THEN_update()
    {
        const string loggedInUserName = "Fake User";
        var instance = await CreateInstance([loggedInUserName]);
        var originalEntry = await instance.Service.AddForumEntry(
            new ForumEntryCreationDto("Original Title", "Original Content"),
            loggedInUserName);
        var updatedEntry = new ForumEntryUpdateDto(originalEntry.Id, "New Title", "New Content");

        await instance.Service.UpdateForumEntry(updatedEntry, loggedInUserName);

        var result = await instance.ForumEntryRepository.FetchBy(originalEntry.Id);
        Assert.Equal(updatedEntry.Title, result!.Title);
        Assert.Equal(updatedEntry.Content, result.Content);
    }

    [Fact]
    public async Task Test_fetchAll_GIVEN_zero_entries_THEN_return_empty()
    {
        var instance = await CreateInstance([]);

        var result = await instance.Service.FetchAll();

        Assert.Empty(result);
    }

    [Fact]
    public async Task Test_fetchAll_GIVEN_two_entries_THEN_return_them_all()
    {
        var userName = "testUser";
        var firstEntry = new ForumEntryCreationDto("First Title", "First Content");
        var secondEntry = new ForumEntryCreationDto("Second Title", "Second Content");
        var instance = await CreateInstance([userName]);
        await instance.Service.AddForumEntry(firstEntry, userName);
        await instance.Service.AddForumEntry(secondEntry, userName);

        var result = await instance.Service.FetchAll();

        var enumerable = result as ForumEntry[] ?? result.ToArray();
        Assert.Equal(2, enumerable.Count());
        Assert.Contains(enumerable, entry => entry.Title == firstEntry.Title && entry.Content == firstEntry.Content);
        Assert.Contains(enumerable, entry => entry.Title == secondEntry.Title && entry.Content == secondEntry.Content);
    }

    [Fact]
    public async Task Test_fetchAll_GIVEN_entries_by_different_users_THEN_return_all_sorted_by_created_date()
    {
        var userName1 = "user1";
        var userName2 = "user2";
        var instance = await CreateInstance([userName1, userName2]);
        
        // Add entries with slight delays to ensure different timestamps
        var firstEntry = await instance.Service.AddForumEntry(
            new ForumEntryCreationDto("First", "Content 1"), userName1);
        await Task.Delay(10);
        var secondEntry = await instance.Service.AddForumEntry(
            new ForumEntryCreationDto("Second", "Content 2"), userName2);
        await Task.Delay(10);
        var thirdEntry = await instance.Service.AddForumEntry(
            new ForumEntryCreationDto("Third", "Content 3"), userName1);

        var result = await instance.Service.FetchAll();

        var enumerable = result.ToArray();
        Assert.Equal(3, enumerable.Length);
        // Should be ordered by CreatedAt descending (newest first)
        Assert.Equal("Third", enumerable[0].Title);
        Assert.Equal("Second", enumerable[1].Title);
        Assert.Equal("First", enumerable[2].Title);
    }

    [Fact]
    public async Task Test_remove_GIVEN_non_existing_id_THEN_throw_exception()
    {
        const string loggedInUserName = "Fake User";
        var nonExistingId = "nonExistingId";
        var instance = await CreateInstance([loggedInUserName]);

        async Task Action()
        {
            await instance.Service.RemoveForumEntry(nonExistingId, loggedInUserName);
        }

        await Assert.ThrowsAsync<ForumEntryNotFoundException>(Action);
    }

    [Fact]
    public async Task Test_remove_GIVEN_different_user_THEN_throw_exception()
    {
        const string creatorUserName = "Creator";
        const string otherUserName = "Other User";
        var instance = await CreateInstance([creatorUserName, otherUserName]);
        var entry = await instance.Service.AddForumEntry(
            new ForumEntryCreationDto("Title", "Content"),
            creatorUserName);

        async Task Action()
        {
            await instance.Service.RemoveForumEntry(entry.Id, otherUserName);
        }

        await Assert.ThrowsAsync<UnauthorizedForumEntryModificationException>(Action);
    }

    [Fact]
    public async Task Test_remove_GIVEN_authorized_user_THEN_delete_entry()
    {
        const string loggedInUserName = "Fake User";
        var instance = await CreateInstance([loggedInUserName]);
        var entry = await instance.Service.AddForumEntry(
            new ForumEntryCreationDto("Title", "Content"),
            loggedInUserName);

        await instance.Service.RemoveForumEntry(entry.Id, loggedInUserName);

        var result = await instance.ForumEntryRepository.FetchBy(entry.Id);
        Assert.Null(result);
    }

    [Fact]
    public async Task Test_commentOnForumEntry_GIVEN_non_existing_id_THEN_throw_exception()
    {
        const string loggedInUserName = "Fake User";
        var nonExistingId = "nonExistingId";
        var instance = await CreateInstance([loggedInUserName]);

        async Task Action()
        {
            await instance.Service.CommentOnForumEntry(nonExistingId, "Test comment", loggedInUserName);
        }

        await Assert.ThrowsAsync<ForumEntryNotFoundException>(Action);
    }

    [Fact]
    public async Task Test_commentOnForumEntry_GIVEN_existing_entry_THEN_store_comment()
    {
        const string loggedInUserName = "Fake User";
        var instance = await CreateInstance([loggedInUserName]);
        var entry = await instance.Service.AddForumEntry(
            new ForumEntryCreationDto("Title", "Content"),
            loggedInUserName);

        await instance.Service.CommentOnForumEntry(entry.Id, "Test comment content", loggedInUserName);

        var commentsAsArray = await FetchAllCommentsFromRepositoryForForumEntry(instance, entry);
        Assert.Single(commentsAsArray);
        Assert.Equal("Test comment content", commentsAsArray[0].Content);
        Assert.Equal(loggedInUserName, commentsAsArray[0].Creator.UserName);
        Assert.Equal(entry.Id, commentsAsArray[0].ForumEntry.Id);
    }

    [Fact]
    public async Task Test_getCommentsForForumEntry_GIVEN_no_comments_THEN_return_empty()
    {
        const string loggedInUserName = "Fake User";
        var instance = await CreateInstance([loggedInUserName]);
        var entry = await instance.Service.AddForumEntry(
            new ForumEntryCreationDto("Title", "Content"),
            loggedInUserName);

        var comments = await instance.Service.GetCommentsForForumEntry(entry.Id);

        Assert.Empty(comments);
    }

    [Fact]
    public async Task Test_getCommentsForForumEntry_GIVEN_multiple_comments_THEN_return_all_sorted_by_date()
    {
        const string loggedInUserName = "Fake User";
        var instance = await CreateInstance([loggedInUserName]);
        var entry = await instance.Service.AddForumEntry(
            new ForumEntryCreationDto("Title", "Content"),
            loggedInUserName);

        await instance.Service.CommentOnForumEntry(entry.Id, "First comment", loggedInUserName);
        await Task.Delay(10);
        await instance.Service.CommentOnForumEntry(entry.Id, "Second comment", loggedInUserName);
        await Task.Delay(10);
        await instance.Service.CommentOnForumEntry(entry.Id, "Third comment", loggedInUserName);

        var comments = await instance.Service.GetCommentsForForumEntry(entry.Id);

        var commentsArray = comments.ToArray();
        Assert.Equal(3, commentsArray.Length);
        // Should be ordered by CreatedAt descending (newest first)
        Assert.Equal("Third comment", commentsArray[0].Content);
        Assert.Equal("Second comment", commentsArray[1].Content);
        Assert.Equal("First comment", commentsArray[2].Content);
    }

    private static async Task<InstanceWrapper> CreateInstance(List<string> availableUserNames)
    {
        var dbContext = TestHelper.GetDbContext<DatabaseContextApplication>();
        var forumEntryRepository = new ForumEntryRepository(dbContext);
        var forumCommentRepository = new ForumCommentRepository(dbContext);
        var forumService = await GetService(dbContext, availableUserNames, forumEntryRepository, forumCommentRepository);
        return new InstanceWrapper(forumEntryRepository, forumCommentRepository, forumService);
    }

    private static async Task<ForumService> GetService(
        DatabaseContextApplication dbContext, 
        List<string> userNames,
        ForumEntryRepository forumEntryRepository, 
        ForumCommentRepository forumCommentRepository)
    {
        var userStore = new UserStore<User>(dbContext);
        var userManager = await CannotInjectUserStoreDirectlySoWrappingInUserManager(userStore, userNames);
        var service = new ForumService(forumEntryRepository, forumCommentRepository, userManager.Object);
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
        ForumEntryRepository forumEntryRepository,
        ForumCommentRepository forumCommentRepository,
        ForumService forumService)
    {
        public readonly ForumEntryRepository ForumEntryRepository = forumEntryRepository;
        public readonly ForumCommentRepository ForumCommentRepository = forumCommentRepository;
        public readonly ForumService Service = forumService;
    }

    private static async Task<ForumComment[]> FetchAllCommentsFromRepositoryForForumEntry(
        InstanceWrapper instance, ForumEntry entry)
    {
        var comments = await instance.ForumCommentRepository.GetCommentsForForumEntry(entry.Id);
        var commentsAsArray = comments as ForumComment[] ?? comments.ToArray();
        return commentsAsArray;
    }
}
