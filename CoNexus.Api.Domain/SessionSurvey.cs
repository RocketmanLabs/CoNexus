using CoNexus.Api.Domain.Constants;
using CoNexus.Api.Domain.ExternalEntities;

namespace CoNexus.Api.Domain;
#region Question Entity

#endregion


/// <summary>
/// SessionSurvey - Publication of a Survey to a Session (many-to-many with metadata)
/// </summary>
public class SessionSurvey : BaseEntity
{
	private SessionSurvey() { }

	public int TenantId { get; private set; }
	public int SessionId { get; private set; }
	public int SurveyId { get; private set; }
	public int DisplayOrder { get; private set; }
	public bool IsRequired { get; private set; }
	public SurveyType SurveyType { get; private set; }
	public DateTime? PublishedAt { get; private set; }
	public DateTime? ClosedAt { get; private set; }

	public static SessionSurvey Create(int tenantId, int sessionId, int surveyId, SurveyType surveyType, int displayOrder, bool isRequired = false)
	{
		if (tenantId <= 0) throw new InvalidExternalEntityException("TenantId must be positive");

		if (sessionId <= 0) throw new InvalidExternalEntityException("SessionId must be positive");

		if (surveyId <= 0) throw new InvalidExternalEntityException("SurveyId must be positive");

		if (displayOrder < 0) throw new InvalidExternalEntityException("DisplayOrder cannot be negative");

		return new SessionSurvey
		{
			TenantId = tenantId,
			SessionId = sessionId,
			SurveyId = surveyId,
			SurveyType = surveyType,
			DisplayOrder = displayOrder,
			IsRequired = isRequired,
			PublishedAt = DateTime.UtcNow,
			Created = DateTime.UtcNow,
			LastUpdated = DateTime.UtcNow
		};
	}

	public void Close()
	{
		ClosedAt = DateTime.UtcNow;
		LastUpdated = DateTime.UtcNow;
	}

	public bool IsOpen => PublishedAt.HasValue && !ClosedAt.HasValue && PublishedAt <= DateTime.UtcNow;

	// Navigation properties
	public virtual Tenant Tenant { get; private set; } = null!;
	public virtual Session Session { get; private set; } = null!;
	public virtual Survey Survey { get; private set; } = null!;
	public virtual ICollection<Response> Responses { get; private set; } = [];
}

#endregion
