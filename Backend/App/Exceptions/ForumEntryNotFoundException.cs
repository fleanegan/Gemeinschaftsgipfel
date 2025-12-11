namespace Gemeinschaftsgipfel.Exceptions;

public class ForumEntryNotFoundException(string id) : Exception("ForumEntry with id " + id + " not found")
{
}
