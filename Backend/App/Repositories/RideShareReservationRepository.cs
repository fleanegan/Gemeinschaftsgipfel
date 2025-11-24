using Gemeinschaftsgipfel.Models;
using Microsoft.EntityFrameworkCore;

namespace Gemeinschaftsgipfel.Repositories;

public class RideShareReservationRepository(DatabaseContextApplication dbContext)
{
    public async Task Create(RideShareReservation newReservation)
    {
        dbContext.Add(newReservation);
        await dbContext.SaveChangesAsync();
    }

    public async Task Remove(RideShareReservation reservation)
    {
        dbContext.RideShareReservations.Remove(reservation);
        await dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<RideShareReservation>> FetchAll()
    {
        return await dbContext.RideShareReservations.ToListAsync();
    }
}
