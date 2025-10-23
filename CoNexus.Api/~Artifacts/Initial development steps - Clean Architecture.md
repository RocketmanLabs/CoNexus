# Survey API - Clean Architecture Implementation Guide

## Architecture Overview

This solution follows **Clean Architecture** principles with clear separation of concerns across multiple layers:

```
┌─────────────────────────────────────────────────────┐
│                  API/Presentation                   │
│              (Controllers, DTOs)                    │
├─────────────────────────────────────────────────────┤
│                   Application                       │
│     (Use Cases, Commands, Queries, Handlers)        │
├─────────────────────────────────────────────────────┤
│                     Domain                          │
│        (Entities, Value Objects, Exceptions)        │
├─────────────────────────────────────────────────────┤
│                  Infrastructure                     │
│    (Data Access, External Services, Repositories)   │
└─────────────────────────────────────────────────────┘
```

### Dependency Flow
- **API** is same as → **Application**
- **Application** depends on → **Domain**
- **Infrastructure** depends on → **Application** and **Domain**
- **Domain** has NO dependencies (pure business logic)

## Project Structure

```
SurveyApi/
├── SurveyApi.Domain/              # Core Domain Layer
│   ├── Entities/                  # Domain entities
│   │   ├── User.cs
│   │   ├── Survey.cs
│   │   ├── Question.cs
│   │   ├── SurveyPublication.cs
│   │   └── UserResponse.cs
│   └── Exceptions/                # Domain exceptions
│       └── DomainExceptions.cs
│
├── SurveyApi.Application/         # Application Layer
│   ├── Common/
│   │   └── Interfaces/            # Repository interfaces
│   ├── Commands/                  # Write operations
│   ├── Queries/                   # Read operations
│   ├── Handlers/
│   │   ├── Commands/              # Command handlers
│   │   └── Queries/               # Query handlers
│   └── DTOs/                      # Data Transfer Objects
│
├── SurveyApi.Infrastructure/      # Infrastructure Layer
│   ├── Persistence/               # Data access
│   │   ├── SurveyDbContext.cs
│   │   ├── UnitOfWork.cs
│   │   └── Repositories/          # Repository implementations
│   └── Services/                  # External services
│       └── CsvExportService.cs
│
└── SurveyApi.API/                 # Presentation Layer
    ├── Controllers/               # API endpoints
    ├── Program.cs                 # DI configuration
    └── appsettings.json           # Configuration
```

## Getting Started

### Step 1: Create Solution and Projects

```bash
# Create solution
dotnet new sln -n SurveyApi

# Create Domain project (Class Library)
dotnet new classlib -n SurveyApi.Domain -f net9.0
dotnet sln add SurveyApi.Domain/SurveyApi.Domain.csproj

# Create Application project (Class Library)
dotnet new classlib -n SurveyApi.Application -f net9.0
dotnet sln add SurveyApi.Application/SurveyApi.Application.csproj

# Create Infrastructure project (Class Library)
dotnet new classlib -n SurveyApi.Infrastructure -f net9.0
dotnet sln add SurveyApi.Infrastructure/SurveyApi.Infrastructure.csproj

# Create API project (Web API)
dotnet new webapi -n SurveyApi.API -f net9.0
dotnet sln add SurveyApi.API/SurveyApi.API.csproj
```

### Step 2: Set Up Project References

```bash
# Application depends on Domain
cd SurveyApi.Application
dotnet add reference ../SurveyApi.Domain/SurveyApi.Domain.csproj

# Infrastructure depends on Application and Domain
cd ../SurveyApi.Infrastructure
dotnet add reference ../SurveyApi.Application/SurveyApi.Application.csproj
dotnet add reference ../SurveyApi.Domain/SurveyApi.Domain.csproj

# API depends on Application and Infrastructure
cd ../SurveyApi.API
dotnet add reference ../SurveyApi.Application/SurveyApi.Application.csproj
dotnet add reference ../SurveyApi.Infrastructure/SurveyApi.Infrastructure.csproj
```

### Step 3: Install NuGet Packages

```bash
# Infrastructure project needs EF Core
cd SurveyApi.Infrastructure
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Design

# API project needs tools
cd ../SurveyApi.API
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Swashbuckle.AspNetCore
```

### Step 4: Configure Database

Update `appsettings.json` in API project:

```json
{
  "ConnectionStrings": {
    "SurveyDatabase": "Server=(localdb)\\mssqllocaldb;Database=SurveyDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "CrmOrigin": "https://your-crm-domain.com",
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

### Step 5: Create and Run Migrations

```bash
# From the API project directory
dotnet ef migrations add InitialCreate --project ../SurveyApi.Infrastructure --startup-project .
dotnet ef database update --project ../SurveyApi.Infrastructure --startup-project .
```

### Step 6: Run the Application

```bash
cd SurveyApi.API
dotnet run
```

Navigate to `https://localhost:5001/swagger` to view the API documentation.

## Clean Architecture Principles Applied

### 1. **Domain Layer (Core)**

**Characteristics:**
- No dependencies on other layers
- Contains business logic and domain rules
- Entities have behavior, not just data
- Uses factory methods for creation
- Validates business rules at entity level

**Example - Domain Entity with Behavior:**

```csharp
public class Survey : BaseEntity
{
    // Private setters - encapsulation
    public string Title { get; private set; }
    
    // Factory method - controlled creation
    public static Survey Create(string title, string description)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required");
        
        return new Survey { /* ... */ };
    }
    
    // Domain behavior
    public SurveyPublication Publish(string publicationName)
    {
        if (!IsActive)
            throw new InvalidOperationException("Cannot publish inactive survey");
        
        return SurveyPublication.Create(SurveyId, publicationName);
    }
}
```

### 2. **Application Layer (Use Cases)**

**Characteristics:**
- Defines interfaces for infrastructure
- Contains business workflow logic
- Uses CQRS pattern (Commands & Queries)
- Orchestrates domain objects
- Independent of infrastructure details

**CQRS Pattern:**

```csharp
// Commands - Write operations
public record CreateSurveyCommand(
    string Title,
    string Description,
    List<CreateQuestionDto> Questions
);

// Command Handler
public class CreateSurveyCommandHandler
{
    public async Task<Guid> Handle(CreateSurveyCommand command)
    {
        // Orchestrate domain logic
        var survey = Survey.Create(command.Title, command.Description);
        // Add questions, save, return ID
    }
}

// Queries - Read operations
public record GetSurveyQuery(Guid SurveyId);

// Query Handler
public class GetSurveyQueryHandler
{
    public async Task<SurveyDto> Handle(GetSurveyQuery query)
    {
        // Fetch and map to DTO
    }
}
```

### 3. **Infrastructure Layer**

**Characteristics:**
- Implements application interfaces
- Contains EF Core DbContext
- Repository pattern for data access
- External service implementations
- Database migrations

**Repository Pattern:**

```csharp
// Interface in Application layer
public interface ISurveyRepository
{
    Task<Survey> GetByIdAsync(Guid id);
    Task AddAsync(Survey survey);
}

// Implementation in Infrastructure layer
public class SurveyRepository : ISurveyRepository
{
    private readonly SurveyDbContext _context;
    
    public async Task<Survey> GetByIdAsync(Guid id)
    {
        return await _context.Surveys.FindAsync(id);
    }
}
```

### 4. **Presentation Layer (API)**

**Characteristics:**
- Thin controllers
- Delegate to handlers
- Handle HTTP concerns only
- Map requests to commands/queries
- Return appropriate HTTP responses

**Thin Controller Example:**

```csharp
[ApiController]
[Route("api/[controller]")]
public class SurveysController : ControllerBase
{
    private readonly CreateSurveyCommandHandler _handler;
    
    [HttpPost]
    public async Task<IActionResult> CreateSurvey(
        [FromBody] CreateSurveyCommand command)
    {
        var surveyId = await _handler.Handle(command);
        return CreatedAtAction(nameof(GetSurvey), new { id = surveyId });
    }
}
```

## Design Patterns Used

### 1. **Repository Pattern**
Abstracts data access logic from business logic.

### 2. **Unit of Work Pattern**
Manages transactions and coordinates writing changes to the database.

### 3. **CQRS (Command Query Responsibility Segregation)**
Separates read and write operations for better scalability and maintainability.

### 4. **Factory Pattern**
Domain entities use static factory methods for creation with validation.

### 5. **Dependency Injection**
All dependencies are injected, making the code testable and maintainable.

## Key Benefits of This Architecture

### ✅ **Testability**
- Domain logic can be tested without database
- Mock repositories for application layer tests
- Integration tests at API level

### ✅ **Maintainability**
- Clear separation of concerns
- Easy to locate and modify code
- Changes in one layer don't affect others

### ✅ **Flexibility**
- Easy to swap out infrastructure (SQL Server → PostgreSQL)
- Can add new use cases without modifying existing code
- Support multiple presentation layers (Web API, gRPC, etc.)

### ✅ **Domain-Centric**
- Business logic is isolated and protected
- Domain rules enforced at entity level
- Technology-agnostic core

## Testing Strategy

### Unit Tests - Domain Layer

```csharp
[Fact]
public void Survey_Create_WithEmptyTitle_ThrowsException()
{
    // Arrange & Act
    var action = () => Survey.Create("", "Description");
    
    // Assert
    Assert.Throws<ArgumentException>(action);
}

[Fact]
public void Survey_Publish_WhenInactive_ThrowsException()
{
    // Arrange
    var survey = Survey.Create("Test", "Description");
    survey.Deactivate();
    
    // Act & Assert
    Assert.Throws<InvalidOperationException>(() => 
        survey.Publish("Publication"));
}
```

### Unit Tests - Application Layer

```csharp
[Fact]
public async Task CreateSurveyHandler_ValidCommand_CreatesSurvey()
{
    // Arrange
    var mockRepo = new Mock<ISurveyRepository>();
    var mockUnitOfWork = new Mock<IUnitOfWork>();
    var handler = new CreateSurveyCommandHandler(mockRepo.Object, mockUnitOfWork.Object);
    
    var command = new CreateSurveyCommand("Title", "Description", new List<CreateQuestionDto>());
    
    // Act
    var surveyId = await handler.Handle(command, CancellationToken.None);
    
    // Assert
    Assert.NotEqual(Guid.Empty, surveyId);
    mockRepo.Verify(r => r.AddAsync(It.IsAny<Survey>(), It.IsAny<CancellationToken>()), Times.Once);
    mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
}
```

### Integration Tests - API Layer

```csharp
[Fact]
public async Task CreateSurvey_ValidRequest_ReturnsCreated()
{
    // Arrange
    var client = _factory.CreateClient();
    var command = new CreateSurveyCommand(/* ... */);
    
    // Act
    var response = await client.PostAsJsonAsync("/api/surveys", command);
    
    // Assert
    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    var survey = await response.Content.ReadFromJsonAsync<SurveyDto>();
    Assert.NotNull(survey);
}
```

## API Endpoints (Same as Before)

All endpoints remain the same as in the previous implementation. The difference is in the internal architecture.

### Example Workflow

**Creating a Survey:**

1. **API Layer**: Controller receives HTTP POST with `CreateSurveyCommand`
2. **Application Layer**: `CreateSurveyCommandHandler` orchestrates the workflow
3. **Domain Layer**: `Survey.Create()` factory method validates and creates entity
4. **Application Layer**: Handler calls `ISurveyRepository.AddAsync()`
5. **Infrastructure Layer**: Repository saves to database via `DbContext`
6. **Application Layer**: `IUnitOfWork.SaveChangesAsync()` commits transaction
7. **API Layer**: Controller returns 201 Created with survey DTO

## Dependency Injection Configuration

The `Program.cs` registers all services following the dependency inversion principle:

```csharp
// Infrastructure Layer - Data Access
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ISurveyRepository, SurveyRepository>();
builder.Services.AddScoped<IPublicationRepository, PublicationRepository>();
builder.Services.AddScoped<IResponseRepository, ResponseRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Infrastructure Layer - External Services
builder.Services.AddScoped<ICsvExportService, CsvExportService>();

// Application Layer - Command Handlers
builder.Services.AddScoped<SyncUsersCommandHandler>();
builder.Services.AddScoped<CreateSurveyCommandHandler>();
builder.Services.AddScoped<UpdateSurveyCommandHandler>();
// ... more handlers

// Application Layer - Query Handlers
builder.Services.AddScoped<GetSurveyQueryHandler>();
builder.Services.AddScoped<GetAllSurveysQueryHandler>();
// ... more handlers
```

## Advanced Features & Extensions

### 1. Adding MediatR (Recommended)

Instead of manually injecting handlers, use MediatR for cleaner code:

```bash
dotnet add package MediatR
```

**Update Application Layer:**

```csharp
// Commands implement IRequest
public record CreateSurveyCommand : IRequest<Guid>
{
    public string Title { get; init; }
    public string Description { get; init; }
    public List<CreateQuestionDto> Questions { get; init; }
}

// Handlers implement IRequestHandler
public class CreateSurveyCommandHandler : IRequestHandler<CreateSurveyCommand, Guid>
{
    public async Task<Guid> Handle(CreateSurveyCommand request, CancellationToken cancellationToken)
    {
        // Implementation
    }
}
```

**Update API Controllers:**

```csharp
public class SurveysController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public SurveysController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateSurvey(CreateSurveyCommand command)
    {
        var surveyId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetSurvey), new { id = surveyId });
    }
}
```

**Update Program.cs:**

```csharp
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(CreateSurveyCommand).Assembly));
```

### 2. Adding FluentValidation

Validate commands before handling:

```bash
dotnet add package FluentValidation
dotnet add package FluentValidation.DependencyInjectionExtensions
```

**Create Validators:**

```csharp
public class CreateSurveyCommandValidator : AbstractValidator<CreateSurveyCommand>
{
    public CreateSurveyCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);
        
        RuleFor(x => x.Description)
            .MaximumLength(1000);
        
        RuleFor(x => x.Questions)
            .NotEmpty()
            .WithMessage("Survey must have at least one question");
    }
}
```

**Register in Program.cs:**

```csharp
builder.Services.AddValidatorsFromAssembly(typeof(CreateSurveyCommand).Assembly);
```

### 3. Adding AutoMapper

Simplify mapping between entities and DTOs:

```bash
dotnet add package AutoMapper
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection
```

**Create Mapping Profiles:**

```csharp
public class SurveyMappingProfile : Profile
{
    public SurveyMappingProfile()
    {
        CreateMap<Survey, SurveyDto>();
        CreateMap<Question, QuestionDto>()
            .ForMember(dest => dest.Choices, 
                opt => opt.MapFrom(src => src.GetChoices()));
    }
}
```

**Register in Program.cs:**

```csharp
builder.Services.AddAutoMapper(typeof(SurveyMappingProfile).Assembly);
```

### 4. Adding Authentication & Authorization

**Install packages:**

```bash
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
```

**Configure in Program.cs:**

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://your-identity-server.com";
        options.Audience = "survey-api";
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("CrmAccess", policy => policy.RequireClaim("scope", "crm-access"));
});

// In pipeline
app.UseAuthentication();
app.UseAuthorization();
```

**Protect endpoints:**

```csharp
[Authorize(Policy = "AdminOnly")]
[HttpPost]
public async Task<IActionResult> CreateSurvey(CreateSurveyCommand command)
{
    // Only admins can create surveys
}

[Authorize(Policy = "CrmAccess")]
[HttpPost("sync")]
public async Task<IActionResult> SyncUsers(List<UserSyncDto> users)
{
    // Only CRM can sync users
}
```

### 5. Adding Domain Events

Implement domain events for cross-aggregate communication:

**Create Domain Event Base:**

```csharp
// In Domain layer
public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}

public record SurveyPublishedEvent(Guid SurveyId, Guid PublicationId, DateTime OccurredOn) : IDomainEvent;
```

**Add to Entity:**

```csharp
public abstract class BaseEntity
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    
    protected void AddDomainEvent(IDomainEvent eventItem)
    {
        _domainEvents.Add(eventItem);
    }
    
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}

public class SurveyPublication : BaseEntity
{
    public static SurveyPublication Create(Guid surveyId, string publicationName)
    {
        var publication = new SurveyPublication { /* ... */ };
        publication.AddDomainEvent(new SurveyPublishedEvent(
            surveyId, 
            publication.Id, 
            DateTime.UtcNow
        ));
        return publication;
    }
}
```

**Create Event Handlers:**

```csharp
// In Application layer
public class SurveyPublishedEventHandler : INotificationHandler<SurveyPublishedEvent>
{
    private readonly IEmailService _emailService;
    
    public async Task Handle(SurveyPublishedEvent notification, CancellationToken cancellationToken)
    {
        // Send email notifications to users
        await _emailService.SendSurveyNotificationAsync(notification.SurveyId);
    }
}
```

### 6. Adding Caching

Implement caching at the infrastructure layer:

```csharp
public class CachedSurveyRepository : ISurveyRepository
{
    private readonly SurveyRepository _decorated;
    private readonly IMemoryCache _cache;
    
    public CachedSurveyRepository(SurveyRepository decorated, IMemoryCache cache)
    {
        _decorated = decorated;
        _cache = cache;
    }
    
    public async Task<Survey> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var key = $"survey_{id}";
        
        if (_cache.TryGetValue(key, out Survey survey))
            return survey;
        
        survey = await _decorated.GetByIdAsync(id, cancellationToken);
        
        if (survey != null)
        {
            _cache.Set(key, survey, TimeSpan.FromMinutes(30));
        }
        
        return survey;
    }
}
```

### 7. Adding Logging

Implement structured logging:

```bash
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.File
```

**Configure in Program.cs:**

```csharp
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/survey-api-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();
```

**Use in handlers:**

```csharp
public class CreateSurveyCommandHandler
{
    private readonly ILogger<CreateSurveyCommandHandler> _logger;
    
    public async Task<Guid> Handle(CreateSurveyCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating survey with title: {Title}", command.Title);
        
        try
        {
            // Create survey
            _logger.LogInformation("Survey created successfully with ID: {SurveyId}", survey.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating survey");
            throw;
        }
    }
}
```

## Migration from Previous Implementation

If you have the previous implementation and want to migrate to Clean Architecture:

### Step-by-Step Migration

1. **Create new project structure** with 4 projects
2. **Move entities to Domain layer** - add factory methods and behavior
3. **Extract interfaces** - move to Application layer
4. **Create Commands and Queries** - define all use cases
5. **Implement Handlers** - move business logic from services
6. **Move repositories** - implement interfaces in Infrastructure
7. **Thin out controllers** - delegate to handlers
8. **Update DI registration** - register all new dependencies
9. **Test thoroughly** - ensure functionality is preserved

## Performance Considerations

### 1. Use AsNoTracking for Read Operations

```csharp
public async Task<List<Survey>> GetAllAsync(bool activeOnly, CancellationToken cancellationToken)
{
    return await _context.Surveys
        .AsNoTracking()  // Better performance for read-only queries
        .Include(s => s.Questions)
        .Where(s => !activeOnly || s.IsActive)
        .ToListAsync(cancellationToken);
}
```

### 2. Implement Pagination

```csharp
public record GetSurveysQuery(int PageNumber, int PageSize, bool ActiveOnly);

public class GetSurveysQueryHandler
{
    public async Task<PaginatedResult<SurveyDto>> Handle(GetSurveysQuery query)
    {
        var surveys = await _repository.GetPagedAsync(
            query.PageNumber, 
            query.PageSize, 
            query.ActiveOnly
        );
        
        return new PaginatedResult<SurveyDto>(
            surveys.Items,
            surveys.TotalCount,
            query.PageNumber,
            query.PageSize
        );
    }
}
```

### 3. Use Projections

Instead of loading entire entities and mapping, project directly to DTOs:

```csharp
public async Task<List<SurveyDto>> GetAllAsync(CancellationToken cancellationToken)
{
    return await _context.Surveys
        .AsNoTracking()
        .Select(s => new SurveyDto(
            s.Id,
            s.Title,
            s.Description,
            s.IsActive,
            s.Questions.Select(q => new QuestionDto(/* ... */)).ToList()
        ))
        .ToListAsync(cancellationToken);
}
```

## Deployment Checklist

### Development Environment

- [ ] All projects build successfully
- [ ] Database migrations applied
- [ ] Swagger documentation accessible
- [ ] All unit tests passing
- [ ] Integration tests passing

### Staging Environment

- [ ] Connection strings configured
- [ ] CORS origins set correctly
- [ ] Authentication configured
- [ ] Logging configured
- [ ] Health checks enabled
- [ ] Performance tested

### Production Environment

- [ ] SSL/TLS certificates installed
- [ ] Database backups configured
- [ ] Monitoring and alerting setup
- [ ] Rate limiting configured
- [ ] API versioning in place
- [ ] Documentation published

## Troubleshooting

### Issue: "The type or namespace name 'X' could not be found"

**Solution:** Check project references. Ensure proper dependencies:
- API → Application, Infrastructure
- Infrastructure → Application, Domain
- Application → Domain

### Issue: "Unable to resolve service for type 'IRepository'"

**Solution:** Check DI registration in Program.cs. Ensure all repositories and handlers are registered.

### Issue: "Migrations fail with circular reference"

**Solution:** Ensure navigation properties are properly configured in DbContext. Use `OnDelete(DeleteBehavior.Restrict)` where appropriate.

### Issue: "Domain exceptions not being caught in API"

**Solution:** Add exception middleware:

```csharp
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;
        
        context.Response.StatusCode = exception switch
        {
            DomainException => 400,
            ValidationException => 400,
            NotFoundException => 404,
            _ => 500
        };
        
        await context.Response.WriteAsJsonAsync(new { error = exception.Message });
    });
});
```

## Best Practices Summary

✅ **DO:**
- Keep domain layer pure (no dependencies)
- Use factory methods for entity creation
- Validate in domain entities
- Use value objects for complex values
- Implement unit of work for transactions
- Use CQRS for scalability
- Write unit tests for domain logic
- Use dependency injection everywhere
- Keep controllers thin
- Use async/await consistently

❌ **DON'T:**
- Put infrastructure code in domain
- Use anemic domain models (just data, no behavior)
- Put business logic in controllers
- Skip validation
- Mix commands and queries
- Use static methods for services
- Forget to dispose resources
- Expose domain entities to API
- Ignore exceptions
- Skip documentation

## Additional Resources

- [Clean Architecture by Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)
- [Domain-Driven Design by Eric Evans](https://www.domainlanguage.com/ddd/)
- [Repository Pattern](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)
- [Unit of Work Pattern](https://www.martinfowler.com/eaaCatalog/unitOfWork.html)

## Conclusion

This Clean Architecture implementation provides:

✅ **Separation of Concerns** - Each layer has a specific responsibility
✅ **Testability** - Easy to unit test without infrastructure
✅ **Maintainability** - Changes are isolated to specific layers
✅ **Flexibility** - Easy to swap implementations
✅ **Scalability** - CQRS pattern supports read/write optimization
✅ **Domain-Centric** - Business logic is protected and isolated

The architecture scales from small projects to enterprise applications while maintaining code quality and developer productivity.