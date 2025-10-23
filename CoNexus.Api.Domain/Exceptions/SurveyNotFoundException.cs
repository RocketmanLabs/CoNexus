namespace CoNexus.Api.Domain.Exceptions;

public class SurveyNotFoundException : DomainException
{
	public SurveyNotFoundException(int surveyId)
		: base($"Survey with ID {surveyId} was not found") { }
}