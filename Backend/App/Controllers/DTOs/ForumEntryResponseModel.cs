namespace Gemeinschaftsgipfel.Controllers.DTOs;

public record ForumEntryResponseModel(
    string Id,
    string Title,
    string Content,
    string CreatorUserName,
    DateTime CreatedAt);
