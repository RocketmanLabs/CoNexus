// ============================================
// Commands and Queries (CQRS Pattern)
// ============================================

namespace CoNexus.Api.Infrastructure.CQRS.Queries
{
	public record GetSurveyStatisticsQuery(int SurveyId, int PublicationId);
}
