// ============================================
// CoNexus.Api - Presentation Layer (Controllers)
// ============================================

using Microsoft.AspNetCore.Mvc;
using SurveyApi.Application.Commands;
using SurveyApi.Application.Handlers.Queries;
using SurveyApi.Application.Queries;

namespace CoNexus.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResponsesController : ControllerBase
{
	private readonly SubmitResponsesCommandHandler _submitHandler;
	private readonly GetUserProgressQueryHandler _progressHandler;

	public ResponsesController(
		SubmitResponsesCommandHandler submitHandler,
		GetUserProgressQueryHandler progressHandler)
	{
		_submitHandler = submitHandler;
		_progressHandler = progressHandler;
	}

	/// <summary>
	/// Submit user responses to a survey
	/// </summary>
	[HttpPost]
	[ProducesResponseType(200)]
	[ProducesResponseType(400)]
	public async Task<IActionResult> SubmitResponses([FromBody] SubmitResponsesCommand command, CancellationToken cancellationToken)
	{
		var result = await _submitHandler.Handle(command, cancellationToken);

		if (!result.DataRetrieved)
			return BadRequest(new { errors = result.Errors });

		return Ok(new { message = "Responses submitted successfully" });
	}

	/// <summary>
	/// Get user's progress on a publication
	/// </summary>
	[HttpGet("progress")]
	[ProducesResponseType(200)]
	[ProducesResponseType(404)]
	public async Task<IActionResult> GetProgress([FromQuery] int userId, [FromQuery] int publicationId, CancellationToken cancellationToken)
	{
		var query = new GetUserProgressQuery(userId, publicationId);
		var progress = await _progressHandler.Handle(query, cancellationToken);

		if (progress == null)
			return NotFound();

		return Ok(progress);
	}
}
