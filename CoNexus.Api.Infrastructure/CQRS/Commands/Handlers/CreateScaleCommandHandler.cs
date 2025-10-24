namespace CoNexus.Api.Infrastructure.CQRS.Commands.Handlers;

public partial class CreateSurveyCommandHandler
{
	public class CreateScaleCommandHandler
	{
		private readonly IScaleRepository _scaleRepository;
		private readonly IUnitOfWork _unitOfWork;

		public CreateScaleCommandHandler(IScaleRepository scaleRepository, IUnitOfWork unitOfWork)
		{
			_scaleRepository = scaleRepository;
			_unitOfWork = unitOfWork;
		}

		public async Task<int> Handle(CreateScaleCommand cmd, CancellationToken cancellationToken)
		{
			var scale = Scale.Create(cmd.TenantId, cmd.Title, cmd.Shareable, cmd.Choices);



			await _scaleRepository.AddAsync(scale, cancellationToken);
			await _unitOfWork.SaveChangesAsync(cancellationToken);

			return scale.Id;
		}
	}
}