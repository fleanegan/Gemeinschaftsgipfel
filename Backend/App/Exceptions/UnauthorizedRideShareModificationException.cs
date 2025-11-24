namespace Gemeinschaftsgipfel.Exceptions;

public class UnauthorizedRideShareModificationException(string id)
    : UnauthorizedAccessException("You are not allowed to modify the ride share of id " + id + ".")
{
}
