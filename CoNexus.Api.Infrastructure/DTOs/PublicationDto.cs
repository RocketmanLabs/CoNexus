namespace CoNexus.Api.Infrastructure.DTOs;

public record PublicationDto(
		int Id,
		int SurveyId,
		string PublicationName,
		DateTime PublishedAt,
		DateTime? ClosedAt,
		bool IsOpen
	);
