

namespace CoNexus.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScalesController : ControllerBase
{
	private readonly CreateScaleCommandHandler _createHandler;
	private readonly UpdateScaleCommandHandler _updateHandler;
	private readonly DeleteScaleCommandHandler _deleteHandler;
	private readonly GetScaleQueryHandler _getScaleHandler;
	private readonly GetAllScalesQueryHandler _getSharedScalesHandler;
	private readonly GetSharedScalesQueryHandler _getAllScalesHandler;

	public ScalesController(
		CreateScaleCommandHandler createHandler,
		UpdateScaleCommandHandler updateHandler,
		DeleteScaleCommandHandler deleteHandler,
		GetScaleQueryHandler getScaleHandler,
		GetAllScalesQueryHandler getAllScalesHandler)
	{
		_createHandler = createHandler;
		_updateHandler = updateHandler;
		_deleteHandler = deleteHandler;
		_getScaleHandler = getScaleHandler;
		_getAllScalesHandler = getAllScalesHandler;
	}

	/// <summary>
	/// Create a new scale with choices
	/// </summary>
	[HttpPost]
	[ProducesResponseType(201)]
	[ProducesResponseType(400)]
	public async Task<IActionResult> CreateScale([FromBody] CreateScaleCommand command, CancellationToken cancellationToken)
	{
		try
		{
			var scaleId = await _createHandler.Handle(command, cancellationToken);
			var scale = await _getScaleHandler.Handle(new GetScaleQuery(scaleId), cancellationToken);
			return CreatedAtAction(nameof(GetScale), new { id = scaleId }, scale);
		}
		catch (ArgumentException ex)
		{
			return BadRequest(new { error = ex.Message });
		}
	}

	/// <summary>
	/// Get scale by ID with choices
	/// </summary>
	[HttpGet("{id}")]
	[ProducesResponseType(200)]
	[ProducesResponseType(404)]
	public async Task<IActionResult> GetScale(int id, CancellationToken cancellationToken)
	{
		var query = new GetScaleQuery(id);
		var scale = await _getScaleHandler.Handle(query, cancellationToken);

		if (scale == null)
			return NotFound();

		return Ok(scale);
	}

	/// <summary>
	/// Get all scales
	/// </summary>
	[HttpGet]
	[ProducesResponseType(200)]
	public async Task<IActionResult> GetAllScales([FromQuery] bool includeUsageCount = false, CancellationToken cancellationToken)
	{
		var query = new GetAllScalesQuery(includeUsageCount);
		var scales = await _getAllScalesHandler.Handle(query, cancellationToken);
		return Ok(scales);
	}

	/// <summary>
	/// Update a scale and its choices
	/// </summary>
	[HttpPut("{id}")]
	[ProducesResponseType(200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(400)]
	public async Task<IActionResult> UpdateScale(int id, [FromBody] UpdateScaleRequest request, CancellationToken cancellationToken)
	{
		try
		{
			var command = new UpdateScaleCommand(id, request.Title, request.Choices);
			var scaleId = await _updateHandler.Handle(command, cancellationToken);
			var scale = await _getScaleHandler.Handle(new GetScaleQuery(scaleId), cancellationToken);
			return Ok(scale);
		}
		catch (ScaleNotFoundException)
		{
			return NotFound();
		}
		catch (ArgumentException ex)
		{
			return BadRequest(new { error = ex.Message });
		}
	}

	/// <summary>
	/// Delete a scale (only if not in use)
	/// </summary>
	[HttpDelete("{id}")]
	[ProducesResponseType(204)]
	[ProducesResponseType(404)]
	[ProducesResponseType(400)]
	public async Task<IActionResult> DeleteScale(int id, CancellationToken cancellationToken)
	{
		try
		{
			var command = new DeleteScaleCommand(id);
			var result = await _deleteHandler.Handle(command, cancellationToken);

			if (!result)
				return NotFound();

			return NoContent();
		}
		catch (ScaleInUseException ex)
		{
			return BadRequest(new { error = ex.Message });
		}
	}
}
