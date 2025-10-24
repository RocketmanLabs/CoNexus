using CoNexus.Api.Domain.Constants;

public record ResponseDto(
        string QuestionText,
        QuestionType QuestionType,
        string Answer,
        DateTime RespondedAt
    );
