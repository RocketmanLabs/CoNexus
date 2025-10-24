namespace CoNexus.Api.Infrastructure.Repositories;

public class ScaleRepository : IScaleRepository
{
	private readonly CnxDb _context;

	public ScaleRepository(CnxDb context)
	{
		_context = context;
	}

	public async Task<Scale> GetByIdAsync(int id, CancellationToken cancellationToken = default)
	{
		var scale = await _context.Scales
			.Include(s => s.Choices.OrderBy(c => c.Sequence))
			.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
		if (scale is null)
			throw new InvalidOperationException($"Scale #{id} not found.");
		return scale;
	}

	public async Task<List<Scale>> GetAllAsync(CancellationToken cancellationToken = default)
	{
		return await _context.Scales
			.Include(s => s.Choices.OrderBy(c => c.Sequence))
			.OrderBy(s => s.Title)
			.ToListAsync(cancellationToken);
	}

	public async Task<List<Scale>> GetSharedAsync(CancellationToken cancellationToken = default)
	{
		return await _context.Scales
			.Include(s => s.Choices.OrderBy(c => c.Sequence))
			.OrderBy(s => s.Title)
			.ToListAsync(cancellationToken);
	}

	public async Task<List<Scale>> GetWithUsageAsync(CancellationToken cancellationToken = default)
	{
		return await _context.Scales
			.Include(s => s.Choices.OrderBy(c => c.Sequence))
			.Include(s => s.Questions)
			.OrderBy(s => s.Title)
			.ToListAsync(cancellationToken);
	}

	public async Task AddAsync(Scale scale, CancellationToken cancellationToken = default)
	{
		await _context.Scales.AddAsync(scale, cancellationToken);
	}

	public Task UpdateAsync(Scale scale, CancellationToken cancellationToken = default)
	{
		_context.Scales.Update(scale);
		return Task.CompletedTask;
	}

	public Task DeleteAsync(Scale scale, CancellationToken cancellationToken = default)
	{
		_context.Scales.Remove(scale);
		return Task.CompletedTask;
	}

	public async Task<bool> IsInUseAsync(int scaleId, CancellationToken cancellationToken = default)
	{
		return await _context.Questions.AnyAsync(q => q.ScaleId == scaleId, cancellationToken);
	}
}
}
