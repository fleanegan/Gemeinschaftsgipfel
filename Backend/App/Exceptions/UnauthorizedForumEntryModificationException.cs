namespace Gemeinschaftsgipfel.Exceptions;

public class UnauthorizedForumEntryModificationException(string id)
    : UnauthorizedAccessException("You are not allowed to modify the forum entry of id " + id + ".")
{
}
