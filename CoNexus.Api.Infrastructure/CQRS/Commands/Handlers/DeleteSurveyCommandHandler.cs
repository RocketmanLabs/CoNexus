namespace CoNexus.Api.Infrastructure.CQRS.Commands.Handlers;

public class DeleteSurveyCommandHandler
{
	private readonly ISurveyRepository _surveyRepository;
	private readonly IUnitOfWork _unitOfWork;

	public DeleteSurveyCommandHandler(ISurveyRepository surveyRepository, IUnitOfWork unitOfWork)
	{
		_surveyRepository = surveyRepository;
		_unitOfWork = unitOfWork;
	}

	public async Task<bool> Handle(DeleteSurveyCommand command, CancellationToken cancellationToken)
	{
		var survey = await _surveyRepository.GetByIdAsync(command.Id, cancellationToken);
		if (survey == null)
			return false;

		survey.Deactivate();
		await _surveyRepository.UpdateAsync(survey, cancellationToken);
		await _unitOfWork.SaveChangesAsync(cancellationToken);

		return true;
	}
}
