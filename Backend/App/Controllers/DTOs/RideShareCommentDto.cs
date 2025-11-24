using System.ComponentModel.DataAnnotations;

namespace Gemeinschaftsgipfel.Controllers.DTOs;

public record RideShareCommentDto(
    [Required] string rideShareId,
    [Required] string content
);
