namespace CoNexus.Api.Domain.Exceptions;

public class PublicationClosedException : DomainException
{
	public PublicationClosedException(int publicationId)
		: base($"Publication {publicationId} is closed and cannot accept responses") { }
}