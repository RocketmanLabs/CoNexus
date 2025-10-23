using CoNexus.Api.Domain.ExternalEntities;

namespace CoNexus.Api.Domain;
#endregion


/// <summary>
/// Event - Large gathering of humans.
/// </summary>
public class Event : Xn
{
	private Event() 
	{
	
	}

	public static Event SaveEngRecord(int tenantId, string xId, string title)
	{
		if (tenantId <= 0) throw new InvalidExternalEntityException("TenantId must be positive");

		return new Event
		{
			XId = xId,
			Title = title,
			TenantId = tenantId,
			Created = DateTime.UtcNow,
			LastUpdated = DateTime.UtcNow
		};
	}

	public override void UpdateEngRecord(string title)
	{
		base.UpdateEngRecord(title);
	}

	// Navigation properties for Event:
	public virtual Tenant Tenant { get; private set; } = null!;
	public virtual ICollection<Session> Sessions { get; private set; } = [];
	public virtual ICollection<Attendee> Attendees { get; private set; } = [];
}

#endregion
