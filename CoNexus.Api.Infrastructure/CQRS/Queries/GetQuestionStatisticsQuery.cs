// ============================================
// Commands and Queries (CQRS Pattern)
// ============================================

namespace CoNexus.Api.Infrastructure.CQRS.Queries
{
	public record GetQuestionStatisticsQuery(int QuestionId, int PublicationId);
}
