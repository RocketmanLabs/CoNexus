namespace CoNexus.Api.Domain.Exceptions;

public class PublicationNotFoundException : DomainException
{
	public PublicationNotFoundException(int publicationId)
		: base($"Publication with ID {publicationId} was not found") { }
}