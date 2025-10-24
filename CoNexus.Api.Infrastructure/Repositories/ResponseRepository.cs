namespace CoNexus.Api.Infrastructure.Repositories;

public class ResponseRepository : IResponseRepository
{
	private readonly CnxDb _context;

	public ResponseRepository(CnxDb context)
	{
		_context = context;
	}

	public async Task<UserResponse> GetByUserQuestionPublicationAsync(
		int userId, int questionId, int publicationId, CancellationToken cancellationToken = default)
	{
		var urs = await _context.Votes
			.FirstOrDefaultAsync(ur =>
				ur.UserId == userId &&
				ur.QuestionId == questionId &&
				ur.PublicationId == publicationId,
				cancellationToken);
		if (urs == null)
			throw new InvalidOperationException($"User response not found for #{userId}, question #{questionId}, publication #{publicationId}.");
		return urs;
	}

	public async Task<List<UserResponse>> GetByUserAndPublicationAsync(
		int userId, int publicationId, CancellationToken cancellationToken = default)
	{
		return await _context.Votes
			.Include(ur => ur.Question)
			.Where(ur => ur.UserId == userId && ur.PublicationId == publicationId)
			.OrderBy(ur => ur.Question.OrderIndex)
			.ToListAsync(cancellationToken);
	}

	public async Task<List<UserResponse>> GetByPublicationAsync(
		int publicationId, CancellationToken cancellationToken = default)
	{
		return await _context.Votes
			.Include(ur => ur.User)
			.Include(ur => ur.Question)
			.Where(ur => ur.PublicationId == publicationId)
			.OrderBy(ur => ur.User.Email)
			.ThenBy(ur => ur.Question.OrderIndex)
			.ToListAsync(cancellationToken);
	}

	public async Task<List<UserResponse>> GetByQuestionAndPublicationAsync(
		int questionId, int publicationId, CancellationToken cancellationToken = default)
	{
		return await _context.Votes
			.Include(ur => ur.Question)
			.Where(ur => ur.QuestionId == questionId && ur.PublicationId == publicationId)
			.ToListAsync(cancellationToken);
	}

	public async Task AddAsync(UserResponse response, CancellationToken cancellationToken = default)
	{
		await _context.Votes.AddAsync(response, cancellationToken);
	}

	public Task UpdateAsync(UserResponse response, CancellationToken cancellationToken = default)
	{
		_context.Votes.Update(response);
		return Task.CompletedTask;
	}

	public async Task<int> CountDistinctUsersAsync(int publicationId, CancellationToken cancellationToken = default)
	{
		return await _context.Votes
			.Where(ur => ur.PublicationId == publicationId)
			.Select(ur => ur.UserId)
			.Distinct()
			.CountAsync(cancellationToken);
	}
}
