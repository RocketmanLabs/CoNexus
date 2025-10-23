// ============================================
// CoNexus.Api - Presentation Layer (Controllers)
// ============================================

using Microsoft.AspNetCore.Mvc;
using SurveyApi.Application.Commands;
using SurveyApi.Application.Handlers.Queries;
using SurveyApi.Application.Queries;
using SurveyApi.Domain.Exceptions;

namespace CoNexus.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PublicationsController : ControllerBase
{
	private readonly PublishSurveyCommandHandler _publishHandler;
	private readonly ClosePublicationCommandHandler _closeHandler;
	private readonly GetPublicationQueryHandler _getPublicationHandler;

	public PublicationsController(
		PublishSurveyCommandHandler publishHandler,
		ClosePublicationCommandHandler closeHandler,
		GetPublicationQueryHandler getPublicationHandler)
	{
		_publishHandler = publishHandler;
		_closeHandler = closeHandler;
		_getPublicationHandler = getPublicationHandler;
	}

	/// <summary>
	/// Publish a survey (CRM endpoint)
	/// </summary>
	[HttpPost]
	[ProducesResponseType(201)]
	[ProducesResponseType(404)]
	public async Task<IActionResult> PublishSurvey([FromBody] PublishSurveyCommand command, CancellationToken cancellationToken)
	{
		try
		{
			var publicationId = await _publishHandler.Handle(command, cancellationToken);
			var publication = await _getPublicationHandler.Handle(new GetPublicationQuery(publicationId), cancellationToken);
			return CreatedAtAction(nameof(GetPublication), new { id = publicationId }, publication);
		}
		catch (SurveyNotFoundException)
		{
			return NotFound(new { error = "Survey not found" });
		}
		catch (InvalidOperationException ex)
		{
			return BadRequest(new { error = ex.Message });
		}
	}

	/// <summary>
	/// Get publication by ID
	/// </summary>
	[HttpGet("{id}")]
	[ProducesResponseType(200)]
	[ProducesResponseType(404)]
	public async Task<IActionResult> GetPublication(int id, CancellationToken cancellationToken)
	{
		var query = new GetPublicationQuery(id);
		var publication = await _getPublicationHandler.Handle(query, cancellationToken);

		if (publication == null)
			return NotFound();

		return Ok(publication);
	}

	/// <summary>
	/// Close a publication
	/// </summary>
	[HttpPost("{id}/close")]
	[ProducesResponseType(200)]
	[ProducesResponseType(404)]
	public async Task<IActionResult> ClosePublication(int id, CancellationToken cancellationToken)
	{
		try
		{
			var command = new ClosePublicationCommand(id);
			var result = await _closeHandler.Handle(command, cancellationToken);

			if (!result)
				return NotFound();

			return Ok(new { message = "Publication closed successfully" });
		}
		catch (InvalidOperationException ex)
		{
			return BadRequest(new { error = ex.Message });
		}
	}
}
