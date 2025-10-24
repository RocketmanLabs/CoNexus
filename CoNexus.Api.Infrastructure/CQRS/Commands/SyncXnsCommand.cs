namespace CoNexus.Api.Infrastructure.CQRS.Commands;

public record SyncXnsCommand(List<XnSyncDto> Xns);
