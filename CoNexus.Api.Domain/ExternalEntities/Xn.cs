namespace CoNexus.Api.Domain.ExternalEntities;

/// <summary>
/// Base class for all External Entities (Xns)
/// Synchronized from CRM with XId (GUID) and Title
/// </summary>
public abstract class Xn : BaseEntity
{
	protected Xn() 
	{
		XId = Guid.NewGuid().ToString("N");
		Title = "New Xn";
	}

	protected Xn(string xid, string title, int? tenantId = null)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(xid, nameof(xid));
		ArgumentOutOfRangeException.ThrowIfGreaterThan(xid.Length, 50, nameof(xid));

		ArgumentException.ThrowIfNullOrWhiteSpace(title, nameof(title));
		ArgumentOutOfRangeException.ThrowIfGreaterThan(title.Length, 100, nameof(title));

		ArgumentNullException.ThrowIfNull(tenantId, nameof(tenantId));
		ArgumentOutOfRangeException.ThrowIfLessThan(tenantId ?? 0, 0, nameof(tenantId));

		XId = xid;
		Title = title;
		TenantId = tenantId;
		Created = DateTime.UtcNow;
		LastUpdated = DateTime.UtcNow;
	}

	/// <summary>
	/// External ID - GUID v4 from CRM (unique per Tenant)
	/// </summary>
	public string XId { get; set; }

	/// <summary>
	/// Display title/name
	/// </summary>
	public string Title { get; set; }

	/// <summary>
	/// Tenant ID - FK to Tenant (not nullable for non-Tenant entities).
	/// </summary>
	public int? TenantId { get; set; }

	/// <summary>
	/// Sets the type for this external entity.
	/// </summary>
	public XnType XnType { get; set; }

	/// <summary>
	/// Called to update the Title field of any ExternalEntity.
	/// </summary>
	/// <param name="xid">XId of record to update.</param>
	/// <param name="title">New title.</param>
	public virtual void SaveEngRecord(string xid, string title)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(xid, nameof(xid));
		ArgumentOutOfRangeException.ThrowIfGreaterThan(xid.Length, 50, nameof(xid));

		ArgumentException.ThrowIfNullOrWhiteSpace(title, nameof(title));
		ArgumentOutOfRangeException.ThrowIfGreaterThan(title.Length, 100, nameof(title));

		Title = title;
		LastUpdated = DateTime.UtcNow;
	}
}
