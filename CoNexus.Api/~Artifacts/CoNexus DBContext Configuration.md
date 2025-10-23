# DbContext Configuration Guide

## Overview

The **SurveyDbContext** is already implemented in the Infrastructure layer and properly configured. This guide shows how to set up the connection string and verify the configuration.

## DbContext Location

The DbContext is located in:
```
SurveyApi.Infrastructure/Persistence/SurveyDbContext.cs
```

## DbContext Implementation

```csharp
namespace SurveyApi.Infrastructure.Persistence
{
    public class SurveyDbContext : DbContext
    {
        public SurveyDbContext(DbContextOptions<SurveyDbContext> options) 
            : base(options) { }

        // DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Survey> Surveys { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<SurveyPublication> SurveyPublications { get; set; }
        public DbSet<UserResponse> UserResponses { get; set; }
        public DbSet<Scale> Scales { get; set; }
        public DbSet<Choice> Choices { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // All entity configurations are already defined
            // Including relationships, indexes, and constraints
        }
    }
}
```

## Configuration in Program.cs

The DbContext is registered in `CoNexus.Api/Program.cs`:

```csharp
// Database configuration
builder.Services.AddDbContext<SurveyDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("Default"),
        b => b.MigrationsAssembly("SurveyApi.Infrastructure")
    ));
```

**Key Points:**
- Connection string key: `"Default"`
- Migrations assembly: `"SurveyApi.Infrastructure"`
- Database provider: SQL Server

## appsettings.json Configuration

Create or update your `appsettings.json` in the API project:

### Development (LocalDB)

```json
{
  "ConnectionStrings": {
    "Default": "Server=(localdb)\\mssqllocaldb;Database=SurveyDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "AllowedHosts": "*",
  "CrmOrigin": "https://localhost:5000"
}
```

### Development (SQL Server Express)

```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost\\SQLEXPRESS;Database=SurveyDb;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "AllowedHosts": "*",
  "CrmOrigin": "https://localhost:5000"
}
```

### Production (Azure SQL Database)

```json
{
  "ConnectionStrings": {
    "Default": "Server=tcp:your-server.database.windows.net,1433;Initial Catalog=SurveyDb;Persist Security Info=False;User ID=your-username;Password=your-password;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "CrmOrigin": "https://your-crm-domain.com"
}
```

### Production (SQL Server with SQL Authentication)

```json
{
  "ConnectionStrings": {
    "Default": "Server=your-sql-server;Database=SurveyDb;User Id=survey_app_user;Password=YourSecurePassword123!;MultipleActiveResultSets=true;TrustServerCertificate=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

## appsettings.Development.json (Optional)

For better security in development, create `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "Default": "Server=(localdb)\\mssqllocaldb;Database=SurveyDb_Dev;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information",
      "Microsoft.EntityFrameworkCore": "Information",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

## User Secrets (Recommended for Development)

For sensitive connection strings in development:

### 1. Initialize User Secrets

```bash
cd CoNexus.Api
dotnet user-secrets init
```

### 2. Set Connection String

```bash
dotnet user-secrets set "ConnectionStrings:Default" "Server=(localdb)\\mssqllocaldb;Database=SurveyDb;Trusted_Connection=true;MultipleActiveResultSets=true"
```

### 3. View Secrets

```bash
dotnet user-secrets list
```

### 4. Remove Secret

```bash
dotnet user-secrets remove "ConnectionStrings:Default"
```

## Environment Variables (Production)

For production deployments, use environment variables:

### Windows
```cmd
setx ConnectionStrings__Default "Server=prod-server;Database=SurveyDb;User Id=app_user;Password=SecurePass123!"
```

### Linux/Mac
```bash
export ConnectionStrings__Default="Server=prod-server;Database=SurveyDb;User Id=app_user;Password=SecurePass123!"
```

### Docker
```dockerfile
ENV ConnectionStrings__Default="Server=prod-server;Database=SurveyDb;User Id=app_user;Password=SecurePass123!"
```

### Azure App Service
Set in Configuration → Application Settings:
- Name: `ConnectionStrings__Default`
- Value: `[your connection string]`
- Type: Custom

## Verify DbContext Configuration

### 1. Check Registrations

Add this to Program.cs for debugging:

```csharp
var app = builder.Build();

// Verify DbContext is registered
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SurveyDbContext>();
    var connectionString = context.Database.GetConnectionString();
    Console.WriteLine($"DbContext Connection: {connectionString}");
}
```

### 2. Test Database Connection

```csharp
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SurveyDbContext>();
    
    try
    {
        var canConnect = await context.Database.CanConnectAsync();
        Console.WriteLine($"Database Connection: {(canConnect ? "SUCCESS" : "FAILED")}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database Connection Error: {ex.Message}");
    }
}
```

## Creating and Running Migrations

### 1. Install EF Core Tools

```bash
dotnet tool install --global dotnet-ef
# or update
dotnet tool update --global dotnet-ef
```

### 2. Create Initial Migration

```bash
# From the API project directory
cd CoNexus.Api

dotnet ef migrations add InitialCreate \
    --project ../SurveyApi.Infrastructure \
    --startup-project . \
    --output-dir Persistence/Migrations
```

### 3. Apply Migration

```bash
dotnet ef database update \
    --project ../SurveyApi.Infrastructure \
    --startup-project .
```

### 4. Add Scale and Choice Migration

```bash
dotnet ef migrations add AddScaleAndChoice \
    --project ../SurveyApi.Infrastructure \
    --startup-project .
    
dotnet ef database update \
    --project ../SurveyApi.Infrastructure \
    --startup-project .
```

## DbContext Features Already Configured

### ✅ All Entity Configurations
- **Users**: Unique constraint on ExternalCrmId
- **Surveys**: Cascade delete to Questions
- **Questions**: Optional ScaleId, indexed on (SurveyId, OrderIndex)
- **Scales**: Cascade delete to Choices, restrict delete from Questions
- **Choices**: Indexed on (ScaleId, Sequence)
- **SurveyPublications**: Restrict delete behavior
- **UserResponses**: Unique constraint on (UserId, QuestionId, PublicationId)

### ✅ Relationships
All foreign key relationships properly configured with appropriate delete behaviors.

### ✅ Indexes
Strategic indexes on frequently queried columns for performance.

### ✅ Constraints
- MaxLength constraints on string properties
- Required field validations
- Unique constraints where needed

## Connection String Parameters Explained

### Common Parameters

| Parameter | Description | Example |
|-----------|-------------|---------|
| `Server` | Database server address | `localhost`, `(localdb)\\mssqllocaldb` |
| `Database` | Database name | `SurveyDb` |
| `Trusted_Connection` | Use Windows Authentication | `true` |
| `User Id` | SQL Server username | `survey_app_user` |
| `Password` | SQL Server password | `SecurePassword123!` |
| `MultipleActiveResultSets` | Enable MARS | `true` |
| `TrustServerCertificate` | Trust SSL certificate | `true` (dev only) |
| `Encrypt` | Encrypt connection | `true` |
| `Connection Timeout` | Timeout in seconds | `30` |
| `Integrated Security` | Use Windows Auth | `true` or `SSPI` |

## Troubleshooting

### Issue: "Cannot open database"

**Solution:**
1. Verify SQL Server is running
2. Check connection string syntax
3. Ensure database exists or run migrations
4. Verify user permissions

```bash
# Test connection with sqlcmd
sqlcmd -S (localdb)\mssqllocaldb -Q "SELECT @@VERSION"
```

### Issue: "Login failed for user"

**Solution:**
1. Verify credentials in connection string
2. Check SQL Server authentication mode
3. Ensure user has appropriate permissions

```sql
-- Grant permissions
USE SurveyDb;
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::dbo TO survey_app_user;
```

### Issue: "A network-related or instance-specific error"

**Solution:**
1. Check if SQL Server is running
2. Verify server name and port
3. Check firewall settings
4. Enable TCP/IP protocol in SQL Server Configuration Manager

### Issue: "The ConnectionString property has not been initialized"

**Solution:**
1. Verify appsettings.json has ConnectionStrings:Default
2. Check file is copied to output directory
3. Verify environment-specific settings

## Database Initialization

### Option 1: Automatic Migration on Startup

Add to Program.cs:

```csharp
var app = builder.Build();

// Apply migrations automatically
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SurveyDbContext>();
    await context.Database.MigrateAsync();
}

// Continue with middleware configuration
app.UseHttpsRedirection();
// ...
```

### Option 2: Manual Migration

```bash
dotnet ef database update
```

### Option 3: SQL Script Generation

```bash
dotnet ef migrations script \
    --project ../SurveyApi.Infrastructure \
    --startup-project . \
    --output migration.sql
```

## Health Checks

Add health checks for database connectivity:

### 1. Install Package

```bash
dotnet add package Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore
```

### 2. Configure in Program.cs

```csharp
builder.Services.AddHealthChecks()
    .AddDbContextCheck<SurveyDbContext>("database");

// ... after app build

app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
```

### 3. Test Health Check

```bash
curl https://localhost:5001/health
```

## Summary

✅ **DbContext exists** in `SurveyApi.Infrastructure/Persistence/SurveyDbContext.cs`

✅ **Already registered** in `Program.cs` with proper configuration

✅ **Connection string key**: `"ConnectionStrings:Default"`

✅ **All entities configured** with relationships, indexes, and constraints

✅ **Ready to use** - just add your connection string to `appsettings.json`

## Quick Start Checklist

- [ ] Add connection string to `appsettings.json` under `ConnectionStrings:Default`
- [ ] Install EF Core tools: `dotnet tool install --global dotnet-ef`
- [ ] Create initial migration: `dotnet ef migrations add InitialCreate`
- [ ] Update database: `dotnet ef database update`
- [ ] Run application: `dotnet run`
- [ ] Test API at: `https://localhost:5001/swagger`

The DbContext is fully implemented and ready to use!