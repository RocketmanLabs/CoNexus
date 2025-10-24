namespace CoNexus.Api.Infrastructure.Interfaces;

public interface ICsvExportService
{
	Task<string> ExportPublicationResponsesAsync(int publicationId, CancellationToken cancellationToken = default);
}

