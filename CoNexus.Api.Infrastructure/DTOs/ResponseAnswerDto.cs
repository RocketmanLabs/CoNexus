// ============================================
// Commands and Queries (CQRS Pattern)
// ============================================

namespace CoNexus.Api.Infrastructure.DTOs;

public record ResponseAnswerDto(int QuestionId, string Answer);
