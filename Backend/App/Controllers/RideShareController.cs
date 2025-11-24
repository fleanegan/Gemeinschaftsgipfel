using Gemeinschaftsgipfel.Controllers.DTOs;
using Gemeinschaftsgipfel.Controllers.Helpers;
using Gemeinschaftsgipfel.Exceptions;
using Gemeinschaftsgipfel.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gemeinschaftsgipfel.Controllers;

public class RideShareController(IRideShareService service) : AbstractController
{
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddNew([FromBody] RideShareCreationDto userInput)
    {
        var userName = GetUserNameFromAuthorization();
        try
        {
            var result = await service.AddRideShare(userInput, userName);
            return Ok(new RideShareResponseModel(
                result.Id, 
                result.Title, 
                result.AvailableSeats, 
                result.From, 
                result.To, 
                result.DepartureTime, 
                result.Description, 
                result.Stops, 
                userName,
                false,
                0,
                result.Status,
                new List<string>()));
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetOne(string id)
    {
        var userName = GetUserNameFromAuthorization();
        try
        {
            var rideShare = await service.GetRideShareById(id);
            return Ok(new RideShareResponseModel(
                rideShare.Id, 
                rideShare.Title, 
                rideShare.AvailableSeats, 
                rideShare.From, 
                rideShare.To, 
                rideShare.DepartureTime, 
                rideShare.Description, 
                rideShare.Stops, 
                rideShare.Driver.UserName,
                rideShare.Reservations.Count(r => r.Passenger.UserName.ToLower() == userName.ToLower()) > 0,
                rideShare.Reservations.Count,
                rideShare.Status,
                rideShare.Reservations.Select(r => r.Passenger.UserName).ToList()));
        }
        catch (RideShareNotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> Delete(string id)
    {
        var userName = GetUserNameFromAuthorization();
        try
        {
            await service.RemoveRideShare(id, userName);
        }
        catch (RideShareNotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (UnauthorizedRideShareModificationException)
        {
            return Forbid(new AuthenticationProperties());
        }

        return Ok();
    }

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> Update([FromBody] RideShareUpdateDto userInput)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        var userName = GetUserNameFromAuthorization();
        try
        {
            var result = await service.UpdateRideShare(userInput, userName);
            return Ok(new RideShareResponseModel(
                result.Id, 
                result.Title, 
                result.AvailableSeats, 
                result.From, 
                result.To, 
                result.DepartureTime, 
                result.Description, 
                result.Stops, 
                userName,
                result.Reservations.Count(r => r.Passenger.UserName.ToLower() == userName.ToLower()) > 0,
                result.Reservations.Count,
                result.Status,
                result.Reservations.Select(r => r.Passenger.UserName).ToList()));
        }
        catch (RideShareNotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (UnauthorizedRideShareModificationException)
        {
            return Forbid(new AuthenticationProperties());
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Cancel(string id)
    {
        var userName = GetUserNameFromAuthorization();
        try
        {
            await service.CancelRideShare(id, userName);
            return Ok();
        }
        catch (RideShareNotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (UnauthorizedRideShareModificationException)
        {
            return Forbid(new AuthenticationProperties());
        }
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> AllExceptLoggedIn()
    {
        var userName = GetUserNameFromAuthorization();
        var result = await service.FetchAllExceptLoggedIn(userName);
        var response = ResponseGenerator.GenerateRideShareResponses(result, userName);
        return Ok(response);
    }

    [Authorize]
    public async Task<IActionResult> AllOfLoggedIn()
    {
        var userName = GetUserNameFromAuthorization();
        var result = await service.FetchAllOfLoggedIn(userName);
        var response = ResponseGenerator.GenerateRideShareResponses(result, userName);
        return Ok(response);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> AddReservation([FromBody] RideShareReservationDto userInput)
    {
        if (!ModelState.IsValid)
            return BadRequest();
        var userName = GetUserNameFromAuthorization();
        try
        {
            await service.AddReservation(userInput.rideShareId, userName);
            return Ok();
        }
        catch (ReservationImpossibleException e)
        {
            return Conflict(e.Message);
        }
        catch (RideShareNotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    [Authorize]
    [HttpDelete]
    public async Task<IActionResult> RemoveReservation(string id)
    {
        var userName = GetUserNameFromAuthorization();
        try
        {
            await service.RemoveReservation(id, userName);
            return Ok();
        }
        catch (ReservationImpossibleException e)
        {
            return Conflict(e.Message);
        }
        catch (RideShareNotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CommentOnRideShare([FromBody] RideShareCommentDto userInput)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        var userName = GetUserNameFromAuthorization();
        try
        {
            await service.CommentOnRideShare(userInput.rideShareId, userInput.content, userName);
            return Ok();
        }
        catch (RideShareNotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
    
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Comments(string rideShareId)
    {
        try
        {
            var comments = await service.GetCommentsForRideShare(rideShareId);
            var response = comments.Select(p => new RideShareCommentResponseModel(
                p.Id,
                p.Content,
                p.Creator.UserName,
                p.CreatedAt));
            
            return Ok(response);
        }
        catch (RideShareNotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
}
