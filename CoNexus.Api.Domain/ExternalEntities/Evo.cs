namespace CoNexus.Api.Domain.ExternalEntities;
#region Presenter Entity (Xn)

#endregion


/// <summary>
/// EVO - Event Organizer/Admin (External Entity)
/// Can create and manage surveys for their Tenant
/// </summary>
public class Evo : Xn
{

	private Evo() { }

	public static Evo CreateEngRecord(int tenantId, string xId, string title)
	{
		if (tenantId <= 0) throw new InvalidExternalEntityException("TenantId must be positive");

		return new Evo
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

	// UNDONE: Creating Surveys from the EVO record???
	public Survey CreateSurvey(int tenantId, string surveyTitle, string description = null)
	{
		if (tenantId != TenantId)
			throw new TenantMismatchException("Survey must belong to same tenant as EVO");

		return Survey.CreateByEVO(tenantId, Id, surveyTitle, description);
	}

	// Navigation properties
	public virtual Tenant Tenant { get; private set; } = null!;
	public virtual ICollection<Survey> CreatedSurveys { get; private set; } = [];
}

#endregion
