using System.ComponentModel.DataAnnotations;
using Gemeinschaftsgipfel.Models;

namespace Tests.Models;

public class ForumCommentTest
{
    [Fact]
    public void Test_instantiation_GIVEN_empty_content_THEN_throw_error()
    {
        var forumEntry = ForumEntry.Create("Title", "Content", new User { Id = "userId" });
        
        Assert.Throws<ValidationException>(() =>
            ForumComment.Create("", new User { Id = "testId" }, forumEntry)
        );
    }

    [Fact]
    public void Test_instantiation_GIVEN_too_long_content_THEN_throw_error()
    {
        var forumEntry = ForumEntry.Create("Title", "Content", new User { Id = "userId" });
        var tooLongContent = new string('a', 5001);
        
        Assert.Throws<ValidationException>(() =>
            ForumComment.Create(tooLongContent, new User { Id = "testId" }, forumEntry)
        );
    }

    [Fact]
    public void Test_instantiation_GIVEN_correct_input_THEN_create_instance()
    {
        var forumEntry = ForumEntry.Create("Title", "Content", new User { Id = "userId" });
        var comment = ForumComment.Create("Valid comment content", new User { Id = "testId" }, forumEntry);
        
        Assert.NotNull(comment);
        Assert.Equal("Valid comment content", comment.Content);
        Assert.True(comment.CreatedAt <= DateTime.UtcNow);
        Assert.Equal(forumEntry.Id, comment.ForumEntry.Id);
    }

    [Fact]
    public void Test_instantiation_GIVEN_max_length_content_THEN_create_instance()
    {
        var forumEntry = ForumEntry.Create("Title", "Content", new User { Id = "userId" });
        var maxLengthContent = new string('a', 1000);
        var comment = ForumComment.Create(maxLengthContent, new User { Id = "testId" }, forumEntry);
        
        Assert.NotNull(comment);
        Assert.Equal(maxLengthContent, comment.Content);
    }
}
