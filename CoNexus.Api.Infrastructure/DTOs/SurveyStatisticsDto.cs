

// ============================================
// DTOs
// ============================================

namespace CoNexus.Api.Infrastructure.DTOs;

public record SurveyStatisticsDto(
		int SurveyId,
		string SurveyTitle,
		int PublicationId,
		string PublicationName,
		int TotalUsers,
		int RespondedUsers,
		double ResponseRate,
		List<QuestionStatisticsDto> QuestionStatistics
	);
