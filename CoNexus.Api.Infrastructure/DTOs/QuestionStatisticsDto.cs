// ============================================
// DTOs
// ============================================

using CoNexus.Api.Domain.Constants;

namespace CoNexus.Api.Infrastructure.DTOs;

public record QuestionStatisticsDto(
		int QuestionId,
		string QuestionText,
		QuestionType QuestionType,
		int N,
		List<string> Mode,
		List<ChoiceFrequency> ChoiceDistribution,
		List<string> TextResponses
	);
