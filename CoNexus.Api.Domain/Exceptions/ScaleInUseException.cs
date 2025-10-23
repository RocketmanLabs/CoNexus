namespace CoNexus.Api.Domain.Exceptions;

public class ScaleInUseException : DomainException
{
	public ScaleInUseException(int scaleId)
		: base($"Scale with ID {scaleId} is in use and cannot be deleted") { }
}