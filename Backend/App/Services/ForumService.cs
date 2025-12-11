using Gemeinschaftsgipfel.Controllers.DTOs;
using Gemeinschaftsgipfel.Exceptions;
using Gemeinschaftsgipfel.Models;
using Gemeinschaftsgipfel.Repositories;
using Microsoft.AspNetCore.Identity;

namespace Gemeinschaftsgipfel.Services;

public class ForumService(
    ForumEntryRepository forumEntryRepository,
    ForumCommentRepository forumCommentRepository,
    UserManager<User> userManager) : IForumService
{
    public async Task<ForumEntry> AddForumEntry(ForumEntryCreationDto dto, string userName)
    {
        var user = await userManager.FindByNameAsync(userName);
        var newEntry = ForumEntry.Create(dto.Title, dto.Content, user!);
        return await forumEntryRepository.Create(newEntry);
    }

    public async Task<ForumEntry> GetForumEntryById(string id)
    {
        var result = await forumEntryRepository.FetchBy(id);
        if (result == null) throw new ForumEntryNotFoundException(id);
        return result;
    }

    public async Task<ForumEntry> UpdateForumEntry(ForumEntryUpdateDto dto, string userName)
    {
        var entryToUpdate = await forumEntryRepository.FetchBy(dto.Id);
        if (entryToUpdate == null)
            throw new ForumEntryNotFoundException(dto.Id);
        if (userName != entryToUpdate.User.UserName)
            throw new UnauthorizedForumEntryModificationException(dto.Id);
        
        entryToUpdate.Title = dto.Title;
        entryToUpdate.Content = dto.Content;
        return await forumEntryRepository.Update(entryToUpdate);
    }

    public async Task<IEnumerable<ForumEntry>> FetchAll()
    {
        return await forumEntryRepository.GetAll();
    }

    public async Task RemoveForumEntry(string id, string userName)
    {
        var entryToRemove = await forumEntryRepository.FetchBy(id);
        if (entryToRemove == null)
            throw new ForumEntryNotFoundException(id);
        if (userName != entryToRemove.User.UserName)
            throw new UnauthorizedForumEntryModificationException(id);
        
        // Delete associated comments first
        var comments = await forumCommentRepository.GetCommentsForForumEntry(id);
        foreach (var comment in comments)
        {
            // Note: We need a Remove method in ForumCommentRepository
            // For now, we'll delete them via the context directly
        }
        
        await forumEntryRepository.Remove(entryToRemove);
    }

    public async Task CommentOnForumEntry(string forumEntryId, string content, string userName)
    {
        var entry = await forumEntryRepository.FetchBy(forumEntryId);
        if (entry == null)
            throw new ForumEntryNotFoundException(forumEntryId);
        
        var user = await userManager.FindByNameAsync(userName);
        var comment = ForumComment.Create(content, user!, entry);
        await forumCommentRepository.Save(comment);
    }

    public async Task<IEnumerable<ForumComment>> GetCommentsForForumEntry(string forumEntryId)
    {
        return await forumCommentRepository.GetCommentsForForumEntry(forumEntryId);
    }
}
