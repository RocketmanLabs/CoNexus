using CoNexus.Api.Infrastructure.CQRS.DTOs;

namespace CoNexus.Api.Infrastructure.CQRS.Queries.Handlers;

public class GetAllSurveysQueryHandler
{
	private readonly ISurveyRepository _surveyRepository;

	public GetAllSurveysQueryHandler(ISurveyRepository surveyRepository)
	{
		_surveyRepository = surveyRepository;
	}

	public async Task<List<SurveyDto>> Handle(GetAllSurveysQuery query, CancellationToken cancellationToken)
	{
		var surveys = await _surveyRepository.GetAllAsync(query.ActiveOnly, cancellationToken);

		return surveys.Select(s => new SurveyDto(
			s.Id,
			s.Title,
			s.Instructions,
			s.IsActive,
			s.Questions.OrderBy(q => q.Sequence).Select(q => new QuestionDto(
				q.Id,
				q.QuestionText,
				q.Type,
				q.Sequence,
				q.IsRequired,
				q.GetChoices()
			)).ToList()
		)).ToList();
	}
}
