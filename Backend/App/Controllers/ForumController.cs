using Gemeinschaftsgipfel.Controllers.DTOs;
using Gemeinschaftsgipfel.Exceptions;
using Gemeinschaftsgipfel.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gemeinschaftsgipfel.Controllers;

[Route("[controller]")]
public class ForumController(IForumService service) : AbstractController
{
    [HttpGet("all")]
    [Authorize]
    public async Task<IActionResult> GetAll()
    {
        var userName = GetUserNameFromAuthorization();
        var entries = await service.FetchAll();
        var response = entries.Select(entry => new ForumEntryResponseModel(
            entry.Id,
            entry.Title,
            entry.Content,
            entry.User.UserName,
            entry.CreatedAt
        )).ToList();
        return Ok(response);
    }

    [HttpGet("getone/{id}")]
    [Authorize]
    public async Task<IActionResult> GetOne(string id)
    {
        try
        {
            var entry = await service.GetForumEntryById(id);
            var response = new ForumEntryResponseModel(
                entry.Id,
                entry.Title,
                entry.Content,
                entry.User.UserName,
                entry.CreatedAt
            );
            return Ok(response);
        }
        catch (ForumEntryNotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    [HttpPost("addnew")]
    [Authorize]
    public async Task<IActionResult> AddNew([FromBody] ForumEntryCreationDto dto)
    {
        var userName = GetUserNameFromAuthorization();
        try
        {
            var result = await service.AddForumEntry(dto, userName);
            var response = new ForumEntryResponseModel(
                result.Id,
                result.Title,
                result.Content,
                result.User.UserName,
                result.CreatedAt
            );
            return Ok(response);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPut("update")]
    [Authorize]
    public async Task<IActionResult> Update([FromBody] ForumEntryUpdateDto dto)
    {
        var userName = GetUserNameFromAuthorization();
        try
        {
            var result = await service.UpdateForumEntry(dto, userName);
            var response = new ForumEntryResponseModel(
                result.Id,
                result.Title,
                result.Content,
                result.User.UserName,
                result.CreatedAt
            );
            return Ok(response);
        }
        catch (ForumEntryNotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (UnauthorizedForumEntryModificationException)
        {
            return Forbid(new AuthenticationProperties());
        }
    }

    [HttpDelete("delete/{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(string id)
    {
        var userName = GetUserNameFromAuthorization();
        try
        {
            await service.RemoveForumEntry(id, userName);
            return Ok();
        }
        catch (ForumEntryNotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (UnauthorizedForumEntryModificationException)
        {
            return Forbid(new AuthenticationProperties());
        }
    }

    [HttpPost("CommentOnForumEntry")]
    [Authorize]
    public async Task<IActionResult> CommentOnForumEntry([FromBody] ForumCommentDto dto)
    {
        var userName = GetUserNameFromAuthorization();
        try
        {
            await service.CommentOnForumEntry(dto.ForumEntryId, dto.Content, userName);
            var comments = await service.GetCommentsForForumEntry(dto.ForumEntryId);
            var latestComment = comments.First();
            return Ok(new
            {
                id = latestComment.Id,
                content = latestComment.Content,
                creatorUserName = latestComment.Creator.UserName,
                createdAt = latestComment.CreatedAt
            });
        }
        catch (ForumEntryNotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    [HttpGet("comments")]
    [Authorize]
    public async Task<IActionResult> GetComments([FromQuery] string ForumEntryId)
    {
        var comments = await service.GetCommentsForForumEntry(ForumEntryId);
        var response = comments.Select(comment => new
        {
            id = comment.Id,
            content = comment.Content,
            creatorUserName = comment.Creator.UserName,
            createdAt = comment.CreatedAt
        }).ToList();
        return Ok(response);
    }
}
