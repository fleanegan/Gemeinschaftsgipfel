using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Gemeinschaftsgipfel.Properties;

namespace Gemeinschaftsgipfel.Models;

public class ForumEntry
{
    internal ForumEntry()
    {
        Title = "";
        Content = "";
        User = new User();
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string Id { get; set; } = null!;

    [StringLength(Constants.MaxLengthForumEntryTitle, ErrorMessage = Constants.MaxLengthForumEntryTitleErrorMessage)]
    [Required(ErrorMessage = Constants.EmptyForumEntryTitleErrorMessage)]
    public string Title { get; set; }

    [StringLength(Constants.MaxLengthForumEntryContent, ErrorMessage = Constants.MaxLengthForumEntryContentErrorMessage)]
    [Required(ErrorMessage = Constants.EmptyForumEntryContentErrorMessage)]
    public string Content { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public User User { get; init; }

    public static ForumEntry Create(string title, string content, User user)
    {
        var model = new ForumEntry
        {
            Title = title,
            Content = content,
            CreatedAt = DateTime.UtcNow,
            User = user
        };
        Validator.ValidateObject(model, new ValidationContext(model), true);
        return model;
    }
}
