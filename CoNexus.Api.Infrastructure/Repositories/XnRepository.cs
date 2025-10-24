namespace CoNexus.Api.Infrastructure.Repositories;

// Repository Implementations
public class XnRepository : IXnRepository
{
	private readonly CnxDb _context;

	public XnRepository(CnxDb context) => _context = context;

	public async Task<Xn?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
	{
		return await _context.Xns.FindAsync(new object[] { id }, cancellationToken);
	}

	public async Task<Xn> GetByXIdAsync(string xid, CancellationToken cancellationToken = default)
	{
		return await _context.Xns
			.FirstOrDefaultAsync(u => u.ExternalCrmId == externalCrmId, cancellationToken);
	}

	public async Task<List<Xn>> GetByXIdsAsync(List<string> externalCrmIds, CancellationToken cancellationToken = default)
	{
		return await _context.Users
			.Where(u => externalCrmIds.Contains(u.ExternalCrmId))
			.ToListAsync(cancellationToken);
	}

	public async Task<List<Xn>> GetActiveUsersAsync(CancellationToken cancellationToken = default)
	{
		return await _context.Users
			.Where(u => u.IsActive)
			.ToListAsync(cancellationToken);
	}

	public async Task AddAsync(Xn user, CancellationToken cancellationToken = default)
	{
		await _context.Users.AddAsync(user, cancellationToken);
	}

	public Task UpdateAsync(Xn user, CancellationToken cancellationToken = default)
	{
		_context.Users.Update(user);
		return Task.CompletedTask;
	}

	public async Task<int> CountActiveUsersAsync(CancellationToken cancellationToken = default)
	{
		return await _context.Users.CountAsync(u => u.IsActive, cancellationToken);
	}
}
