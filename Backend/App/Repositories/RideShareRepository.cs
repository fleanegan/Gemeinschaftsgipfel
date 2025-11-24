using Gemeinschaftsgipfel.Models;
using Microsoft.EntityFrameworkCore;

namespace Gemeinschaftsgipfel.Repositories;

public class RideShareRepository(DatabaseContextApplication dbContext)
{
    public async Task<IEnumerable<RideShare>> GetAll()
    {
        return await dbContext
            .RideShares
            .Include(rideShare => rideShare.Driver)
            .Where(rideShare => rideShare.Id != "")
            .ToListAsync();
    }

    public async Task<RideShare> Create(RideShare newRideShare)
    {
        dbContext.RideShares.Add(newRideShare);
        await dbContext.SaveChangesAsync();
        return newRideShare;
    }

    public async Task<RideShare?> FetchBy(string rideShareId)
    {
        return await dbContext.RideShares
            .Include(c => c.Driver)
            .Include(c => c.Reservations)
            .ThenInclude(c => c.Passenger)
            .FirstOrDefaultAsync(c => c.Id == rideShareId);
    }

    public async Task<RideShare> Update(RideShare updatedRideShare)
    {
        dbContext.RideShares.Update(updatedRideShare);
        await dbContext.SaveChangesAsync();
        return (await FetchBy(updatedRideShare.Id))!;
    }

    public async Task<IEnumerable<RideShare>> GetAllExceptForUser(string userName)
    {
        return await dbContext.RideShares
            .Include(c => c.Driver)
            .Include(c => c.Reservations)
            .ThenInclude(c => c.Passenger)
            .Where(rideShare => rideShare.Driver.UserName.ToLower() != userName.ToLower())
            .ToListAsync();
    }

    public async Task<IEnumerable<RideShare>> GetAllForUser(string userName)
    {
        return await dbContext.RideShares
            .Include(c => c.Driver)
            .Include(c => c.Reservations)
            .ThenInclude(c => c.Passenger)
            .Where(rideShare => rideShare.Driver.UserName.ToLower() == userName.ToLower())
            .ToListAsync();
    }

    public async Task Remove(RideShare rideShareToDelete)
    {
        dbContext.Remove(rideShareToDelete);
        await dbContext.SaveChangesAsync();
    }
}
