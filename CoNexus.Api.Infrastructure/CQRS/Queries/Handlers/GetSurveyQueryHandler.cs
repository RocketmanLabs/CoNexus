using CoNexus.Api.Infrastructure.CQRS.DTOs;

namespace CoNexus.Api.Infrastructure.CQRS.Queries.Handlers;

public class GetSurveyQueryHandler
	{
		private readonly ISurveyRepository _surveyRepository;

		public GetSurveyQueryHandler(ISurveyRepository surveyRepository)
		{
			_surveyRepository = surveyRepository;
		}

		public async Task<SurveyDto> Handle(GetSurveyQuery query, CancellationToken cancellationToken)
		{
			var survey = await _surveyRepository.GetByIdWithQuestionsAsync(query.SurveyId, cancellationToken);
			if (survey == null)
				return null;

			return MapToDto(survey);
		}

		private SurveyDto MapToDto(Survey survey)
		{
			return new SurveyDto(
				survey.Id,
				survey.Title,
				survey.Instructions,
				survey.IsActive,
				survey.Questions.OrderBy(q => q.Sequence).Select(q => new QuestionDto(
					q.Id,
					q.QuestionText,
					q.Type,
					q.Sequence,
					q.IsRequired,
					q.GetChoices()
				)).ToList()
			);
		}
	}
