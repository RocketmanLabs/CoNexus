namespace CoNexus.Api.Domain.Constants;

public enum SurveyType
{
	SessionFeedback = 1,	// XnSurvey is tied to Session
	EventFeedback,			// XnSurvey is tied to Event
	SessionDialogue,
	EventDialogue
}
