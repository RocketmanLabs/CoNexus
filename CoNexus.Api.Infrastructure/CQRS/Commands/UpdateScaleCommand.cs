namespace CoNexus.Api.Infrastructure.CQRS.Commands;

public record UpdateScaleCommand(
		int Id,
		string Title,
		List<ChoiceDto> Choices
	);
