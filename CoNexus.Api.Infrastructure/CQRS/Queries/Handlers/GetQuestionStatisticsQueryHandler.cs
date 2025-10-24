using CoNexus.Api.Domain.Constants;
using CoNexus.Api.Infrastructure.CQRS.DTOs;
using CoNexus.Api.Infrastructure.CQRS.Queries;
using CoNexus.Api.Infrastructure.Interfaces;

namespace CoNexus.Api.Infrastructure.CQRS.Queries.Handlers;

public class GetQuestionStatisticsQueryHandler
	{
		private readonly IResponseRepository _responseRepository;
		private readonly ISurveyRepository _surveyRepository;

		public GetQuestionStatisticsQueryHandler(
			IResponseRepository responseRepository,
			ISurveyRepository surveyRepository)
		{
			_responseRepository = responseRepository;
			_surveyRepository = surveyRepository;
		}

		public async Task<QuestionStatisticsDto?> Handle(GetQuestionStatisticsQuery query, CancellationToken cancellationToken)
		{
			var responses = await _responseRepository.GetByQuestionAndPublicationAsync(
				query.QuestionId, query.PublicationId, cancellationToken);

			if (!responses.Any())
				return null;

			var question = responses.First().Question;
			var n = responses.Count;

			List<string> mode = null;
			List<ChoiceFrequency> distribution = null;
			List<string> textResponses = null;

			if (question.Type == QuestionType.MULTIPLECHOICE)
			{
				var groups = responses.GroupBy(r => r.ResponseText)
					.Select(g => new ChoiceFrequency(
						g.Key,
						g.Count(),
						g.Count() * 100.0 / n
					))
					.OrderByDescending(cf => cf.Count)
					.ToList();

				distribution = groups;
				var maxCount = groups.Max(g => g.Count);
				mode = groups.Where(g => g.Count == maxCount).Select(g => g.Choice).ToList();
			}
			else
			{
				textResponses = responses.Select(r => r.ResponseText).ToList();
			}

			return new QuestionStatisticsDto(
				question.Id,
				question.QuestionText,
				question.Type,
				n,
				mode,
				distribution,
				textResponses
			);
		}
	}
