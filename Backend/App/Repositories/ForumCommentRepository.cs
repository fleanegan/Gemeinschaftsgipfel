using Gemeinschaftsgipfel.Models;
using Microsoft.EntityFrameworkCore;

namespace Gemeinschaftsgipfel.Repositories;

public class ForumCommentRepository(DatabaseContextApplication dbContext)
{
    public async Task<ForumComment> Save(ForumComment newForumComment)
    {
        dbContext.ForumComments.Add(newForumComment);
        await dbContext.SaveChangesAsync();
        return newForumComment;
    }

    public async Task<IEnumerable<ForumComment>> GetCommentsForForumEntry(string forumEntryId)
    {
        return await dbContext.ForumComments
            .Include(comment => comment.Creator)
            .Include(comment => comment.ForumEntry)
            .Where(comment => comment.ForumEntry.Id == forumEntryId)
            .OrderByDescending(comment => comment.CreatedAt)
            .ToListAsync();
    }
}
