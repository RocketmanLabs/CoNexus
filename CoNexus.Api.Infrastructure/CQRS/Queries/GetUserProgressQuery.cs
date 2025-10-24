// ============================================
// Commands and Queries (CQRS Pattern)
// ============================================

namespace CoNexus.Api.Infrastructure.CQRS.Queries
{
	public record GetUserProgressQuery(int UserId, int PublicationId);
}
