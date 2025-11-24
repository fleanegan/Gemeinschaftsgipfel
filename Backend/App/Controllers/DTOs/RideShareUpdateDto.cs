using System.ComponentModel.DataAnnotations;

namespace Gemeinschaftsgipfel.Controllers.DTOs;

public class RideShareUpdateDto(
    string id,
    int availableSeats,
    string from,
    string to,
    DateTime departureTime,
    string? description,
    string? stops)
{
    [Required]
    public string Id { get; } = id;

    [Required]
    [Range(1, int.MaxValue)]
    public int AvailableSeats { get; } = availableSeats;

    [Required] 
    public string From { get; } = from;

    [Required] 
    public string To { get; } = to;

    [Required] 
    public DateTime DepartureTime { get; } = departureTime;

    public string? Description { get; } = description;

    public string? Stops { get; } = stops;
}
