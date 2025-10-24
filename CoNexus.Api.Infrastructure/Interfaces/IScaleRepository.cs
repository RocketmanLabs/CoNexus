namespace CoNexus.Api.Infrastructure.Interfaces;

public interface IScaleRepository
{
	Task<Scale> GetByIdAsync(int id, CancellationToken cancellationToken = default);
	Task<List<Scale>> GetAllAsync(CancellationToken cancellationToken = default);
	Task<List<Scale>> GetSharedAsync(CancellationToken cancellationToken = default);
	Task<List<Scale>> GetWithUsageAsync(CancellationToken cancellationToken = default);
	Task AddAsync(Scale scale, CancellationToken cancellationToken = default);
	Task UpdateAsync(Scale scale, CancellationToken cancellationToken = default);
	Task DeleteAsync(Scale scale, CancellationToken cancellationToken = default);
	Task<bool> IsInUseAsync(int scaleId, CancellationToken cancellationToken = default);
}
