using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Gemeinschaftsgipfel.Properties;

namespace Gemeinschaftsgipfel.Models;

public class RideShare
{
    internal RideShare()
    {
        Description = "";
        From = "";
        To = "";
        Driver = new User();
    }

    public RideShare(string description, int availableSeats, string from, string to, 
        DateTime departureTime, string? stops, User driver, ICollection<RideShareReservation> reservations)
    {
        Description = description;
        AvailableSeats = availableSeats;
        From = from;
        To = to;
        DepartureTime = departureTime;
        Stops = stops;
        Driver = driver;
        Reservations = reservations;
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string Id { get; set; } = null!;

    [StringLength(Constants.MaxLengthDescription, ErrorMessage = Constants.MaxLengthDescriptionErrorMessage)]
    public string Description { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Available seats must be at least 1")]
    public int AvailableSeats { get; set; }

    [Required]
    public string From { get; set; }

    [Required]
    public string To { get; set; }

    [Required]
    public DateTime DepartureTime { get; set; }

    public string? Stops { get; set; }

    public RideShareStatus Status { get; set; } = RideShareStatus.Active;

    [Required] 
    public User Driver { get; init; }

    public ICollection<RideShareReservation> Reservations { get; set; } = [];

    public static RideShare Create(string description, int availableSeats, 
        string from, string to, DateTime departureTime, string? stops, User driver)
    {
        var model = new RideShare 
        { 
            Description = description, 
            AvailableSeats = availableSeats,
            From = from,
            To = to,
            DepartureTime = departureTime,
            Stops = stops,
            Driver = driver 
        };
        Validator.ValidateObject(model, new ValidationContext(model), true);
        return model;
    }
}
