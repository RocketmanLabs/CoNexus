namespace CoNexus.Api.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
	private readonly CnxDb _context;
	private IDbContextTransaction _transaction = default!;

	public UnitOfWork(CnxDb context)
	{
		_context = context;
	}

	public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
	{
		return await _context.SaveChangesAsync(cancellationToken);
	}

	public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
	{
		_transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
	}

	public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			await _context.SaveChangesAsync(cancellationToken);
			await _transaction.CommitAsync(cancellationToken);
		}
		catch
		{
			await RollbackTransactionAsync(cancellationToken);
			throw;
		}
		finally
		{
			_transaction.Dispose();
			_transaction = default!;
		}
	}

	public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
	{
		await _transaction.RollbackAsync(cancellationToken);
		_transaction?.Dispose();
		_transaction = default!;
	}
}
