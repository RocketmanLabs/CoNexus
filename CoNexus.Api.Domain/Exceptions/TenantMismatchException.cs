namespace CoNexus.Api.Domain.ExternalEntities;

public class TenantMismatchException : DomainException
{
	public TenantMismatchException(string message) : base(message) { }
}

#endregion
