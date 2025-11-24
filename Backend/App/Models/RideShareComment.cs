using System.ComponentModel.DataAnnotations;
using static Gemeinschaftsgipfel.Properties.Constants;

namespace Gemeinschaftsgipfel.Models;

public class RideShareComment
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public User Creator { get; set; }
    public RideShare RideShare { get; set; }

    private RideShareComment() { }

    public static RideShareComment Create(string content, User creator, RideShare rideShare)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ValidationException("Content cannot be empty");
        
        if (content.Length > MaxLengthTopicCommentContent)
            throw new ValidationException("Content is too long (max 5000 characters)");

        return new RideShareComment
        {
            Content = content,
            CreatedAt = DateTime.Now,
            Creator = creator,
            RideShare = rideShare
        };
    }
}
