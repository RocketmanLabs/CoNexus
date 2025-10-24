

// ============================================
// DTOs
// ============================================

namespace CoNexus.Api.Infrastructure.DTOs;

public record UserProgressDto(
		int TotalQuestions,
		int AnsweredQuestions,
		double PercentComplete,
		List<int> UnansweredQuestionIds
	);
