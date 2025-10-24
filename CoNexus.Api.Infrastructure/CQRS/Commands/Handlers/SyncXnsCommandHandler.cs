namespace CoNexus.Api.Infrastructure.CQRS.Commands.Handlers;

public class SyncXnsCommandHandler
{
	private readonly IXnRepository _xnRepository;
	private readonly IUnitOfWork _unitOfWork;

	public SyncXnsCommandHandler(IXnRepository xnRepository, IUnitOfWork unitOfWork)
	{
		_xnRepository = xnRepository;
		_unitOfWork = unitOfWork;
	}

	public async Task<SyncXnsResult> Handle(SyncXnsCommand command, CancellationToken cancellationToken)
	{
		int created = 0, updated = 0, deactivated = 0;

		var externalIds = command.Xns.Select(u => u.ExternalCrmId).ToList();
		var existingXns = await _xnRepository.GetByXIdsAsync(externalIds, cancellationToken);
		var existingXnDict = existingXns.ToDictionary(u => u.ExternalCrmId);

		foreach (var xnDto in command.Xns)
		{
			if (existingXnDict.TryGetValue(xnDto.ExternalCrmId, out var existingXn))
			{
				existingXn.UpdateInfo(xnDto.Email, xnDto.FirstName, xnDto.LastName);

				if (xnDto.IsActive && !existingXn.IsActive)
					existingXn.Activate();
				else if (!xnDto.IsActive && existingXn.IsActive)
				{
					existingXn.Deactivate();
					deactivated++;
				}

				await _xnRepository.UpdateAsync(existingXn, cancellationToken);
				updated++;
			}
			else
			{
				var newXn = Xn.Create(
					xnDto.ExternalCrmId,
					xnDto.Email,
					xnDto.FirstName,
					xnDto.LastName
				);

				if (!xnDto.IsActive)
					newXn.Deactivate();

				await _xnRepository.AddAsync(newXn, cancellationToken);
				created++;
			}
		}

		await _unitOfWork.SaveChangesAsync(cancellationToken);
		return new SyncXnsResult(created, updated, deactivated);
	}
}