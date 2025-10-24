namespace CoNexus.Api.Domain.ExternalEntities;

public class InvalidExternalEntityException : DomainException
{
	public InvalidExternalEntityException(string message) : base(message) { }
}
