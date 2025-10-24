namespace CoNexus.Api.Infrastructure.CQRS.Commands.Handlers;

public class ClosePublicationCommandHandler
{
	private readonly IPublicationRepository _publicationRepository;
	private readonly IUnitOfWork _unitOfWork;

	public ClosePublicationCommandHandler(
		IPublicationRepository publicationRepository,
		IUnitOfWork unitOfWork)
	{
		_publicationRepository = publicationRepository;
		_unitOfWork = unitOfWork;
	}

	public async Task<bool> Handle(ClosePublicationCommand command, CancellationToken cancellationToken)
	{
		var publication = await _publicationRepository.GetByIdAsync(command.PublicationId, cancellationToken);
		if (publication == null)
			return false;

		publication.Close();
		await _publicationRepository.UpdateAsync(publication, cancellationToken);
		await _unitOfWork.SaveChangesAsync(cancellationToken);

		return true;
	}
}