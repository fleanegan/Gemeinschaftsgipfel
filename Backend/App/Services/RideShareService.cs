using Gemeinschaftsgipfel.Controllers.DTOs;
using Gemeinschaftsgipfel.Exceptions;
using Gemeinschaftsgipfel.Models;
using Gemeinschaftsgipfel.Repositories;
using Microsoft.AspNetCore.Identity;

namespace Gemeinschaftsgipfel.Services;

public class RideShareService(
    RideShareRepository rideShareRepository,
    RideShareReservationRepository reservationRepository,
    RideShareCommentRepository commentRepository,
    UserManager<User> userManager)
    : IRideShareService
{
    public async Task<RideShare> GetRideShareById(string id)
    {
        var result = await rideShareRepository.FetchBy(id);
        if (result == null) throw new RideShareNotFoundException(id);
        return result;
    }

    public async Task<RideShare> AddRideShare(RideShareCreationDto dto, string userName)
    {
        var user = await userManager.FindByNameAsync(userName);
        var newRideShare = RideShare.Create(
            dto.Description ?? "",
            dto.AvailableSeats,
            dto.From,
            dto.To,
            dto.DepartureTime,
            dto.Stops,
            user!);
        return await rideShareRepository.Create(newRideShare);
    }

    public async Task<RideShare> UpdateRideShare(RideShareUpdateDto dto, string userName)
    {
        var rideShareToChange = await rideShareRepository.FetchBy(dto.Id);
        if (rideShareToChange == null)
            throw new RideShareNotFoundException(dto.Id);
        if (userName != rideShareToChange.Driver.UserName)
            throw new UnauthorizedRideShareModificationException(rideShareToChange.Id);
        
        rideShareToChange.AvailableSeats = dto.AvailableSeats;
        rideShareToChange.From = dto.From;
        rideShareToChange.To = dto.To;
        rideShareToChange.DepartureTime = dto.DepartureTime;
        rideShareToChange.Description = dto.Description ?? "";
        rideShareToChange.Stops = dto.Stops;
        
        return await rideShareRepository.Update(rideShareToChange);
    }

    public async Task<IEnumerable<RideShare>> FetchAllExceptLoggedIn(string userName)
    {
        return await rideShareRepository.GetAllExceptForUser(userName);
    }

    public async Task<IEnumerable<RideShare>> FetchAllOfLoggedIn(string userName)
    {
        return await rideShareRepository.GetAllForUser(userName);
    }

    public async Task AddReservation(string rideShareId, string userName)
    {
        var rideShare = await rideShareRepository.FetchBy(rideShareId);
        if (rideShare == null) throw new RideShareNotFoundException(rideShareId);
        
        var passenger = await userManager.FindByNameAsync(userName);
        
        // Check if user already has a reservation
        if (rideShare.Reservations.Any(r => r.Passenger.UserName.ToLower() == userName.ToLower()))
            throw new ReservationImpossibleException(rideShareId);
        
        // Check if seats are available
        if (rideShare.AvailableSeats <= rideShare.Reservations.Count)
            throw new ReservationImpossibleException(rideShareId);
        
        var newReservation = new RideShareReservation(rideShare, passenger!);
        await reservationRepository.Create(newReservation);
    }

    public async Task RemoveReservation(string rideShareId, string userName)
    {
        var rideShare = await rideShareRepository.FetchBy(rideShareId);
        if (rideShare == null) throw new RideShareNotFoundException(rideShareId);
        
        var reservation = rideShare.Reservations.FirstOrDefault(
            r => r.Passenger.UserName.ToLower() == userName.ToLower());
        
        if (reservation != null) 
            await reservationRepository.Remove(reservation);
    }

    public async Task RemoveRideShare(string id, string userName)
    {
        var rideShare = await rideShareRepository.FetchBy(id);
        if (rideShare == null)
            throw new RideShareNotFoundException(id);
        if (rideShare.Driver.UserName != userName)
            throw new Exception("Not your RideShare");
        await rideShareRepository.Remove(rideShare);
    }

    public async Task CancelRideShare(string id, string userName)
    {
        var rideShare = await rideShareRepository.FetchBy(id);
        if (rideShare == null)
            throw new RideShareNotFoundException(id);
        if (rideShare.Driver.UserName != userName)
            throw new UnauthorizedRideShareModificationException(id);
        
        rideShare.Status = RideShareStatus.Canceled;
        await rideShareRepository.Update(rideShare);
    }

    public async Task UncancelRideShare(string id, string userName)
    {
        var rideShare = await rideShareRepository.FetchBy(id);
        if (rideShare == null)
            throw new RideShareNotFoundException(id);
        if (rideShare.Driver.UserName != userName)
            throw new UnauthorizedRideShareModificationException(id);
        
        rideShare.Status = RideShareStatus.Active;
        await rideShareRepository.Update(rideShare);
    }

    public async Task CommentOnRideShare(string rideShareId, string content, string userName)
    {
        var rideShare = await rideShareRepository.FetchBy(rideShareId);
        if (rideShare == null)
            throw new RideShareNotFoundException(rideShareId);
        var user = await userManager.FindByNameAsync(userName);
        await commentRepository.Save(RideShareComment.Create(content, user!, rideShare));
    }

    public async Task<IEnumerable<RideShareComment>> GetCommentsForRideShare(string rideShareId)
    {
        return await commentRepository.GetCommentsForRideShare(rideShareId);
    }
}
