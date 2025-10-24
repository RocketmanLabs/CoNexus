namespace CoNexus.Api.Infrastructure.CQRS.Commands.Handlers;

public class UpdateSurveyCommandHandler
{
	private readonly ISurveyRepository _surveyRepository;
	private readonly IUnitOfWork _unitOfWork;

	public UpdateSurveyCommandHandler(ISurveyRepository surveyRepository, IUnitOfWork unitOfWork)
	{
		_surveyRepository = surveyRepository;
		_unitOfWork = unitOfWork;
	}

	public async Task<int> Handle(UpdateSurveyCommand command, CancellationToken cancellationToken)
	{
		var survey = await _surveyRepository.GetByIdWithQuestionsAsync(command.Id, cancellationToken);
		if (survey == null)
			throw new SurveyNotFoundException(command.Id);

		survey.Update(command.Title, command.Description);

		if (command.IsActive.HasValue && !command.IsActive.Value)
			survey.Deactivate();

		// Replace questions
		survey.RemoveAllQuestions();
		foreach (var questionDto in command.Questions.OrderBy(q => q.OrderIndex))
		{
			survey.AddQuestion(
				questionDto.QuestionText,
				questionDto.Type,
				questionDto.OrderIndex,
				questionDto.IsRequired,
				questionDto.Choices
			);
		}

		await _surveyRepository.UpdateAsync(survey, cancellationToken);
		await _unitOfWork.SaveChangesAsync(cancellationToken);

		return survey.Id;
	}
}

public class UpdateScaleCommandHandler
{
	private readonly IScaleRepository _scaleRepository;
	private readonly IUnitOfWork _unitOfWork;

	public UpdateScaleCommandHandler(IScaleRepository scaleRepository, IUnitOfWork unitOfWork)
	{
		_scaleRepository = scaleRepository;
		_unitOfWork = unitOfWork;
	}

	public async Task<int> Handle(UpdateScaleCommand command, CancellationToken cancellationToken)
	{
		var scale = await _scaleRepository.GetByIdAsync(command.Id, cancellationToken);
		if (scale is null) throw new ScaleNotFoundException(command.Id);

		scale.Update(command.Title);

		if (command.IsActive.HasValue && !command.IsActive.Value) scale.Deactivate();

		// Replace questions
		scale.RemoveAllQuestions();
		foreach (var dto in command.Choices.OrderBy(q => q.Sequence))
		{
			scale.AddChoice(dto.Text, dto.Sequence, dto.Value);
		}

		await _scaleRepository.UpdateAsync(scale, cancellationToken);
		await _unitOfWork.SaveChangesAsync(cancellationToken);

		return scale.Id;
	}
}
