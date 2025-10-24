namespace CoNexus.Api.Infrastructure;

public class CnxDb : DbContext
{
	public CnxDb(DbContextOptions<CnxDb> options) : base(options) { }

	public DbSet<Xn> Xns { get; set; }
	//public DbSet<Tenant> Tenants { get; set; }
	//public DbSet<Event> Events { get; set; }
	//public DbSet<Session> Sessions { get; set; }
	//public DbSet<Attendee> Attendees { get; set; }
	//public DbSet<Evo> Evos { get; set; }
	//public DbSet<Presenter> Presenters { get; set; }
	public DbSet<SessionSurvey> SessionSurveys { get; set; }
	public DbSet<Survey> Surveys { get; set; }
	public DbSet<Question> Questions { get; set; }
	public DbSet<SurveyPublication> SurveyPublications { get; set; }
	public DbSet<Vote> Votes { get; set; }
	public DbSet<Scale> Scales { get; set; }
	public DbSet<Choice> Choices { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		// User (Xn) configuration
		modelBuilder.Entity<Xn>(entity =>
		{
			entity.HasKey(e => e.Id);
			entity.Property(e => e.XId).IsRequired().HasMaxLength(50);
			entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
			entity.HasIndex(e => e.XId).IsUnique();
		});

		// Survey configuration
		modelBuilder.Entity<Survey>(entity =>
		{
			entity.HasKey(e => e.Id);
			entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
			entity.Property(e => e.Instructions).HasMaxLength(1000);
			entity.Property(e => e.IsActive).IsRequired();

			entity.HasMany(s => s.Questions)
				.WithOne(q => q.Survey)
				.HasForeignKey(q => q.SurveyId)
				.OnDelete(DeleteBehavior.Cascade);
		});

		// Scale configuration
		modelBuilder.Entity<Scale>(entity =>
		{
			entity.HasKey(e => e.Id);
			entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
			entity.Property(e => e.TenantId).IsRequired();

			entity.HasMany(s => s.Choices)
				.WithOne(c => c.Scale)
				.HasForeignKey(c => c.ScaleId)
				.OnDelete(DeleteBehavior.Cascade);

			entity.HasMany(s => s.Questions)
				.WithOne(q => q.Scale)
				.HasForeignKey(q => q.ScaleId)
				.OnDelete(DeleteBehavior.Restrict);
		});

		// Choice configuration
		modelBuilder.Entity<Choice>(entity =>
		{
			entity.HasKey(e => e.Id);
			entity.Property(e => e.Text).IsRequired().HasMaxLength(50);
			entity.Property(e => e.Sequence).IsRequired();
			entity.Property(e => e.Value).IsRequired();

			entity.HasIndex(e => new { e.ScaleId, e.Sequence });
		});

		// Question configuration
		modelBuilder.Entity<Question>(entity =>
		{
			entity.HasKey(e => e.Id);
			entity.Property(e => e.QuestionText).IsRequired().HasMaxLength(500);
			entity.Property(e => e.QuestionType).IsRequired();
			entity.Property(e => e.Sequence).IsRequired();
			entity.Property(e => e.IsRequired).IsRequired();
			entity.Property(e => e.ScaleId).IsRequired(false);

			entity.HasIndex(e => new { e.SurveyId, e.Sequence });
		});

		// SurveyPublication configuration
		modelBuilder.Entity<SurveyPublication>(entity =>
		{
			entity.HasKey(e => e.Id);
			entity.Property(e => e.PublicationName).IsRequired().HasMaxLength(200);
			entity.Property(e => e.PublishedAt).IsRequired();
			entity.Property(e => e.IsOpen).IsRequired();

			entity.HasOne(sp => sp.Survey)
				.WithMany(s => s.Publications)
				.HasForeignKey(sp => sp.SurveyId)
				.OnDelete(DeleteBehavior.Restrict);
		});

		// UserResponse configuration
		modelBuilder.Entity<User>(entity =>
		{
			entity.HasKey(e => e.Id);
			entity.Property(e => e.ResponseText).IsRequired().HasMaxLength(4000);
			entity.Property(e => e.RespondedAt).IsRequired();

			entity.HasOne(ur => ur.User)
				.WithMany(u => u.Responses)
				.HasForeignKey(ur => ur.UserId)
				.OnDelete(DeleteBehavior.Cascade);

			entity.HasOne(ur => ur.Question)
				.WithMany(q => q.Responses)
				.HasForeignKey(ur => ur.QuestionId)
				.OnDelete(DeleteBehavior.Restrict);

			entity.HasOne(ur => ur.Publication)
				.WithMany(sp => sp.Responses)
				.HasForeignKey(ur => ur.PublicationId)
				.OnDelete(DeleteBehavior.Cascade);

			entity.HasIndex(e => new { e.UserId, e.QuestionId, e.PublicationId }).IsUnique();
		});
	}
}
