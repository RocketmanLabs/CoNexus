namespace CoNexus.Api.Infrastructure.CQRS.Commands.Handlers;

public class SubmitResponsesCommandHandler
{
	private readonly IXnRepository _userRepository;
	private readonly IPublicationRepository _publicationRepository;
	private readonly IResponseRepository _responseRepository;
	private readonly IUnitOfWork _unitOfWork;

	public SubmitResponsesCommandHandler(
		IXnRepository userRepository,
		IPublicationRepository publicationRepository,
		IResponseRepository responseRepository,
		IUnitOfWork unitOfWork)
	{
		_userRepository = userRepository;
		_publicationRepository = publicationRepository;
		_responseRepository = responseRepository;
		_unitOfWork = unitOfWork;
	}

	public async Task<SubmitResponsesResult> Handle(SubmitResponsesCommand command, CancellationToken cancellationToken)
	{
		var errors = new List<string>();

		// Validate user
		var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);
		if (user == null)
		{
			errors.Add("User not found");
			return new SubmitResponsesResult(false, errors);
		}

		// Validate publication
		var publication = await _publicationRepository.GetByIdWithSurveyAsync(command.PublicationId, cancellationToken);
		if (publication == null)
		{
			errors.Add("Publication not found");
			return new SubmitResponsesResult(false, errors);
		}

		if (!publication.IsOpen)
		{
			errors.Add("Publication is closed");
			return new SubmitResponsesResult(false, errors);
		}

		// Validate required questions
		var requiredQuestions = publication.Survey.Questions.Where(q => q.IsRequired).ToList();
		var answeredQuestionIds = command.Answers.Select(a => a.QuestionId).ToHashSet();

		foreach (var requiredQuestion in requiredQuestions)
		{
			if (!answeredQuestionIds.Contains(requiredQuestion.Id))
			{
				errors.Add($"Required question not answered: {requiredQuestion.QuestionText}");
			}
		}

		if (errors.Any())
			return new SubmitResponsesResult(false, errors);

		// Validate and save responses
		foreach (var answer in command.Answers)
		{
			var question = publication.Survey.Questions.FirstOrDefault(q => q.Id == answer.QuestionId);
			if (question == null)
			{
				errors.Add($"Invalid question ID: {answer.QuestionId}");
				continue;
			}

			if (!question.IsValidAnswer(answer.Answer))
			{
				errors.Add($"Invalid answer for question: {question.QuestionText}");
				continue;
			}

			// Check if response already exists (update or create)
			var existingResponse = await _responseRepository.GetByUserQuestionPublicationAsync(
				command.UserId, answer.QuestionId, command.PublicationId, cancellationToken);

			if (existingResponse != null)
			{
				existingResponse.UpdateResponse(answer.Answer);
				await _responseRepository.UpdateAsync(existingResponse, cancellationToken);
			}
			else
			{
				var newResponse = UserResponse.Create(
					command.UserId,
					answer.QuestionId,
					command.PublicationId,
					answer.Answer
				);
				await _responseRepository.AddAsync(newResponse, cancellationToken);
			}
		}

		await _unitOfWork.SaveChangesAsync(cancellationToken);
		return new SubmitResponsesResult(errors.Count == 0, errors);
	}
}
