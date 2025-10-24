// ============================================
// Commands and Queries (CQRS Pattern)
// ============================================

namespace CoNexus.Api.Infrastructure.CQRS.Queries
{
	public record GetUserResponsesQuery(int UserId, int PublicationId);
}
