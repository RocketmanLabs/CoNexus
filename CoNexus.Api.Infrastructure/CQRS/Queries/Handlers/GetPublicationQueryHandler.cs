using CoNexus.Api.Infrastructure.CQRS.Queries;
using CoNexus.Api.Infrastructure.CQRS.DTOs;

public class GetPublicationQueryHandler
	{
		private readonly IPublicationRepository _publicationRepository;

		public GetPublicationQueryHandler(IPublicationRepository publicationRepository)
		{
			_publicationRepository = publicationRepository;
		}

		public async Task<PublicationDto> Handle(GetPublicationQuery query, CancellationToken cancellationToken)
		{
			var publication = await _publicationRepository.GetByIdAsync(query.PublicationId, cancellationToken);
			if (publication == null)
				return null;

			return new PublicationDto(
				publication.Id,
				publication.SurveyId,
				publication.PublicationName,
				publication.PublishedAt,
				publication.ClosedAt,
				publication.IsOpen
			);
		}
	}
