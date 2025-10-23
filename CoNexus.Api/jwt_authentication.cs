// ============================================
// SurveyApi.Domain - Add Authentication Entities
// ============================================

namespace SurveyApi.Domain.Entities
{
    public class ApiClient : BaseEntity
    {
        public string ClientId { get; private set; }
        public string ClientName { get; private set; }
        public string ClientSecret { get; private set; } // Hashed
        public string[] Scopes { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime? LastLoginAt { get; private set; }

        private ApiClient() { }

        public static ApiClient Create(string clientId, string clientName, string clientSecret, string[] scopes)
        {
            if (string.IsNullOrWhiteSpace(clientId))
                throw new ArgumentException("Client ID is required", nameof(clientId));
            
            if (string.IsNullOrWhiteSpace(clientSecret))
                throw new ArgumentException("Client secret is required", nameof(clientSecret));

            return new ApiClient
            {
                ClientId = clientId,
                ClientName = clientName,
                ClientSecret = clientSecret, // Should be hashed before storage
                Scopes = scopes ?? new[] { "api.read" },
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public void UpdateLastLogin()
        {
            LastLoginAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void RotateSecret(string newSecret)
        {
            ClientSecret = newSecret;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}

namespace SurveyApi.Domain.Exceptions
{
    public class InvalidCredentialsException : DomainException
    {
        public InvalidCredentialsException() 
            : base("Invalid client credentials") { }
    }

    public class InactiveClientException : DomainException
    {
        public InactiveClientException(string clientId) 
            : base($"Client {clientId} is inactive") { }
    }
}

// ============================================
// SurveyApi.Application - Authentication Services
// ============================================

namespace SurveyApi.Application.Common.Interfaces
{
    public interface IApiClientRepository
    {
        Task<ApiClient> GetByClientIdAsync(string clientId, CancellationToken cancellationToken = default);
        Task<ApiClient> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<List<ApiClient>> GetAllAsync(CancellationToken cancellationToken = default);
        Task AddAsync(ApiClient client, CancellationToken cancellationToken = default);
        Task UpdateAsync(ApiClient client, CancellationToken cancellationToken = default);
    }

    public interface IAuthenticationService
    {
        Task<AuthenticationResult> AuthenticateAsync(string clientId, string clientSecret, CancellationToken cancellationToken = default);
        string GenerateJwtToken(ApiClient client);
        Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
    }

    public interface IPasswordHasher
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
    }
}

namespace SurveyApi.Application.DTOs
{
    public record AuthenticationResult(
        bool Success,
        string AccessToken,
        string TokenType,
        int ExpiresIn,
        string[] Scopes,
        string Error
    );

    public record ApiClientDto(
        int Id,
        string ClientId,
        string ClientName,
        string[] Scopes,
        bool IsActive,
        DateTime? LastLoginAt
    );
}

namespace SurveyApi.Application.Commands
{
    public record AuthenticateCommand(string ClientId, string ClientSecret);

    public record CreateApiClientCommand(
        string ClientId,
        string ClientName,
        string ClientSecret,
        string[] Scopes
    );

    public record RevokeClientCommand(int ClientId);

    public record RotateClientSecretCommand(int ClientId, string NewSecret);
}

namespace SurveyApi.Application.Handlers.Commands
{
    using SurveyApi.Application.Commands;
    using SurveyApi.Application.Common.Interfaces;
    using SurveyApi.Application.DTOs;
    using SurveyApi.Domain.Entities;
    using SurveyApi.Domain.Exceptions;

    public class AuthenticateCommandHandler
    {
        private readonly IAuthenticationService _authService;

        public AuthenticateCommandHandler(IAuthenticationService authService)
        {
            _authService = authService;
        }

        public async Task<AuthenticationResult> Handle(AuthenticateCommand command, CancellationToken cancellationToken)
        {
            return await _authService.AuthenticateAsync(command.ClientId, command.ClientSecret, cancellationToken);
        }
    }

    public class CreateApiClientCommandHandler
    {
        private readonly IApiClientRepository _clientRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUnitOfWork _unitOfWork;

        public CreateApiClientCommandHandler(
            IApiClientRepository clientRepository,
            IPasswordHasher passwordHasher,
            IUnitOfWork unitOfWork)
        {
            _clientRepository = clientRepository;
            _passwordHasher = passwordHasher;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiClientDto> Handle(CreateApiClientCommand command, CancellationToken cancellationToken)
        {
            // Check if client ID already exists
            var existing = await _clientRepository.GetByClientIdAsync(command.ClientId, cancellationToken);
            if (existing != null)
                throw new ValidationException("Client ID already exists");

            var hashedSecret = _passwordHasher.HashPassword(command.ClientSecret);
            var client = ApiClient.Create(command.ClientId, command.ClientName, hashedSecret, command.Scopes);

            await _clientRepository.AddAsync(client, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ApiClientDto(
                client.Id,
                client.ClientId,
                client.ClientName,
                client.Scopes,
                client.IsActive,
                client.LastLoginAt
            );
        }
    }

    public class RevokeClientCommandHandler
    {
        private readonly IApiClientRepository _clientRepository;
        private readonly IUnitOfWork _unitOfWork;

        public RevokeClientCommandHandler(IApiClientRepository clientRepository, IUnitOfWork unitOfWork)
        {
            _clientRepository = clientRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(RevokeClientCommand command, CancellationToken cancellationToken)
        {
            var client = await _clientRepository.GetByIdAsync(command.ClientId, cancellationToken);
            if (client == null)
                return false;

            client.Deactivate();
            await _clientRepository.UpdateAsync(client, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}

// ============================================
// SurveyApi.Infrastructure - Authentication Implementation
// ============================================

namespace SurveyApi.Infrastructure.Persistence
{
    public partial class SurveyDbContext
    {
        public DbSet<ApiClient> ApiClients { get; set; }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {
            // ApiClient configuration
            modelBuilder.Entity<ApiClient>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ClientId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ClientName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.ClientSecret).IsRequired().HasMaxLength(500);
                entity.Property(e => e.IsActive).IsRequired();
                
                entity.HasIndex(e => e.ClientId).IsUnique();

                // Store scopes as JSON
                entity.Property(e => e.Scopes)
                    .HasConversion(
                        v => System.Text.Json.JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                        v => System.Text.Json.JsonSerializer.Deserialize<string[]>(v, (JsonSerializerOptions)null))
                    .HasMaxLength(1000);
            });
        }
    }

    public class ApiClientRepository : IApiClientRepository
    {
        private readonly SurveyDbContext _context;

        public ApiClientRepository(SurveyDbContext context)
        {
            _context = context;
        }

        public async Task<ApiClient> GetByClientIdAsync(string clientId, CancellationToken cancellationToken = default)
        {
            return await _context.ApiClients
                .FirstOrDefaultAsync(c => c.ClientId == clientId, cancellationToken);
        }

        public async Task<ApiClient> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.ApiClients.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<List<ApiClient>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.ApiClients
                .Where(c => c.IsActive)
                .OrderBy(c => c.ClientName)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(ApiClient client, CancellationToken cancellationToken = default)
        {
            await _context.ApiClients.AddAsync(client, cancellationToken);
        }

        public Task UpdateAsync(ApiClient client, CancellationToken cancellationToken = default)
        {
            _context.ApiClients.Update(client);
            return Task.CompletedTask;
        }
    }
}

namespace SurveyApi.Infrastructure.Services
{
    using Microsoft.IdentityModel.Tokens;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;
    using SurveyApi.Application.Common.Interfaces;
    using SurveyApi.Application.DTOs;
    using SurveyApi.Domain.Entities;
    using SurveyApi.Domain.Exceptions;

    public class JwtAuthenticationService : IAuthenticationService
    {
        private readonly IApiClientRepository _clientRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;

        public JwtAuthenticationService(
            IApiClientRepository clientRepository,
            IPasswordHasher passwordHasher,
            IConfiguration configuration,
            IUnitOfWork unitOfWork)
        {
            _clientRepository = clientRepository;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
            _unitOfWork = unitOfWork;
        }

        public async Task<AuthenticationResult> AuthenticateAsync(
            string clientId, 
            string clientSecret, 
            CancellationToken cancellationToken = default)
        {
            var client = await _clientRepository.GetByClientIdAsync(clientId, cancellationToken);

            if (client == null)
            {
                return new AuthenticationResult(false, null, null, 0, null, "Invalid credentials");
            }

            if (!client.IsActive)
            {
                return new AuthenticationResult(false, null, null, 0, null, "Client is inactive");
            }

            if (!_passwordHasher.VerifyPassword(clientSecret, client.ClientSecret))
            {
                return new AuthenticationResult(false, null, null, 0, null, "Invalid credentials");
            }

            // Update last login
            client.UpdateLastLogin();
            await _clientRepository.UpdateAsync(client, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Generate JWT token
            var token = GenerateJwtToken(client);
            var expiresIn = int.Parse(_configuration["Jwt:ExpiresInMinutes"] ?? "60") * 60;

            return new AuthenticationResult(
                true,
                token,
                "Bearer",
                expiresIn,
                client.Scopes,
                null
            );
        }

        public string GenerateJwtToken(ApiClient client)
        {
            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, client.ClientId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("client_id", client.ClientId),
                new Claim("client_name", client.ClientName)
            };

            // Add scopes as claims
            foreach (var scope in client.Scopes)
            {
                claims.Add(new Claim("scope", scope));
            }

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpiresInMinutes"] ?? "60")),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]);

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    public class PasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}

// ============================================
// SurveyApi.API - Authentication Controller
// ============================================

namespace SurveyApi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthenticateCommandHandler _authenticateHandler;
        private readonly CreateApiClientCommandHandler _createClientHandler;
        private readonly RevokeClientCommandHandler _revokeClientHandler;

        public AuthController(
            AuthenticateCommandHandler authenticateHandler,
            CreateApiClientCommandHandler createClientHandler,
            RevokeClientCommandHandler revokeClientHandler)
        {
            _authenticateHandler = authenticateHandler;
            _createClientHandler = createClientHandler;
            _revokeClientHandler = revokeClientHandler;
        }

        /// <summary>
        /// Authenticate and obtain access token
        /// </summary>
        [HttpPost("token")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthenticationResult), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetToken([FromBody] TokenRequest request, CancellationToken cancellationToken)
        {
            if (request.GrantType != "client_credentials")
            {
                return BadRequest(new { error = "unsupported_grant_type" });
            }

            var command = new AuthenticateCommand(request.ClientId, request.ClientSecret);
            var result = await _authenticateHandler.Handle(command, cancellationToken);

            if (!result.Success)
            {
                return Unauthorized(new { error = "invalid_client", error_description = result.Error });
            }

            return Ok(new
            {
                access_token = result.AccessToken,
                token_type = result.TokenType,
                expires_in = result.ExpiresIn,
                scope = string.Join(" ", result.Scopes)
            });
        }

        /// <summary>
        /// Create new API client (Admin only)
        /// </summary>
        [HttpPost("clients")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(ApiClientDto), 201)]
        public async Task<IActionResult> CreateClient([FromBody] CreateClientRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var command = new CreateApiClientCommand(
                    request.ClientId,
                    request.ClientName,
                    request.ClientSecret,
                    request.Scopes ?? new[] { "api.read", "api.write" }
                );

                var client = await _createClientHandler.Handle(command, cancellationToken);
                return CreatedAtAction(nameof(GetClient), new { id = client.Id }, client);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get API client by ID (Admin only)
        /// </summary>
        [HttpGet("clients/{id}")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(ApiClientDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetClient(int id)
        {
            // Implementation would fetch from repository
            return Ok();
        }

        /// <summary>
        /// Revoke API client (Admin only)
        /// </summary>
        [HttpDelete("clients/{id}")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> RevokeClient(int id, CancellationToken cancellationToken)
        {
            var command = new RevokeClientCommand(id);
            var result = await _revokeClientHandler.Handle(command, cancellationToken);

            if (!result)
                return NotFound();

            return NoContent();
        }
    }

    public record TokenRequest(
        string GrantType,
        string ClientId,
        string ClientSecret
    );

    public record CreateClientRequest(
        string ClientId,
        string ClientName,
        string ClientSecret,
        string[] Scopes
    );
}

// ============================================
// Program.cs - JWT Configuration
// ============================================

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with JWT support
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Survey API",
        Version = "v1",
        Description = "Clean Architecture Survey Management API with JWT Authentication"
    });

    // Add JWT Authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Database
builder.Services.AddDbContext<CnxDb>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("Default"),
        b => b.MigrationsAssembly("SurveyApi.Infrastructure")
    ));

// JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"])),
        ClockSkew = TimeSpan.Zero
    };
});

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireClaim("scope", "admin"));

    options.AddPolicy("CrmAccess", policy =>
        policy.RequireClaim("scope", "crm.sync"));

    options.AddPolicy("ApiWrite", policy =>
        policy.RequireClaim("scope", "api.write"));

    options.AddPolicy("ApiRead", policy =>
        policy.RequireClaim("scope", "api.read"));
});

// Infrastructure - Repositories
builder.Services.AddScoped<IApiClientRepository, ApiClientRepository>();
// ... existing repositories

// Infrastructure - Services
builder.Services.AddScoped<IAuthenticationService, JwtAuthenticationService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
// ... existing services

// Application - Command Handlers
builder.Services.AddScoped<AuthenticateCommandHandler>();
builder.Services.AddScoped<CreateApiClientCommandHandler>();
builder.Services.AddScoped<RevokeClientCommandHandler>();
// ... existing handlers

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

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("CrmPolicy");

// Authentication & Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();