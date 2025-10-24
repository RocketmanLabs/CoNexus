namespace CoNexus.Api.Infrastructure.CQRS.Commands;

public class GetUserReportQueryHandler
	{
		private readonly IXnRepository _userRepository;
		private readonly IResponseRepository _responseRepository;

		public GetUserReportQueryHandler(
			IXnRepository userRepository,
			IResponseRepository responseRepository)
		{
			_userRepository = userRepository;
			_responseRepository = responseRepository;
		}

		public async Task<UserReportDto> Handle(GetUserReportQuery query, CancellationToken cancellationToken)
		{
			var user = await _userRepository.GetByIdAsync(query.UserId, cancellationToken);
			if (user == null)
				return null;

			var responses = await _responseRepository.GetByUserAndPublicationAsync(
				query.UserId, query.PublicationId, cancellationToken);

			return new UserReportDto(
				user.Id,
				user.GetFullName(),
				query.PublicationId,
				responses.Select(r => new ResponseDto(
					r.Question.QuestionText,
					r.Question.Type,
					r.ResponseText,
					r.RespondedAt
				)).ToList()
			);
		}
	}
