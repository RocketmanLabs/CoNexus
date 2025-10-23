using CoNexus.Api.Domain.Constants;
using CoNexus.Api.Domain.ExternalEntities;

namespace CoNexus.Api.Domain;

/// <summary>
/// Survey - Survey template created by an EVO
/// Now requires TenantId for multi-tenant isolation
/// </summary>
public class Survey : BaseEntity
{
	private Survey() { }

	public int TenantId { get; private set; }
	public string Title { get; private set; } = "New Survey";
	public string Description { get; private set; } = "";
	public SurveyStatus Status { get; private set; } = SurveyStatus.Draft;

	public static Survey Create(int tenantId, string title, string? description = null)
	{
		if (tenantId <= 0) throw new InvalidExternalEntityException("TenantId must be positive");
		if (string.IsNullOrWhiteSpace(title)) throw new InvalidExternalEntityException("Survey title is required");

		return new Survey
		{
			TenantId = tenantId,
			Title = title,
			Description = description ?? "",
			Status = SurveyStatus.Draft,
			Created = DateTime.UtcNow,
			LastUpdated = DateTime.UtcNow
		};
	}

	public void Update(string title, string? description = null)
	{
		if (Status != SurveyStatus.Draft)
			throw new DomainException("Only draft surveys can be updated");

		if (string.IsNullOrWhiteSpace(title))
			throw new InvalidExternalEntityException("Survey title is required");

		Title = title;
		Description = description ?? "";
		LastUpdated = DateTime.UtcNow;
	}

	public void Publish()
	{
		if (Status != SurveyStatus.Draft) throw new DomainException("Only draft surveys can be published");

		if (Questions.Count == 0) throw new DomainException("Survey must have at least one question before publishing");

		Status = SurveyStatus.Published;
		LastUpdated = DateTime.UtcNow;
	}

	public void Archive()
	{
		Status = SurveyStatus.Archived;
		LastUpdated = DateTime.UtcNow;
	}

	public void AddQuestion(Question question)
	{
		if (question.SurveyId != Id)
			throw new TenantMismatchException("Question must belong to this survey");

		Questions.Add(question);
	}

	// Navigation properties
	public virtual Tenant Tenant { get; private set; } = null!;
	public virtual ICollection<Question> Questions { get; private set; } = [];
	public virtual ICollection<SessionSurvey> SessionSurveys { get; private set; } = [];
}
