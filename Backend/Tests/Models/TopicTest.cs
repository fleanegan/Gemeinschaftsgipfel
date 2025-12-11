using System.ComponentModel.DataAnnotations;
using Gemeinschaftsgipfel.Models;

namespace Tests.Models;

public class TopicTest
{
    [Fact]
    public void Test_instantiation_GIVEN_empty_title_THEN_throw_error()
    {
        Assert.Throws<ValidationException>(() => Topic.Create("", 1, "Non-empty description", TopicCategory.Workshop, "material", new User { Id = "testId" }));
    }

    [Fact]
    public void Test_instantiation_GIVEN_too_long_title_THEN_throw_error()
    {
        var tooLongTitle = new string('a', 151);
        Assert.Throws<ValidationException>(() =>
            Topic.Create(
                tooLongTitle,
                1,
                "Non-empty description",
                TopicCategory.Sonstiges,
                "material",
                new User { Id = "testId" }
            )
        );
    }

    [Fact]
    public void Test_instantiation_GIVEN_too_long_description_THEN_throw_error()
    {
        var tooLongDescription = new string('e', 10001);
        Assert.Throws<ValidationException>(() =>
            Topic.Create(
                "This title is ok",
                1,
                tooLongDescription,
                TopicCategory.Vortrag,
                "material",
                new User { Id = "testId" }
            )
        );
    }

    [Fact]
    public void Test_instantiation_GIVEN_legally_long_description_THEN_create_instance()
    {
        var legallyLongDescription = new string('e', 10000);
        Topic.Create(
            "This title is ok",
            1,
            legallyLongDescription,
            TopicCategory.Sport,
            "material",
            new User { Id = "testId" }
        );
    }

    [Fact]
    public void Test_instantiation_GIVEN_correct_input_THEN_create_instance()
    {
        Topic.Create("title", 1, "description", TopicCategory.Diskussion, "material", new User { Id = "testId" });
    }
}
