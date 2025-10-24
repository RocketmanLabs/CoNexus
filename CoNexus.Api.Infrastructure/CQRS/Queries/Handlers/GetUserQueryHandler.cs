namespace CoNexus.Api.Infrastructure.CQRS.Queries.Handlers;

public class GetXnQueryHandler
	{
		private readonly IXnRepository _xnRepository;

		public GetXnQueryHandler(IXnRepository xnRepository)
		{
			_xnRepository = xnRepository;
		}

		public async Task<UserDto> Handle(GetXnQuery query, CancellationToken cancellationToken)
		{
			var user = await _xnRepository.GetByIdAsync(query.XnId, cancellationToken);
			if (user == null)
				return null;

			return new XnDto(
				user.Id,
				user.ExternalCrmId,
				user.Email,
				user.FirstName,
				user.LastName,
				user.IsActive
			);
		}
	}
