namespace CoNexus.Api.Infrastructure.Interfaces;

public interface IXnRepository
{
	Task<Xn?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
	Task<Xn?> GetByXIdAsync(string xid, CancellationToken cancellationToken = default);
	Task<List<Xn>> GetByXIdsAsync(List<string> xids, CancellationToken cancellationToken = default);
	Task<List<Xn>> GetActiveXnsAsync(CancellationToken cancellationToken = default);
	Task AddAsync(Xn user, CancellationToken cancellationToken = default);
	Task UpdateAsync(Xn user, CancellationToken cancellationToken = default);
	Task<int> CountAsync(CancellationToken cancellationToken = default);
}
