namespace CoNexus.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SurveysController : ControllerBase
{
	private readonly CreateSurveyCommandHandler _createHandler;
	private readonly UpdateSurveyCommandHandler _updateHandler;
	private readonly DeleteSurveyCommandHandler _deleteHandler;
	private readonly GetSurveyQueryHandler _getSurveyHandler;
	private readonly GetAllSurveysQueryHandler _getAllSurveysHandler;

	public SurveysController(
		CreateSurveyCommandHandler createHandler,
		UpdateSurveyCommandHandler updateHandler,
		DeleteSurveyCommandHandler deleteHandler,
		GetSurveyQueryHandler getSurveyHandler,
		GetAllSurveysQueryHandler getAllSurveysHandler)
	{
		_createHandler = createHandler;
		_updateHandler = updateHandler;
		_deleteHandler = deleteHandler;
		_getSurveyHandler = getSurveyHandler;
		_getAllSurveysHandler = getAllSurveysHandler;
	}

	/// <summary>
	/// Get all surveys
	/// </summary>
	[HttpGet]
	[ProducesResponseType(200)]
	public async Task<IActionResult> GetAllSurveys([FromQuery] bool activeOnly = true, CancellationToken cancellationToken = default)
	{
		var query = new GetAllSurveysQuery(activeOnly);
		var surveys = await _getAllSurveysHandler.Handle(query, cancellationToken);
		return Ok(surveys);
	}

	/// <summary>
	/// Create a new survey
	/// </summary>
	[HttpPost]
	[ProducesResponseType(201)]
	public async Task<IActionResult> CreateSurvey([FromBody] CreateSurveyCommand command, CancellationToken cancellationToken)
	{
		try
		{
			var surveyId = await _createHandler.Handle(command, cancellationToken);
			var survey = await _getSurveyHandler.Handle(new GetSurveyQuery(surveyId), cancellationToken);
			return CreatedAtAction(nameof(GetSurvey), new { id = surveyId }, survey);
		}
		catch (ArgumentException ex)
		{
			return BadRequest(new { error = ex.Message });
		}
	}

	/// <summary>
	/// Get survey by ID
	/// </summary>
	[HttpGet("{id}")]
	[ProducesResponseType(200)]
	[ProducesResponseType(404)]
	public async Task<IActionResult> GetSurvey(int id, CancellationToken cancellationToken)
	{
		var query = new GetSurveyQuery(id);
		var survey = await _getSurveyHandler.Handle(query, cancellationToken);

		if (survey == null)
			return NotFound();

		return Ok(survey);
	}

	/// <summary>
	/// Update a survey
	/// </summary>
	[HttpPut("{id}")]
	[ProducesResponseType(200)]
	[ProducesResponseType(404)]
	public async Task<IActionResult> UpdateSurvey(int id, [FromBody] UpdateSurveyRequest request, CancellationToken cancellationToken)
	{
		try
		{
			var command = new UpdateSurveyCommand(
				id,
				request.Title,
				request.Description,
				request.IsActive,
				request.Questions
			);

			var surveyId = await _updateHandler.Handle(command, cancellationToken);
			var survey = await _getSurveyHandler.Handle(new GetSurveyQuery(surveyId), cancellationToken);
			return Ok(survey);
		}
		catch (SurveyNotFoundException)
		{
			return NotFound();
		}
		catch (ArgumentException ex)
		{
			return BadRequest(new { error = ex.Message });
		}
	}

	/// <summary>
	/// Delete (deactivate) a survey
	/// </summary>
	[HttpDelete("{id}")]
	[ProducesResponseType(204)]
	[ProducesResponseType(404)]
	public async Task<IActionResult> DeleteSurvey(int id, CancellationToken cancellationToken)
	{
		var command = new DeleteSurveyCommand(id);
		var result = await _deleteHandler.Handle(command, cancellationToken);

		if (!result)
			return NotFound();

		return NoContent();
	}
}
