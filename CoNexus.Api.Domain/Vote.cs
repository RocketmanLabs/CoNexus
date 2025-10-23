using CoNexus.Api.Domain.ExternalEntities;

namespace CoNexus.Api.Domain;
#region SessionSurvey Entity

#endregion

#region Response Entity

/// <summary>
/// Vote - Survey response from an Attendee to a Survey published in a Session
/// </summary>
public class Vote : BaseEntity
{
	public int TenantId { get; private set; }
	public int SessionSurveyId { get; private set; }
	public int AttendeeId { get; private set; }
	public int QuestionId { get; private set; }
	public int SelectedId { get; private set; }
	public string ResponseText { get; private set; }
	public DateTime RespondedAt { get; private set; }

	// Navigation properties
	public virtual Tenant Tenant { get; private set; } = null!;
	public virtual SessionSurvey SessionSurvey { get; private set; } = null!;
	public virtual Attendee Attendee { get; private set; } = null!;
	public virtual Question Question { get; private set; } = null!;
	public virtual Choice Choice { get; private set; } = null!;

	private Vote() { }

	public static Vote Create(int tenantId,
						   int sessionSurveyId,
						   int attendeeId,
						   int questionId,
						   int selectedId,
						   string responseText = "")
	{
		if (tenantId <= 0) throw new InvalidExternalEntityException("TenantId must be positive");
		if (sessionSurveyId <= 0) throw new InvalidExternalEntityException("SessionSurveyId must be positive");
		if (attendeeId <= 0) throw new InvalidExternalEntityException("AttendeeId must be positive");
		if (questionId <= 0) throw new InvalidExternalEntityException("QuestionId must be positive");
		if (selectedId <= 0) throw new InvalidExternalEntityException("Selected ChoiceId must be positive");

		return new Vote
		{
			TenantId = tenantId,
			SessionSurveyId = sessionSurveyId,
			AttendeeId = attendeeId,
			QuestionId = questionId,
			SelectedId = selectedId,
			ResponseText = responseText,
			RespondedAt = DateTime.UtcNow,
			Created = DateTime.UtcNow,
			LastUpdated = DateTime.UtcNow
		};
	}
}

#endregion
