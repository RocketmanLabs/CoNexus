// ============================================
// Commands and Queries (CQRS Pattern)
// ============================================

namespace CoNexus.Api.Infrastructure.CQRS.Queries
{
	public record GetAllSurveysQuery(bool ActiveOnly = true);
}
