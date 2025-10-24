namespace CoNexus.Api.Infrastructure.Interfaces;

public interface ISurveyRepository
	{
		Task<Survey> GetByIdAsync(int id, CancellationToken cancellationToken = default);
		Task<Survey> GetByIdWithQuestionsAsync(int id, CancellationToken cancellationToken = default);
		Task<List<Survey>> GetAllAsync(bool activeOnly = true, CancellationToken cancellationToken = default);
		Task AddAsync(Survey survey, CancellationToken cancellationToken = default);
		Task UpdateAsync(Survey survey, CancellationToken cancellationToken = default);
		Task DeleteAsync(Survey survey, CancellationToken cancellationToken = default);
	}
