// ============================================
// CoNexus.Api - Presentation Layer (Controllers)
// ============================================

namespace CoNexus.Api.Infrastructure.DTOs.Requests;

// Request models (for API)
public record UpdateSurveyRequest(
	string Title,
	string Description,
	bool? IsActive,
	List<CreateQuestionDto> Questions
);

