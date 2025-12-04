using Gemeinschaftsgipfel.Controllers.DTOs;
using Gemeinschaftsgipfel.Models;

namespace Gemeinschaftsgipfel.Controllers.Helpers;

public abstract class ResponseGenerator
{
    public static List<OwnTopicResponseModel> GenerateOwnTopicResponses(IEnumerable<Topic> fetchAllExceptLoggedIn)
    {
        return fetchAllExceptLoggedIn
            .Select(topic => new OwnTopicResponseModel(topic.Id, topic.Title, topic.PresentationTimeInMinutes, topic.Description, topic.User.UserName,
                topic.Votes.Count, topic.Category, topic.Material))
            .ToList();
    }

    public static List<ForeignTopicResponseModel> GenerateForeignTopicResponses(
        IEnumerable<Topic> fetchAllExceptLoggedIn, string loggedInUserName)
    {
        return fetchAllExceptLoggedIn
            .Select(topic =>
                new ForeignTopicResponseModel(
                    topic.Id,
                    topic.Title,
		    topic.PresentationTimeInMinutes,
                    topic.Description,
                    topic.User.UserName,
                    topic.Votes.Count(vote => vote.Voter.UserName.ToLower() == loggedInUserName.ToLower()) > 0,
                    topic.Category,
                    topic.Material
                )
            )
            .ToList();
    }

    public static List<SupportTaskResponseModel> GenerateSupportTaskResponses(IEnumerable<SupportTask> supportTasks)
    {
        return supportTasks
            .Select(task => new SupportTaskResponseModel(task.Id, task.Title, task.Description, task.Duration,
                task.RequiredSupporters,
                task.SupportPromises.Select(supporter => supporter.Supporter.UserName.ToLower()).Distinct().ToList()))
            .ToList();
    }

    public static List<RideShareResponseModel> GenerateRideShareResponses(
        IEnumerable<RideShare> rideShares, string loggedInUserName)
    {
        return rideShares
            .Select(rideShare =>
                new RideShareResponseModel(
                    rideShare.Id,
                    rideShare.AvailableSeats,
                    rideShare.From,
                    rideShare.To,
                    rideShare.DepartureTime,
                    rideShare.Description,
                    rideShare.Stops,
                    rideShare.Driver.UserName,
                    rideShare.Reservations.Count(r => r.Passenger.UserName.ToLower() == loggedInUserName.ToLower()) > 0,
                    rideShare.Reservations.Count,
                    rideShare.Status,
                    rideShare.Reservations.Select(r => r.Passenger.UserName).ToList()
                )
            )
            .ToList();
    }
}
