using System.ComponentModel.DataAnnotations;

namespace Gemeinschaftsgipfel.Controllers.DTOs;

public record RideShareReservationDto(
    [Required] string rideShareId
);
