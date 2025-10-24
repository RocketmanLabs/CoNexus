namespace CoNexus.Api.Domain.Exceptions;

public class OrchestrationException : DomainException
{
	public OrchestrationException(string name, string msg)
		: base($"Orchestrator had a problem executing '{name}': {msg}") { }
}