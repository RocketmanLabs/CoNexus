using CoNexus.Api.Domain.ExternalEntities;

namespace CoNexus.Api.Domain;

/// <summary>
/// Choice - Answer option for multiple choice questions
/// </summary>
public class Choice : BaseEntity
{
	private Choice() { }

	public int ScaleId { get; private set; }
	public string Text { get; private set; } = "???";
	public int Sequence { get; private set; }
	public int Value { get; private set; }

	public static Choice Create(int scaleId, string text, int sequence, int value)
	{
		if (scaleId <= 0) throw new InvalidExternalEntityException("QuestionId must be positive");

		if (string.IsNullOrWhiteSpace(text)) throw new InvalidExternalEntityException("Choice text is required");

		if (text.Length > 500) throw new InvalidExternalEntityException("Choice text cannot exceed 500 characters");

		return new Choice
		{
			ScaleId = scaleId,
			Text = text,
			Sequence = sequence,
			Value = value,
			Created = DateTime.UtcNow,
			LastUpdated = DateTime.UtcNow
		};
	}

	public void Update(string text, int sequence, int value)
	{
		if (string.IsNullOrWhiteSpace(text)) throw new InvalidExternalEntityException("Choice text is required.");
		if (sequence <= 0) throw new InvalidExternalEntityException("Choice sequence must be positive.");
		Text = text;
		Sequence = sequence;
		Value = value;
		LastUpdated = DateTime.UtcNow;
	}

	// Navigation properties
	public virtual Scale Scale { get; private set; } = null!;
}
