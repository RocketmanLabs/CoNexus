namespace CoNexus.Api.Infrastructure.CQRS.Commands;

public record CreateScaleCommand(int TenantId,
								 string Title,
								 List<Choice> Choices,
								 bool Shareable = false);
