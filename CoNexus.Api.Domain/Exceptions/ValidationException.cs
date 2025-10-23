namespace CoNexus.Api.Domain.Exceptions;

public class ValidationException : DomainException
{
    public List<string> Errors { get; }

    public ValidationException(List<string> errors) 
        : base("Validation failed")
    {
        Errors = errors;
    }

    public ValidationException(string error) 
        : base("Validation failed")
    {
        Errors = new List<string> { error };
    }
}