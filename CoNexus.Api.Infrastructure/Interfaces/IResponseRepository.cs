namespace CoNexus.Api.Infrastructure.Interfaces;

public interface IResponseRepository
	{
		Task<UserResponse> GetByUserQuestionPublicationAsync(int userId, int questionId, int publicationId, CancellationToken cancellationToken = default);
		Task<List<UserResponse>> GetByUserAndPublicationAsync(int userId, int publicationId, CancellationToken cancellationToken = default);
		Task<List<UserResponse>> GetByPublicationAsync(int publicationId, CancellationToken cancellationToken = default);
		Task<List<UserResponse>> GetByQuestionAndPublicationAsync(int questionId, int publicationId, CancellationToken cancellationToken = default);
		Task AddAsync(UserResponse response, CancellationToken cancellationToken = default);
		Task UpdateAsync(UserResponse response, CancellationToken cancellationToken = default);
		Task<int> CountDistinctUsersAsync(int publicationId, CancellationToken cancellationToken = default);
	}
