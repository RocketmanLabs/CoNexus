// ============================================
// Commands and Queries (CQRS Pattern)
// ============================================
using CoNexus.Api.Infrastructure.DTOs;

namespace CoNexus.Api.Infrastructure.CQRS;

public record SubmitResponsesCommand(
		int PublicationId,
		int UserId,
		List<ResponseAnswerDto> Answers
	);
