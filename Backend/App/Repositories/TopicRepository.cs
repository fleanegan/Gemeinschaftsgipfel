using Gemeinschaftsgipfel.Models;
using Microsoft.EntityFrameworkCore;

namespace Gemeinschaftsgipfel.Repositories;

public class TopicRepository(DatabaseContextApplication dbContext)
{
    public async Task<IEnumerable<Topic>> GetAll()
    {
        return await dbContext
            .Topics
            .Include(topic => topic.User)
            .Where(topic => topic.Id != "")
            .ToListAsync();
    }

    public async Task<Topic> Create(Topic newTopic)
    {
        dbContext.Topics.Add(newTopic);
        await dbContext.SaveChangesAsync();
        return newTopic;
    }

    public async Task<Topic?> FetchBy(string topicId)
    {
        return await dbContext.Topics
            .Include(c => c.User)
            .Include(c => c.Votes)
            .ThenInclude(c => c.Voter)
            .FirstOrDefaultAsync(c => c.Id == topicId);
    }

    public async Task<Topic> Update(Topic updatedTopic)
    {
        dbContext.Topics.Update(updatedTopic);
        await dbContext.SaveChangesAsync();
        return (await FetchBy(updatedTopic.Id))!;
    }

    public async Task<IEnumerable<Topic>> GetAllExceptForUser(string userName)
    {
        return await dbContext.Topics
            .Include(c => c.User)
            .Include(c => c.Votes)
            .ThenInclude(c => c.Voter)
            .Where(topic => topic.User.UserName.ToLower() != userName.ToLower())
            .ToListAsync();
    }

    public async Task<IEnumerable<Topic>> GetAllForUser(string userName)
    {
        return await dbContext.Topics
            .Include(c => c.User)
            .Include(c => c.Votes)
            .ThenInclude(c => c.Voter)
            .Where(topic => topic.User.UserName.ToLower() == userName.ToLower())
            .ToListAsync();
    }

    public async Task Remove(Topic topicToDelete)
    {
        // Remove related votes first to avoid foreign key constraint
        var votes = dbContext.Votes.Where(v => v.Topic.Id == topicToDelete.Id);
        dbContext.Votes.RemoveRange(votes);

        // Remove related comments first to avoid foreign key constraint
        var comments = dbContext.TopicComments.Where(c => c.Topic.Id == topicToDelete.Id);
        dbContext.TopicComments.RemoveRange(comments);

        dbContext.Remove(topicToDelete);
        await dbContext.SaveChangesAsync();
    }
}
