namespace CoNexus.Api.Infrastructure.Results;

public record SubmitResponsesResult(bool DataRetrieved, List<string> Errors);
