using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gemeinschaftsgipfel.Models;

public class RideShareReservation(RideShare rideShare, User passenger)
{
    internal RideShareReservation() : this(
        RideShare.Create("title", "", 1, "from", "to", DateTime.Now, null, new User()), 
        new User())
    {
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string Id { get; init; } = null!;

    [Required] public RideShare RideShare { get; init; } = rideShare;

    [Required] public User Passenger { get; init; } = passenger;
}
