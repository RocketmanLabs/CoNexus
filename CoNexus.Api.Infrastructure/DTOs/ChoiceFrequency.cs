namespace CoNexus.Api.Infrastructure.DTOs;

public record ChoiceFrequency(
		string Choice,
		int Count,
		double Percentage
	);
