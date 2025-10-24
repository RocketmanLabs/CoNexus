namespace CoNexus.Api.Infrastructure.Repositories;

public class PublicationRepository : IPublicationRepository
{
	private readonly CnxDb _context;

	public PublicationRepository(CnxDb context)
	{
		_context = context;
	}

	public async Task<SurveyPublication> GetByIdAsync(int id, CancellationToken cancellationToken = default)
	{
		// Throws if not found, matching interface signature (non-nullable return)
		var publication = await _context.SurveyPublications.FindAsync(new object[] { id }, cancellationToken);
		if (publication == null)
			throw new InvalidOperationException($"SurveyPublication #{id} not found.");
		return publication;
	}

	public async Task<SurveyPublication> GetByIdWithSurveyAsync(int id, CancellationToken cancellationToken = default)
	{
		var publication = await _context.SurveyPublications
			.Include(sp => sp.Survey)
			.ThenInclude(s => s.Questions)
			.FirstAsync(sp => sp.Id == id, cancellationToken);
		if (publication == null)
			throw new InvalidOperationException($"SurveyPublication #{id} not found.");
		return publication;
	}

	public async Task AddAsync(SurveyPublication publication, CancellationToken cancellationToken = default)
	{
		await _context.SurveyPublications.AddAsync(publication, cancellationToken);
	}

	public Task UpdateAsync(SurveyPublication publication, CancellationToken cancellationToken = default)
	{
		_context.SurveyPublications.Update(publication);
		return Task.CompletedTask;
	}
}
