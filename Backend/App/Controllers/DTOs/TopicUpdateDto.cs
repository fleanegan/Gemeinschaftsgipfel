using System.ComponentModel.DataAnnotations;
using Gemeinschaftsgipfel.Models;
using Constants = Gemeinschaftsgipfel.Properties.Constants;

namespace Gemeinschaftsgipfel.Controllers.DTOs;

public class TopicUpdateDto(string id, string title, int presentationTimeInMinutes, string? description, TopicCategory category, string? material)
{
    [Required(ErrorMessage = Constants.EmptyIdErrorMessage)]
    public string Id { get; } = id;

    public string? Description { get; } = description;

    [Required(ErrorMessage = Constants.EmptyTitleErrorMessage)]
    public string Title { get; } = title;

    [Required] public int PresentationTimeInMinutes {get;} = presentationTimeInMinutes;

    [Required(ErrorMessage = Constants.MissingCategoryErrorMessage)]
    public TopicCategory Category { get; } = category;

    [StringLength(Constants.MaxLengthMaterial, ErrorMessage = Constants.MaxLengthMaterialErrorMessage)]
    public string? Material { get; } = material;
}
