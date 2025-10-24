
// CSV Export Service

        
        
        
        
        
        
    
    
    
// CSV Export Service
namespace SurveyApi.Infrastructure.Services;

public class CsvExportService : ICsvExportService
    {
        private readonly IResponseRepository _responseRepository;

        public CsvExportService(IResponseRepository responseRepository)
        {
            _responseRepository = responseRepository;
        }

        public async Task<string?> ExportPublicationResponsesAsync(int publicationId, CancellationToken cancellationToken = default)
        {
            var responses = await _responseRepository.GetByPublicationAsync(publicationId, cancellationToken);
            
            if (!responses.Any())
                return null;

            var csv = new StringBuilder();

            // Header
            csv.AppendLine(string.Join(",", new[]
            {
                "User Email",
                "User Name",
                "Question",
                "Question Type",
                "Answer",
                "Responded At"
            }.Select(EscapeCsvField)));

            // Data rows
            foreach (var response in responses)
            {
                csv.AppendLine(string.Join(",", new[]
                {
                    response.User.Email,
                    response.User.GetFullName(),
                    response.Question.QuestionText,
                    response.Question.Type.ToString(),
                    response.ResponseText,
                    response.RespondedAt.ToString("yyyy-MM-dd HH:mm:ss")
                }.Select(EscapeCsvField)));
            }
            return csv.ToString();
        }

        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return "\"\"";
            
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
            {
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }
            
            return field;
        }
    }
