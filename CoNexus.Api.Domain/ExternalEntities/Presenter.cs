namespace CoNexus.Api.Domain.ExternalEntities;
#region Attendee Entity (Xn)

#endregion


/// <summary>
/// Presenter - Facilitator/speaker (External Entity)
/// Can present multiple Sessions across Events
/// </summary>
public class Presenter : Xn
{
	private Presenter() { }

	public static Presenter CreateEngRecord(int tenantId, string xId, string title)
	{
		if (tenantId <= 0) throw new InvalidExternalEntityException("TenantId must be positive");

		return new Presenter
		{
			XId = xId,
			Title = title,
			TenantId = tenantId,
			Created = DateTime.UtcNow,
			LastUpdated = DateTime.UtcNow
		};
	}

	public override void SaveEngRecord(string xid, string title)
	{
		base.SaveEngRecord(xid, title);
	}

	// Navigation properties
	public virtual Tenant Tenant { get; private set; } = null!;
	public virtual ICollection<Session> Sessions { get; private set; } = [];
}

#endregion
