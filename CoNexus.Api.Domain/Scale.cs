using CoNexus.Api.Domain.ExternalEntities;

namespace CoNexus.Api.Domain;

/// <summary>
/// Scale - Reusable rating scale (e.g., 1-5, 1-10)
/// </summary>
public class Scale : BaseEntity
{
	private Scale() { }

	public int TenantId { get; private set; }
	public string Title { get; private set; } = "New Scale";
	public bool Shareable { get; set; }

	public static Scale Create(int tenantId, string title)
	{
		if (tenantId <= 0)
			throw new InvalidExternalEntityException("TenantId must be positive");

		if (string.IsNullOrWhiteSpace(title))
			throw new InvalidExternalEntityException("Scale title is required");

		return new Scale
		{
			TenantId = tenantId,
			Title = title,
			Created = DateTime.UtcNow,
			LastUpdated = DateTime.UtcNow
		};
	}

	public void Update(string title, bool shareable)
	{
		if (string.IsNullOrWhiteSpace(title)) throw new InvalidExternalEntityException("Scale title is required");

		Title = title;
		Shareable = shareable;
		LastUpdated = DateTime.UtcNow;
	}

	// Navigation properties
	public virtual Tenant Tenant { get; private set; } = null!;
	public virtual ICollection<Choice> Choices { get; private set; } = [];
}
