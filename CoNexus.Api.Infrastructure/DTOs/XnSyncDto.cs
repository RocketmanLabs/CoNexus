namespace CoNexus.Api.Infrastructure.DTOs;

public record XnSyncDto(
	string ExternalCrmId,
	string Email,
	string FirstName,
	string LastName,
	bool IsActive
);
