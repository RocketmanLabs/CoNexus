using CoNexus.Api.Domain.Constants;

namespace CoNexus.Api.Infrastructure.DTOs;

public record CreateQuestionDto(
		string QuestionText,
		QuestionType Type,
		int OrderIndex,
		bool IsRequired,
		List<string> Choices
	);
