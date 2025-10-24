namespace CoNexus.Api.Infrastructure.DTOs.Requests;

public record UpdateScaleRequest(
	string Title,
	List<Choice> Choices
);

