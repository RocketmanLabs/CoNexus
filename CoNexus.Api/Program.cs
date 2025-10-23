/* Program.cs
 *
 * Entry point and configuration for the CoNexus Survey Service
 *
 *
 * Changelog:
 * 2025.08.31 initial
 *
 */

using CoNexus.Api.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoNexus.Api;

internal class Program
{
	private static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		// Add services to the container
		builder.Services.AddControllers();
		builder.Services.AddEndpointsApiExplorer();
		builder.Services.AddOpenApi(options =>
		{
			options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
			{
				Title = "Survey API",
				Version = "v1",
				Description = "Clean Architecture Survey Management API"
			});
		});

		// Database
		builder.Services.AddDbContext<CnxDb>(options =>
			options.UseSqlServer(
				builder.Configuration.GetConnectionString("Default"),
				b => b.MigrationsAssembly("SurveyApi.Infrastructure")
			));

		// Infrastructure - Repositories
		builder.Services.AddScoped<IUserRepository, XnRepository>();
		builder.Services.AddScoped<ISurveyRepository, SurveyRepository>();
		builder.Services.AddScoped<IPublicationRepository, PublicationRepository>();
		builder.Services.AddScoped<IResponseRepository, ResponseRepository>();
		builder.Services.AddScoped<IScaleRepository, ScaleRepository>();
		builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

		// Infrastructure - Services
		builder.Services.AddScoped<ICsvExportService, CsvExportService>();

		// Application - Command Handlers
		builder.Services.AddScoped<SyncUsersCommandHandler>();
		builder.Services.AddScoped<CreateSurveyCommandHandler>();
		builder.Services.AddScoped<UpdateSurveyCommandHandler>();
		builder.Services.AddScoped<DeleteSurveyCommandHandler>();
		builder.Services.AddScoped<PublishSurveyCommandHandler>();
		builder.Services.AddScoped<ClosePublicationCommandHandler>();
		builder.Services.AddScoped<SubmitResponsesCommandHandler>();
		builder.Services.AddScoped<CreateScaleCommandHandler>();
		builder.Services.AddScoped<UpdateScaleCommandHandler>();
		builder.Services.AddScoped<DeleteScaleCommandHandler>();

		// Application - Query Handlers
		builder.Services.AddScoped<GetUserQueryHandler>();
		builder.Services.AddScoped<GetSurveyQueryHandler>();
		builder.Services.AddScoped<GetAllSurveysQueryHandler>();
		builder.Services.AddScoped<GetPublicationQueryHandler>();
		builder.Services.AddScoped<GetUserProgressQueryHandler>();
		builder.Services.AddScoped<GetUserReportQueryHandler>();
		builder.Services.AddScoped<GetQuestionStatisticsQueryHandler>();
		builder.Services.AddScoped<GetSurveyStatisticsQueryHandler>();
		builder.Services.AddScoped<GetScaleQueryHandler>();
		builder.Services.AddScoped<GetAllScalesQueryHandler>();

		// CORS
		builder.Services.AddCors(options =>
		{
			options.AddPolicy("CrmPolicy", policy =>
			{
				policy.WithOrigins(builder.Configuration["CrmOrigin"] ?? "*")
					.AllowAnyMethod()
					.AllowAnyHeader();
			});
		});

		var app = builder.Build();

		// Configure the HTTP request pipeline
		if (app.Environment.IsDevelopment())
		{
			app.UseSwagger();
			app.UseSwaggerUI();
		}

		app.UseHttpsRedirection();
		app.UseCors("CrmPolicy");
		app.UseAuthorization();
		app.MapControllers();

		app.Run();
	}
}