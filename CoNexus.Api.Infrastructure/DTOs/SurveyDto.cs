

// ============================================
// DTOs
// ============================================

namespace CoNexus.Api.Infrastructure.DTOs;

public record SurveyDto(
		int Id,
		string Title,
		string Description,
		bool IsActive,
		List<QuestionDto> Questions
	);
