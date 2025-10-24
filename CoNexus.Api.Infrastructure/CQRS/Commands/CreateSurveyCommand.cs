namespace CoNexus.Api.Infrastructure.CQRS.Commands;

public record CreateSurveyCommand(
	string Title,
	string Description,
	List<CreateQuestionDto> Questions
); 
