using CoNexus.Api.Domain.ExternalEntities;

namespace CoNexus.Api.Domain;

/// <summary>
/// Tenant - Organization/Customer account (External Entity)
/// Root entity with no parent TenantId
/// </summary>
public class Tenant : Xn
{
	// Navigation properties
	public virtual ICollection<Event> Events { get; private set; } = [];
	public virtual ICollection<Session> Sessions { get; private set; } = [];
	public virtual ICollection<Attendee> Attendees { get; private set; } = [];
	public virtual ICollection<Presenter> Presenters { get; private set; } = [];
	public virtual ICollection<Evo> EVOs { get; private set; } = [];
	public virtual ICollection<Survey> Surveys { get; private set; } = [];
	public virtual ICollection<Scale> Scales { get; private set; } = [];
	public virtual ICollection<Response> Responses { get; private set; } = [];

	private Tenant() { }

	public static Tenant SaveEngRecord(string xid, string title)
	{
		return new Tenant
		{
			XId = xid,
			Title = title,
			TenantId = null,  // Tenant has no parent
			Created = DateTime.UtcNow,
			LastUpdated = DateTime.UtcNow
		};
	}

	public override void UpdateEngRecord(string xid, string title)
	{
		base.UpdateEngRecord(xid, title);
	}
}
