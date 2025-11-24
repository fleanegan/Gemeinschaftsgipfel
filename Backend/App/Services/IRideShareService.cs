using Gemeinschaftsgipfel.Controllers.DTOs;
using Gemeinschaftsgipfel.Models;

namespace Gemeinschaftsgipfel.Services;

public interface IRideShareService
{
    Task<RideShare> AddRideShare(RideShareCreationDto dto, string userName);
    Task<RideShare> GetRideShareById(string id);
    Task<RideShare> UpdateRideShare(RideShareUpdateDto dto, string userName);
    Task<IEnumerable<RideShare>> FetchAllExceptLoggedIn(string userName);
    Task<IEnumerable<RideShare>> FetchAllOfLoggedIn(string userName);
    Task RemoveRideShare(string id, string userName);
    Task CancelRideShare(string id, string userName);
    Task UncancelRideShare(string id, string userName);
    Task AddReservation(string rideShareId, string userName);
    Task RemoveReservation(string rideShareId, string userName);
    Task CommentOnRideShare(string rideShareId, string content, string userName);
    Task<IEnumerable<RideShareComment>> GetCommentsForRideShare(string rideShareId);
}
