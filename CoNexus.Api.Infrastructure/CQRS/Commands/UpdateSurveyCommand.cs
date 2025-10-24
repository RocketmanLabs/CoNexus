namespace CoNexus.Api.Infrastructure.CQRS.Commands;

public record UpdateSurveyCommand(
		int Id,
		string Title,
		string Description,
		bool? IsActive,
		List<CreateQuestionDto> Questions
	);
