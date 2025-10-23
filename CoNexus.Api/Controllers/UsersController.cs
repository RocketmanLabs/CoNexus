namespace CoNexus.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
	private readonly SyncUsersCommandHandler _syncUsersHandler;
	private readonly GetUserQueryHandler _getUserHandler;

	public UsersController(
		SyncUsersCommandHandler syncUsersHandler,
		GetUserQueryHandler getUserHandler)
	{
		_syncUsersHandler = syncUsersHandler;
		_getUserHandler = getUserHandler;
	}

	/// <summary>
	/// Sync users from CRM
	/// </summary>
	[HttpPost("sync")]
	[ProducesResponseType(typeof(SyncXnsResult), 200)]
	public async Task<IActionResult> SyncUsers([FromBody] List<XnSyncDto> xns, CancellationToken cancellationToken)
	{
		var command = new SyncXnsCommand(xns);
		var result = await _syncUsersHandler.Handle(command, cancellationToken);
		return Ok(result);
	}

	/// <summary>
	/// Get user by ID
	/// </summary>
	[HttpGet("{id}")]
	[ProducesResponseType(200)]
	[ProducesResponseType(404)]
	public async Task<IActionResult> GetUser(int id, CancellationToken cancellationToken)
	{
		var query = new GetUserQuery(id);
		var user = await _getUserHandler.Handle(query, cancellationToken);

		if (user == null)
			return NotFound();

		return Ok(user);
	}
}
