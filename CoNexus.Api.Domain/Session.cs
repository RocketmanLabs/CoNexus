using CoNexus.Api.Domain.ExternalEntities;

namespace CoNexus.Api.Domain;

/// <summary>
/// Session - Facilitated content delivery within an Event (External Entity)
/// </summary>
public class Session : Xn
{
	/// <summary>
	/// Foreign key to Event
	/// </summary>
	public int EventId { get; private set; }

	// Navigation properties
	public virtual Tenant Tenant { get; private set; }
	public virtual Event Event { get; private set; }
	public virtual ICollection<SessionSurvey> Surveys { get; private set; } = new List<SessionSurvey>();
	public virtual ICollection<Presenter> Presenters { get; private set; } = new List<Presenter>();

	private Session() { }

	public static Session CreateEngRecord(int tenantId, int eventId, string xId, string title)
	{
		if (tenantId <= 0) throw new InvalidExternalEntityException("TenantId must be positive");
		if (eventId <= 0) throw new InvalidExternalEntityException("EventId must be positive");

		return new Session
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
}

#endregion
