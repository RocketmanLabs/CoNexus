namespace CoNexus.Api.Infrastructure.DTOs; 

public record ScaleDto(
	int Id,
	int TenantId,
	string Title,
	List<ChoiceDto> Choices,
	bool Shareable,
	int QuestionCount
);
