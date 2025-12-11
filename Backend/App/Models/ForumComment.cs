using System.ComponentModel.DataAnnotations;
using static Gemeinschaftsgipfel.Properties.Constants;

namespace Gemeinschaftsgipfel.Models;

public class ForumComment
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public User Creator { get; set; }
    public ForumEntry ForumEntry { get; set; }

    private ForumComment() { }

    public static ForumComment Create(string content, User creator, ForumEntry forumEntry)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ValidationException("Content cannot be empty");
        
        if (content.Length > MaxLengthForumCommentContent)
            throw new ValidationException($"Content is too long (max {MaxLengthForumCommentContent} characters)");

        return new ForumComment
        {
            Content = content,
            CreatedAt = DateTime.UtcNow,
            Creator = creator,
            ForumEntry = forumEntry
        };
    }
}
