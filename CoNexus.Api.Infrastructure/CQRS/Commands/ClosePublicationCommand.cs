// ============================================
// Commands and Queries (CQRS Pattern)
// ============================================

namespace CoNexus.Api.Infrastructure.CQRS.Commands;

public record ClosePublicationCommand(int PublicationId);
