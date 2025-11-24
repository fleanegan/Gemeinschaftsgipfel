namespace Gemeinschaftsgipfel.Exceptions;

public class ReservationImpossibleException(string id) : Exception("The Reservation for RideShare with id " + id + " failed")
{
}
