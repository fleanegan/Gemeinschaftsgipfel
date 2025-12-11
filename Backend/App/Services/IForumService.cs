using Gemeinschaftsgipfel.Controllers.DTOs;
using Gemeinschaftsgipfel.Models;

namespace Gemeinschaftsgipfel.Services;

public interface IForumService
{
    Task<ForumEntry> AddForumEntry(ForumEntryCreationDto dto, string userName);
    Task<ForumEntry> GetForumEntryById(string id);
    Task<ForumEntry> UpdateForumEntry(ForumEntryUpdateDto dto, string userName);
    Task<IEnumerable<ForumEntry>> FetchAll();
    Task RemoveForumEntry(string id, string userName);
    Task CommentOnForumEntry(string forumEntryId, string content, string userName);
    Task<IEnumerable<ForumComment>> GetCommentsForForumEntry(string forumEntryId);
}
