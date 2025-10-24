namespace CoNexus.Api.Infrastructure.CQRS.Queries.Handlers;

public class GetUserProgressQueryHandler
	{
		private readonly IPublicationRepository _publicationRepository;
		private readonly IResponseRepository _responseRepository;

		public GetUserProgressQueryHandler(
			IPublicationRepository publicationRepository,
			IResponseRepository responseRepository)
		{
			_publicationRepository = publicationRepository;
			_responseRepository = responseRepository;
		}

		public async Task<UserProgressDto> Handle(GetUserProgressQuery query, CancellationToken cancellationToken)
		{
			var publication = await _publicationRepository.GetByIdWithSurveyAsync(query.PublicationId, cancellationToken);
			if (publication == null)
				return null;

			var responses = await _responseRepository.GetByUserAndPublicationAsync(
				query.UserId, query.PublicationId, cancellationToken);

			var answeredQuestionIds = responses.Select(r => r.QuestionId).ToHashSet();
			var totalQuestions = publication.Survey.Questions.Count;
			var answeredCount = answeredQuestionIds.Count;

			return new UserProgressDto(
				totalQuestions,
				answeredCount,
				totalQuestions > 0 ? answeredCount * 100.0 / totalQuestions : 0,
				publication.Survey.Questions
					.Where(q => !answeredQuestionIds.Contains(q.Id))
					.Select(q => q.Id)
					.ToList()
			);
		}
	}
