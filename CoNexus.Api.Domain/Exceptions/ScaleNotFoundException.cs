namespace CoNexus.Api.Domain.Exceptions;

public class ScaleNotFoundException : DomainException
{
	public ScaleNotFoundException(int scaleId)
		: base($"Scale with ID {scaleId} was not found") { }
}