namespace CoNexus.Api.Infrastructure.CQRS.Queries.Handlers;

public class GetSurveyStatisticsQueryHandler
	{
		private readonly ISurveyRepository _surveyRepository;
		private readonly IPublicationRepository _publicationRepository;
		private readonly IResponseRepository _responseRepository;
		private readonly IXnRepository _userRepository;
		private readonly GetQuestionStatisticsQueryHandler _questionStatsHandler;

		public GetSurveyStatisticsQueryHandler(
			ISurveyRepository surveyRepository,
			IPublicationRepository publicationRepository,
			IResponseRepository responseRepository,
			IXnRepository userRepository,
			GetQuestionStatisticsQueryHandler questionStatsHandler)
		{
			_surveyRepository = surveyRepository;
			_publicationRepository = publicationRepository;
			_responseRepository = responseRepository;
			_userRepository = userRepository;
			_questionStatsHandler = questionStatsHandler;
		}

		public async Task<SurveyStatisticsDto> Handle(GetSurveyStatisticsQuery query, CancellationToken cancellationToken)
		{
			var survey = await _surveyRepository.GetByIdWithQuestionsAsync(query.SurveyId, cancellationToken);
			if (survey == null)
				return null;

			var publication = await _publicationRepository.GetByIdAsync(query.PublicationId, cancellationToken);
			if (publication == null || publication.SurveyId != query.SurveyId)
				return null;

			var totalUsers = await _userRepository.CountActiveUsersAsync(cancellationToken);
			var respondedUsers = await _responseRepository.CountDistinctUsersAsync(query.PublicationId, cancellationToken);

			var questionStats = new List<QuestionStatisticsDto>();
			foreach (var question in survey.Questions.OrderBy(q => q.Sequence))
			{
				var stat = await _questionStatsHandler.Handle(
					new GetQuestionStatisticsQuery(question.Id, query.PublicationId),
					cancellationToken
				);
				if (stat != null)
					questionStats.Add(stat);
			}

			return new SurveyStatisticsDto(
				survey.Id,
				survey.Title,
				publication.Id,
				publication.PublicationName,
				totalUsers,
				respondedUsers,
				totalUsers > 0 ? respondedUsers * 100.0 / totalUsers : 0,
				questionStats
			);
		}
	}
