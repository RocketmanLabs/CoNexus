namespace CoNexus.Api.Infrastructure.DTOs;

public record XnDto(
		int TenantId, 
		int Id,
		string XId,
		XnType XnType
	);
