using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PlataformaCursos.API.Application.Interfaces;
using PlataformaCursos.API.Application.Services;
using PlataformaCursos.API.Infrastructure.Data;
using PlataformaCursos.API.Domain.Entities;
using PlataformaCursos.API.Infrastructure.Gateways;
using PlataformaCursos.API.Infrastructure.Middleware;
using Scalar.AspNetCore;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ======================================================
// Controllers
// ======================================================
builder.Services.AddControllers();

// ======================================================
// Validators (FluentValidation)
// ======================================================
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// ======================================================
// ProblemDetails
// ======================================================
builder.Services.AddProblemDetails();

// ======================================================
// OpenAPI (nativo .NET 10)
// ======================================================
builder.Services.AddOpenApi();

// ======================================================
// DbContext
// ======================================================
builder.Services.AddDbContext<ApplicationDbContext>(opt =>
{
	opt.UseSqlServer(
		builder.Configuration.GetConnectionString("Default"));
});

// ======================================================
// Identity
// ======================================================
builder.Services
	.AddIdentity<Student, IdentityRole>(opt =>
	{
		opt.Password.RequireDigit = true;
		opt.Password.RequiredLength = 8;
		opt.Password.RequireNonAlphanumeric = false;
		opt.Password.RequireUppercase = true;
		opt.Password.RequireLowercase = true;

		opt.Lockout.MaxFailedAccessAttempts = 5;
		opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);

		opt.User.RequireUniqueEmail = true;
	})
	.AddEntityFrameworkStores<ApplicationDbContext>()
	.AddDefaultTokenProviders();

// ======================================================
// AutoMapper
// ======================================================
builder.Services.AddAutoMapper(cfg =>
{
	cfg.AddMaps(typeof(Program).Assembly);
});

// ======================================================
// Application Services
// ======================================================
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<CourseService>();
builder.Services.AddScoped<StudentService>();
builder.Services.AddScoped<EnrollmentService>();
builder.Services.AddScoped<PaymentService>();

// ======================================================
// Repositórios
// ======================================================
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

// ======================================================
// Gateway simulado — troque por StripePaymentGateway quando necessário
// ======================================================
builder.Services.AddScoped<IPaymentGateway, FakePaymentGateway>();

// ======================================================
// JWT
// ======================================================

// Fallback para testes — em produção sempre virá do user-secrets
// ou variáveis de ambiente
var jwtKey = builder.Configuration["Jwt:Key"]
	?? "chave-secreta-para-testes-deve-ter-32-chars!!";

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
	?? "PlataformaCursosAPI";

var jwtAudience = builder.Configuration["Jwt:Audience"]
	?? "PlataformaCursosClient";

var key = new SymmetricSecurityKey(
	Encoding.UTF8.GetBytes(jwtKey));

builder.Services
	.AddAuthentication(opt =>
	{
		opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
		opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
	})
	.AddJwtBearer(opt =>
	{
		opt.Events = new JwtBearerEvents
		{
			OnAuthenticationFailed = ctx =>
			{
				Console.WriteLine("JWT FAILED: " + ctx.Exception.Message);
				return Task.CompletedTask;
			},
			OnTokenValidated = ctx =>
			{
				Console.WriteLine("JWT OK");
				return Task.CompletedTask;
			}
		};

		opt.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuerSigningKey = true,
			IssuerSigningKey = key,

			ValidateIssuer = true,
			ValidIssuer = jwtIssuer,

			ValidateAudience = true,
			ValidAudience = jwtAudience,

			ValidateLifetime = true,

			ClockSkew = TimeSpan.Zero,

			RoleClaimType = ClaimTypes.Role,
			NameClaimType = ClaimTypes.Name
		};
	});

// ======================================================
// Authorization
// ======================================================
builder.Services.AddAuthorization();

// ======================================================
// Health Checks
// ======================================================
builder.Services.AddHealthChecks()
	.AddSqlServer(
		connectionString: builder.Configuration.GetConnectionString("Default")
			?? "Server=127.0.0.1;Database=PlataformaCursosDb;User Id=sa;Password=placeholder",
		name: "sqlserver",
		tags: ["db", "sql"]);

// ======================================================
// BUILD
// ======================================================
var app = builder.Build();

// ======================================================
// Seed (Somente em Development)
// ======================================================
if (app.Environment.IsDevelopment())
{
	using var scope = app.Services.CreateScope();
	var services = scope.ServiceProvider;

	var userManager = services.GetRequiredService<UserManager<Student>>();
	var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

	await DbInitializer.SeedAsync(userManager, roleManager);
}

// ======================================================
// Error Handling
// ======================================================
app.UseExceptionHandler();
app.UseStatusCodePages();

// ======================================================
// OpenAPI + Scalar UI
// ======================================================
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
	app.MapScalarApiReference(opt =>
	{
		opt.Title = "Plataforma Cursos API";
		opt.Theme = ScalarTheme.Purple;
		opt.WithHttpBearerAuthentication(bearer =>
		{
			bearer.Token = "seu-token-jwt-aqui";
		});
	});
}

// ======================================================
// Middlewares
// ======================================================
app.UseHttpsRedirection();

app.UseMiddleware<ExceptionMiddleware>();

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

// ======================================================
// Health Check Endpoints
// ======================================================
app.MapHealthChecks("/health");

app.MapHealthChecks("/health/db", new HealthCheckOptions
{
	Predicate = check => check.Tags.Contains("db")
});

// ======================================================
// Rotas
// ======================================================
app.MapControllers();

// ======================================================
// Run
// ======================================================
app.Run();