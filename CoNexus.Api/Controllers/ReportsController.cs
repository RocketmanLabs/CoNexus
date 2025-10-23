namespace CoNexus.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
	private readonly GetUserReportQueryHandler _userReportHandler;
	private readonly GetQuestionStatisticsQueryHandler _questionStatsHandler;
	private readonly GetSurveyStatisticsQueryHandler _surveyStatsHandler;
	private readonly ICsvExportService _csvExportService;

	public ReportsController(
		GetUserReportQueryHandler userReportHandler,
		GetQuestionStatisticsQueryHandler questionStatsHandler,
		GetSurveyStatisticsQueryHandler surveyStatsHandler,
		ICsvExportService csvExportService)
	{
		_userReportHandler = userReportHandler;
		_questionStatsHandler = questionStatsHandler;
		_surveyStatsHandler = surveyStatsHandler;
		_csvExportService = csvExportService;
	}

	/// <summary>
	/// Get per-user report
	/// </summary>
	[HttpGet("user/{userId}/publication/{publicationId}")]
	[ProducesResponseType(200)]
	[ProducesResponseType(404)]
	public async Task<IActionResult> GetUserReport(int userId, int publicationId, CancellationToken cancellationToken)
	{
		var query = new GetUserReportQuery(userId, publicationId);
		var report = await _userReportHandler.Handle(query, cancellationToken);

		if (report == null)
			return NotFound();

		return Ok(report);
	}

	/// <summary>
	/// Get per-question statistics
	/// </summary>
	[HttpGet("question/{questionId}/publication/{publicationId}")]
	[ProducesResponseType(200)]
	[ProducesResponseType(404)]
	public async Task<IActionResult> GetQuestionStatistics(int questionId, int publicationId, CancellationToken cancellationToken)
	{
		var query = new GetQuestionStatisticsQuery(questionId, publicationId);
		var stats = await _questionStatsHandler.Handle(query, cancellationToken);

		if (stats == null)
			return NotFound();

		return Ok(stats);
	}

	/// <summary>
	/// Get per-survey statistics (aggregated)
	/// </summary>
	[HttpGet("survey/{surveyId}/publication/{publicationId}")]
	[ProducesResponseType(200)]
	[ProducesResponseType(404)]
	public async Task<IActionResult> GetSurveyStatistics(int surveyId, int publicationId, CancellationToken cancellationToken)
	{
		var query = new GetSurveyStatisticsQuery(surveyId, publicationId);
		var stats = await _surveyStatsHandler.Handle(query, cancellationToken);

		if (stats == null)
			return NotFound();

		return Ok(stats);
	}

	/// <summary>
	/// Export publication results to CSV
	/// </summary>
	[HttpGet("export/publication/{publicationId}")]
	[ProducesResponseType(200)]
	[ProducesResponseType(404)]
	public async Task<IActionResult> ExportToCsv(int publicationId, CancellationToken cancellationToken)
	{
		var csvData = await _csvExportService.ExportPublicationResponsesAsync(publicationId, cancellationToken);

		if (csvData == null)
			return NotFound();

		return File(
			System.Text.Encoding.UTF8.GetBytes(csvData),
			"text/csv",
			$"survey_results_{publicationId}.csv"
		);
	}
}
        {
