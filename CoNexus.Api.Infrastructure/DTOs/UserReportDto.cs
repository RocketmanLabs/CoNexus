

// ============================================
// DTOs
// ============================================

namespace CoNexus.Api.Infrastructure.DTOs;

public record UserReportDto(
		int UserId,
		string UserName,
		int PublicationId,
		List<ResponseDto> Responses
	);
