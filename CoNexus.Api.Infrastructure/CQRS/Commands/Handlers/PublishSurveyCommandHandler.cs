namespace CoNexus.Api.Infrastructure.CQRS.Commands.Handlers;

public class PublishSurveyCommandHandler
{
	private readonly ISurveyRepository _surveyRepository;
	private readonly IPublicationRepository _publicationRepository;
	private readonly IUnitOfWork _unitOfWork;

	public PublishSurveyCommandHandler(
		ISurveyRepository surveyRepository,
		IPublicationRepository publicationRepository,
		IUnitOfWork unitOfWork)
	{
		_surveyRepository = surveyRepository;
		_publicationRepository = publicationRepository;
		_unitOfWork = unitOfWork;
	}

	public async Task<int> Handle(PublishSurveyCommand command, CancellationToken cancellationToken)
	{
		var survey = await _surveyRepository.GetByIdAsync(command.SurveyId, cancellationToken);
		if (survey == null)
			throw new SurveyNotFoundException(command.SurveyId);

		var publication = survey.Publish(command.PublicationName);

		await _publicationRepository.AddAsync(publication, cancellationToken);
		await _unitOfWork.SaveChangesAsync(cancellationToken);

		return publication.Id;
	}
}
