using System.Net;
using System.Text.Json;
using Gemeinschaftsgipfel.Controllers.DTOs;
using Gemeinschaftsgipfel.Exceptions;
using Gemeinschaftsgipfel.Models;
using Gemeinschaftsgipfel.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Tests.Controllers;

public class ForumControllerTest : IClassFixture<WebApplicationFactory<Program>>
{
    private const string HappyPathDummyId = "happyPathDummyId";
    private const string NonExistingDummyId = "nonExistingDummyId";
    private const string ConflictingDummyId = "conflictingDummyId";

    private readonly IEnumerable<ForumEntry> _demoForumEntries =
    [
        ForumEntry.Create("Test Title", "Test Content", 
            new User { UserName = AutoAuthorizeMiddleware.UserName })
    ];

    private readonly WebApplicationFactory<Program> _factoryWithAuthorization;
    private readonly WebApplicationFactory<Program> _factoryWithoutAuthorization;

    private Mock<IForumService>? _mockForumService;

    public ForumControllerTest(WebApplicationFactory<Program> factory)
    {
        TestHelper.ReadTestEnv();
        _factoryWithAuthorization = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IForumService));
                if (descriptor != null) services.Remove(descriptor);

                SetupMock(services);

                //authentication: this Middleware automatically adds user "FakeAuthUser"
                services.AddSingleton<IStartupFilter>(new AutoAuthorizeStartupFilter());
            });
        });

        _factoryWithoutAuthorization = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IForumService));
                if (descriptor != null) services.Remove(descriptor);

                SetupMock(services);
            });
        });
    }

    private void SetupMock(IServiceCollection services)
    {
        _mockForumService = new Mock<IForumService>();
        
        _mockForumService.Setup(s => s.GetForumEntryById(It.IsAny<string>())).Returns(async (string entryId) =>
        {
            return await Task.Run(() =>
            {
                if (entryId == NonExistingDummyId)
                    throw new ForumEntryNotFoundException(entryId);
                var entry = _demoForumEntries.ToArray()[0];
                entry.Id = HappyPathDummyId;
                return entry;
            });
        });
        
        _mockForumService.Setup(s => s.AddForumEntry(It.IsAny<ForumEntryCreationDto>(), It.IsAny<string>()))
            .ReturnsAsync((ForumEntryCreationDto newEntry, string userName) =>
            {
                var entry = ForumEntry.Create(newEntry.Title, newEntry.Content, 
                    new User { UserName = userName });
                entry.Id = HappyPathDummyId;
                return entry;
            });
        
        _mockForumService.Setup(s => s.UpdateForumEntry(It.IsAny<ForumEntryUpdateDto>(), It.IsAny<string>()))
            .ReturnsAsync((ForumEntryUpdateDto entryToUpdate, string userName) =>
            {
                if (entryToUpdate.Id == NonExistingDummyId)
                    throw new ForumEntryNotFoundException(entryToUpdate.Id);
                if (entryToUpdate.Id == ConflictingDummyId)
                    throw new UnauthorizedForumEntryModificationException(entryToUpdate.Id);
                var entry = ForumEntry.Create(entryToUpdate.Title, entryToUpdate.Content,
                    new User { UserName = userName });
                entry.Id = HappyPathDummyId;
                return entry;
            });
        
        _mockForumService.Setup(c => c.FetchAll()).ReturnsAsync(() => _demoForumEntries);
        
        _mockForumService.Setup(c => c.RemoveForumEntry(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(async (string entryId, string _) =>
            {
                await Task.Run(() =>
                {
                    if (entryId == NonExistingDummyId)
                        throw new ForumEntryNotFoundException(entryId);
                    if (entryId == ConflictingDummyId)
                        throw new UnauthorizedForumEntryModificationException(entryId);
                });
            });
        
        _mockForumService.Setup(c => c.CommentOnForumEntry(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(async (string entryId, string content, string userName) =>
            {
                await Task.Run(() =>
                {
                    if (entryId == NonExistingDummyId)
                        throw new ForumEntryNotFoundException(entryId);
                });
            });
        
        _mockForumService.Setup(c => c.GetCommentsForForumEntry(It.IsAny<string>())).ReturnsAsync((string entryId) =>
        {
            if (entryId == NonExistingDummyId)
                return [];
            return new List<ForumComment>
            {
                ForumComment.Create("Test comment", 
                    new User { UserName = "testUser" }, 
                    _demoForumEntries.First())
            };
        });

        services.AddScoped(_ => _mockForumService.Object);
    }

    [Fact]
    public async Task Test_GetAll_GIVEN_unauthorized_THEN_return_401()
    {
        var client = _factoryWithoutAuthorization.CreateClient();

        var response = await client.GetAsync("/forum/all");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Test_GetAll_GIVEN_authorized_THEN_return_200_and_entries()
    {
        var client = _factoryWithAuthorization.CreateClient();

        var response = await client.GetAsync("/forum/all");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseString = await response.Content.ReadAsStringAsync();
        var entries = JsonSerializer.Deserialize<List<ForumEntryResponseModel>>(responseString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(entries);
        Assert.NotEmpty(entries);
    }

    [Fact]
    public async Task Test_GetOne_GIVEN_non_existing_id_THEN_return_404()
    {
        var client = _factoryWithAuthorization.CreateClient();

        var response = await client.GetAsync($"/forum/getone/{NonExistingDummyId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Test_GetOne_GIVEN_existing_id_THEN_return_200_and_entry()
    {
        var client = _factoryWithAuthorization.CreateClient();

        var response = await client.GetAsync($"/forum/getone/{HappyPathDummyId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseString = await response.Content.ReadAsStringAsync();
        var entry = JsonSerializer.Deserialize<ForumEntryResponseModel>(responseString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(entry);
        Assert.Equal(HappyPathDummyId, entry.Id);
    }

    [Fact]
    public async Task Test_AddNew_GIVEN_unauthorized_THEN_return_401()
    {
        var client = _factoryWithoutAuthorization.CreateClient();
        var newEntry = new ForumEntryCreationDto("Test Title", "Test Content");

        var response = await client.PostAsync("/forum/addnew", TestHelper.EncodeBody(newEntry));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Test_AddNew_GIVEN_valid_input_THEN_return_200_and_created_entry()
    {
        var client = _factoryWithAuthorization.CreateClient();
        var newEntry = new ForumEntryCreationDto("Test Title", "Test Content");

        var response = await client.PostAsync("/forum/addnew", TestHelper.EncodeBody(newEntry));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseString = await response.Content.ReadAsStringAsync();
        var createdEntry = JsonSerializer.Deserialize<ForumEntryResponseModel>(responseString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(createdEntry);
        Assert.Equal("Test Title", createdEntry.Title);
        Assert.Equal("Test Content", createdEntry.Content);
    }

    [Fact]
    public async Task Test_Update_GIVEN_non_existing_id_THEN_return_404()
    {
        var client = _factoryWithAuthorization.CreateClient();
        var updateDto = new ForumEntryUpdateDto(NonExistingDummyId, "Updated Title", "Updated Content");

        var response = await client.PutAsync("/forum/update", TestHelper.EncodeBody(updateDto));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Test_Update_GIVEN_unauthorized_modification_THEN_return_403()
    {
        var client = _factoryWithAuthorization.CreateClient();
        var updateDto = new ForumEntryUpdateDto(ConflictingDummyId, "Updated Title", "Updated Content");

        var response = await client.PutAsync("/forum/update", TestHelper.EncodeBody(updateDto));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Test_Update_GIVEN_valid_input_THEN_return_200_and_updated_entry()
    {
        var client = _factoryWithAuthorization.CreateClient();
        var updateDto = new ForumEntryUpdateDto(HappyPathDummyId, "Updated Title", "Updated Content");

        var response = await client.PutAsync("/forum/update", TestHelper.EncodeBody(updateDto));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseString = await response.Content.ReadAsStringAsync();
        var updatedEntry = JsonSerializer.Deserialize<ForumEntryResponseModel>(responseString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(updatedEntry);
        Assert.Equal("Updated Title", updatedEntry.Title);
        Assert.Equal("Updated Content", updatedEntry.Content);
    }

    [Fact]
    public async Task Test_Delete_GIVEN_unauthorized_THEN_return_401()
    {
        var client = _factoryWithoutAuthorization.CreateClient();

        var response = await client.DeleteAsync($"/forum/delete/{HappyPathDummyId}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Test_Delete_GIVEN_non_existing_id_THEN_return_404()
    {
        var client = _factoryWithAuthorization.CreateClient();

        var response = await client.DeleteAsync($"/forum/delete/{NonExistingDummyId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Test_Delete_GIVEN_unauthorized_modification_THEN_return_403()
    {
        var client = _factoryWithAuthorization.CreateClient();

        var response = await client.DeleteAsync($"/forum/delete/{ConflictingDummyId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Test_Delete_GIVEN_valid_id_THEN_return_200()
    {
        var client = _factoryWithAuthorization.CreateClient();

        var response = await client.DeleteAsync($"/forum/delete/{HappyPathDummyId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Test_CommentOnForumEntry_GIVEN_unauthorized_THEN_return_401()
    {
        var client = _factoryWithoutAuthorization.CreateClient();
        var commentDto = new ForumCommentDto(HappyPathDummyId, "Test comment");

        var response = await client.PostAsync("/forum/CommentOnForumEntry", TestHelper.EncodeBody(commentDto));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Test_CommentOnForumEntry_GIVEN_non_existing_entry_THEN_return_404()
    {
        var client = _factoryWithAuthorization.CreateClient();
        var commentDto = new ForumCommentDto(NonExistingDummyId, "Test comment");

        var response = await client.PostAsync("/forum/CommentOnForumEntry", TestHelper.EncodeBody(commentDto));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Test_CommentOnForumEntry_GIVEN_valid_input_THEN_return_200()
    {
        var client = _factoryWithAuthorization.CreateClient();
        var commentDto = new ForumCommentDto(HappyPathDummyId, "Test comment");

        var response = await client.PostAsync("/forum/CommentOnForumEntry", TestHelper.EncodeBody(commentDto));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Test_GetComments_GIVEN_unauthorized_THEN_return_401()
    {
        var client = _factoryWithoutAuthorization.CreateClient();

        var response = await client.GetAsync($"/forum/comments?ForumEntryId={HappyPathDummyId}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Test_GetComments_GIVEN_valid_entry_id_THEN_return_200_and_comments()
    {
        var client = _factoryWithAuthorization.CreateClient();

        var response = await client.GetAsync($"/forum/comments?ForumEntryId={HappyPathDummyId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseString = await response.Content.ReadAsStringAsync();
        var comments = JsonSerializer.Deserialize<List<dynamic>>(responseString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(comments);
    }
}
