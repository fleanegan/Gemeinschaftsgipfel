using System.ComponentModel.DataAnnotations;
using Gemeinschaftsgipfel.Models;
using Constants = Gemeinschaftsgipfel.Properties.Constants;

namespace Gemeinschaftsgipfel.Controllers.DTOs;

public class TopicCreationDto(string title, int presentationTimeInMinutes, string? description, TopicCategory category, string? material)
{
    public string? Description { get; } = description;

    [Required] public string Title { get; } = title;

    [Required] public int PresentationTimeInMinutes {get;} = presentationTimeInMinutes;

    [Required(ErrorMessage = Constants.MissingCategoryErrorMessage)]
    public TopicCategory Category { get; } = category;

    [StringLength(Constants.MaxLengthMaterial, ErrorMessage = Constants.MaxLengthMaterialErrorMessage)]
    public string? Material { get; } = material;
}
