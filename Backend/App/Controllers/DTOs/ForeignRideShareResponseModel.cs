using Gemeinschaftsgipfel.Models;

namespace Gemeinschaftsgipfel.Controllers.DTOs;

public class ForeignRideShareResponseModel(
    string id,
    string title,
    int availableSeats,
    string from,
    string to,
    DateTime departureTime,
    string? description,
    string? stops,
    string driverUserName,
    bool didIReserve,
    RideShareStatus status)
{
    public string Id { get; } = id;
    public string Title { get; } = title;
    public int AvailableSeats { get; } = availableSeats;
    public string From { get; } = from;
    public string To { get; } = to;
    public DateTime DepartureTime { get; } = departureTime;
    public string? Description { get; } = description;
    public string? Stops { get; } = stops;
    public string DriverUserName { get; } = driverUserName;
    public bool DidIReserve { get; } = didIReserve;
    public RideShareStatus Status { get; } = status;
}
