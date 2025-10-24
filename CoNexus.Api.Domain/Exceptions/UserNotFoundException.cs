namespace CoNexus.Api.Domain.Exceptions;

public class UserNotFoundException : DomainException
{
	public UserNotFoundException(int userId)
		: base($"User with ID {userId} was not found") { }
}
