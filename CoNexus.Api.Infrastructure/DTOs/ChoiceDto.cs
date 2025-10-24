namespace CoNexus.Api.Infrastructure.DTOs;

public record ChoiceDto(
	int Id,
	string Text,
	int Sequence,
	int Value
);
