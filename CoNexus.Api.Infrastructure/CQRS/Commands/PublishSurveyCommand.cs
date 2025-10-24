// ============================================
// Commands and Queries (CQRS Pattern)
// ============================================

namespace CoNexus.Api.Infrastructure.CQRS;

public record PublishSurveyCommand(int SurveyId, string PublicationName);
