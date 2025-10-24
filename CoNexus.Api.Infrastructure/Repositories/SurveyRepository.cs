using SurveyApi.Infrastructure;

namespace CoNexus.Api.Infrastructure.Repositories;

public class SurveyRepository : ISurveyRepository
    {
        private readonly CnxDb _context;

        public SurveyRepository(CnxDb context)
        {
            _context = context;
        }

        public async Task<Survey> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Surveys.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<Survey> GetByIdWithQuestionsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Surveys
                .Include(s => s.Questions)
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        }

        public async Task<List<Survey>> GetAllAsync(bool activeOnly = true, CancellationToken cancellationToken = default)
        {
            var query = _context.Surveys.Include(s => s.Questions).AsQueryable();
            
            if (activeOnly)
                query = query.Where(s => s.IsActive);

            return await query.ToListAsync(cancellationToken);
        }

        public async Task AddAsync(Survey survey, CancellationToken cancellationToken = default)
        {
            await _context.Surveys.AddAsync(survey, cancellationToken);
        }

        public Task UpdateAsync(Survey survey, CancellationToken cancellationToken = default)
        {
            _context.Surveys.Update(survey);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Survey survey, CancellationToken cancellationToken = default)
        {
            _context.Surveys.Remove(survey);
            return Task.CompletedTask;
        }
    }
