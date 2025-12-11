using Gemeinschaftsgipfel.Models;
using Microsoft.EntityFrameworkCore;

namespace Gemeinschaftsgipfel.Repositories;

public class ForumEntryRepository(DatabaseContextApplication dbContext)
{
    public async Task<IEnumerable<ForumEntry>> GetAll()
    {
        return await dbContext
            .ForumEntries
            .Include(entry => entry.User)
            .OrderByDescending(entry => entry.CreatedAt)
            .ToListAsync();
    }

    public async Task<ForumEntry> Create(ForumEntry newForumEntry)
    {
        dbContext.ForumEntries.Add(newForumEntry);
        await dbContext.SaveChangesAsync();
        return newForumEntry;
    }

    public async Task<ForumEntry?> FetchBy(string forumEntryId)
    {
        return await dbContext.ForumEntries
            .Include(entry => entry.User)
            .FirstOrDefaultAsync(entry => entry.Id == forumEntryId);
    }

    public async Task<ForumEntry> Update(ForumEntry updatedForumEntry)
    {
        dbContext.ForumEntries.Update(updatedForumEntry);
        await dbContext.SaveChangesAsync();
        return (await FetchBy(updatedForumEntry.Id))!;
    }

    public async Task Remove(ForumEntry forumEntry)
    {
        dbContext.ForumEntries.Remove(forumEntry);
        await dbContext.SaveChangesAsync();
    }
}
