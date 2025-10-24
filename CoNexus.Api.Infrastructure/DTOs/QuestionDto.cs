using CoNexus.Api.Domain.Constants;

namespace CoNexus.Api.Infrastructure.DTOs;

public record QuestionDto(
		int Id,
		string QuestionText,
		QuestionType Type,
		int OrderIndex,
		bool IsRequired,
		List<string> Choices
	);
