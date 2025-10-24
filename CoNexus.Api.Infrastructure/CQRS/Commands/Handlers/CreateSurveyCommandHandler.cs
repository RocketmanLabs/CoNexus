namespace CoNexus.Api.Infrastructure.CQRS.Commands.Handlers;

public partial class CreateSurveyCommandHandler
{
	private readonly ISurveyRepository _surveyRepository;
	private readonly IUnitOfWork _unitOfWork;

	public CreateSurveyCommandHandler(ISurveyRepository surveyRepository, IUnitOfWork unitOfWork)
	{
		_surveyRepository = surveyRepository;
		_unitOfWork = unitOfWork;
	}

	public async Task<int> Handle(CreateSurveyCommand command, CancellationToken cancellationToken)
	{
		var survey = Survey.Create(command.Title, command.Description);

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

		await _surveyRepository.AddAsync(survey, cancellationToken);
		await _unitOfWork.SaveChangesAsync(cancellationToken);

		return survey.Id;
	}
}