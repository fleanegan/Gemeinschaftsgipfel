using Gemeinschaftsgipfel.Models;
using Microsoft.EntityFrameworkCore;

namespace Gemeinschaftsgipfel.Repositories;

public class RideShareCommentRepository(DatabaseContextApplication dbContext)
{
    public async Task<RideShareComment> Save(RideShareComment newRideShareComment)
    {
        dbContext.RideShareComments.Add(newRideShareComment);
        await dbContext.SaveChangesAsync();
        return newRideShareComment;
    }

    public async Task<IEnumerable<RideShareComment>> GetCommentsForRideShare(string rideShareId)
    {
        return await dbContext.RideShareComments
            .Include(rideShareComment => rideShareComment.Creator)
            .Include(rideShareComment => rideShareComment.RideShare)
            .Where(rideShareComment => rideShareComment.RideShare.Id == rideShareId)
            .OrderByDescending(rideShareComment => rideShareComment.CreatedAt)
            .ToListAsync();
    }
}
