namespace CoNexus.Api.Domain;

/// <summary>
/// Question - Survey question
/// </summary>
public class Question : BaseEntity
{
	private Question() { }

	public int SurveyId { get; private set; }
	public int ScaleId { get; private set; }
	public string QuestionText { get; private set; } = "???";
	public QuestionType QuestionType { get; private set; }
	public int Sequence { get; private set; }
	public bool IsRequired { get; private set; }

	public static Question Create(int surveyId,
								   int seq,
								   string questionText,
								   QuestionType questionType,
								   bool isRequired = false)
	{
		if (surveyId <= 0) throw new InvalidExternalEntityException("SurveyId must be positive");
		if (string.IsNullOrWhiteSpace(questionText)) throw new InvalidExternalEntityException("Question text is required");

		return new Question
		{
			SurveyId = surveyId,
			QuestionText = questionText,
			QuestionType = questionType,
			Sequence = seq,
			IsRequired = isRequired,
			Created = DateTime.UtcNow,
			LastUpdated = DateTime.UtcNow
		};
	}

	public void Update(string questionText, int seq, bool isRequired)
	{
		if (string.IsNullOrWhiteSpace(questionText)) throw new InvalidExternalEntityException("Question text is required.");

		QuestionText = questionText;
		Sequence = seq;
		IsRequired = isRequired;
		LastUpdated = DateTime.UtcNow;
	}

	public void AddChoice(Choice choice)
	{
		if (choice.ScaleId != Id) throw new TenantMismatchException("Choice must belong to this Scale.");
		Choices.Add(choice);
	}

	// Navigation properties
	public virtual Survey Survey { get; private set; } = null!;
	public virtual Scale Scale { get; private set; } = null!;
	public virtual ICollection<Choice> Choices { get; private set; } = [];
	public virtual ICollection<Vote> Votes { get; private set; } = [];
}
