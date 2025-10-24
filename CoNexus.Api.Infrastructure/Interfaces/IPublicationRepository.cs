namespace CoNexus.Api.Infrastructure.Interfaces;

public interface IPublicationRepository
{
	Task<SurveyPublication> GetByIdAsync(int id, CancellationToken cancellationToken = default);
	Task<SurveyPublication> GetByIdWithSurveyAsync(int id, CancellationToken cancellationToken = default);
	Task AddAsync(SurveyPublication publication, CancellationToken cancellationToken = default);
	Task UpdateAsync(SurveyPublication publication, CancellationToken cancellationToken = default);
}
