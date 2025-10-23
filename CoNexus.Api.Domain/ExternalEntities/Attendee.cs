namespace CoNexus.Api.Domain.ExternalEntities;

/// <summary>
/// Attendee - Person attending an Event (External Entity)
/// Can submit responses to surveys
/// </summary>
public class Attendee : Xn
{
	/// <summary>
	/// Foreign key to Event
	/// </summary>
	public int EventId { get; private set; }

	private Attendee() { }

	public static Attendee CreateEngRecord(int tenantId, int eventId, string xId, string title)
	{
		if (tenantId <= 0)
			throw new InvalidExternalEntityException("TenantId must be positive");

		if (eventId <= 0)
			throw new InvalidExternalEntityException("EventId must be positive");

		return new Attendee
		{
			XId = xId,
			Title = title,
			TenantId = tenantId,
			EventId = eventId,
			Created = DateTime.UtcNow,
			LastUpdated = DateTime.UtcNow
		};
	}

	public override void SaveEngRecord(string xid, string title)
	{
		base.SaveEngRecord(xid, title);
	}

	public void AddResponse(Response response)
	{
		if (response.AttendeeId != Id)
			throw new TenantMismatchException("Response must belong to this attendee");

		Responses.Add(response);
	}

	// Navigation properties
	public virtual Tenant Tenant { get; private set; } = null!;
	public virtual Event Event { get; private set; } = null!;
	public virtual ICollection<Response> Responses { get; private set; } = [];
}

#endregion
