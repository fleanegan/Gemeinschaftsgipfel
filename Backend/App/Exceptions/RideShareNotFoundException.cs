namespace Gemeinschaftsgipfel.Exceptions;

public class RideShareNotFoundException(string id) : Exception("RideShare with id " + id + " not found")
{
}
