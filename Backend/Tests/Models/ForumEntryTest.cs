using System.ComponentModel.DataAnnotations;
using Gemeinschaftsgipfel.Models;

namespace Tests.Models;

public class ForumEntryTest
{
    [Fact]
    public void Test_instantiation_GIVEN_empty_title_THEN_throw_error()
    {
        Assert.Throws<ValidationException>(() => 
            ForumEntry.Create("", "Non-empty content", new User { Id = "testId" }));
    }

    [Fact]
    public void Test_instantiation_GIVEN_too_long_title_THEN_throw_error()
    {
        var tooLongTitle = new string('a', 151);
        Assert.Throws<ValidationException>(() =>
            ForumEntry.Create(tooLongTitle, "Non-empty content", new User { Id = "testId" })
        );
    }

    [Fact]
    public void Test_instantiation_GIVEN_empty_content_THEN_throw_error()
    {
        Assert.Throws<ValidationException>(() =>
            ForumEntry.Create("Valid title", "", new User { Id = "testId" })
        );
    }

    [Fact]
    public void Test_instantiation_GIVEN_too_long_content_THEN_throw_error()
    {
        var tooLongContent = new string('a', 10001);
        Assert.Throws<ValidationException>(() =>
            ForumEntry.Create("Valid title", tooLongContent, new User { Id = "testId" })
        );
    }

    [Fact]
    public void Test_instantiation_GIVEN_correct_input_THEN_create_instance()
    {
        var entry = ForumEntry.Create("Valid title", "Valid content", new User { Id = "testId" });
        
        Assert.NotNull(entry);
        Assert.Equal("Valid title", entry.Title);
        Assert.Equal("Valid content", entry.Content);
        Assert.True(entry.CreatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void Test_instantiation_GIVEN_max_length_title_THEN_create_instance()
    {
        var maxLengthTitle = new string('a', 150);
        var entry = ForumEntry.Create(maxLengthTitle, "Valid content", new User { Id = "testId" });
        
        Assert.NotNull(entry);
        Assert.Equal(maxLengthTitle, entry.Title);
    }

    [Fact]
    public void Test_instantiation_GIVEN_max_length_content_THEN_create_instance()
    {
        var maxLengthContent = new string('a', 10000);
        var entry = ForumEntry.Create("Valid title", maxLengthContent, new User { Id = "testId" });
        
        Assert.NotNull(entry);
        Assert.Equal(maxLengthContent, entry.Content);
    }
}
