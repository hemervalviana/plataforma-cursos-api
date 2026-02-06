using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PlataformaCursos.API.Application.Services;
using PlataformaCursos.API.Domain.Entities;
using PlataformaCursos.API.Infrastructure.Data;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ======================================================
// CONFIGURAÇÃO DE SERVIÇOS (DEPENDENCY INJECTION)
// ======================================================
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<AuthService>();


// --------------------------
// Controllers (Web API)
// --------------------------
builder.Services.AddControllers();


// --------------------------
// Swagger (Documentação)
// --------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// --------------------------
// DbContext (SQL Server via User Secrets / Variáveis)
// --------------------------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseSqlServer(
		builder.Configuration.GetConnectionString("Default")
	)
);


// --------------------------
// ASP.NET Identity
// --------------------------
builder.Services.AddIdentity<Student, IdentityRole>(options =>
{
	// Regras de senha
	options.Password.RequireDigit = true;
	options.Password.RequiredLength = 8;
	options.Password.RequireNonAlphanumeric = false;
	options.Password.RequireUppercase = true;
	options.Password.RequireLowercase = true;

	// Lockout (bloqueio por tentativas)
	options.Lockout.MaxFailedAccessAttempts = 5;
	options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
	options.Lockout.AllowedForNewUsers = true;

	// Email único
	options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();


// --------------------------
// AutoMapper
// --------------------------
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());


// --------------------------
// JWT Authentication
// --------------------------

// Lê configurações do User Secrets / Environment
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

// Validação de segurança
if (string.IsNullOrEmpty(jwtKey))
{
	throw new InvalidOperationException("JWT Key não configurada.");
}

var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services
	.AddAuthentication(options =>
	{
		options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
		options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
	})
	.AddJwtBearer(options =>
	{
		options.TokenValidationParameters = new TokenValidationParameters
		{
			// Valida assinatura
			ValidateIssuerSigningKey = true,
			IssuerSigningKey = key,

			// Valida emissor
			ValidateIssuer = true,
			ValidIssuer = jwtIssuer,

			// Valida audiência
			ValidateAudience = true,
			ValidAudience = jwtAudience,

			// Valida expiração
			ValidateLifetime = true,

			// Tempo extra permitido (clock skew)
			ClockSkew = TimeSpan.Zero
		};
	});


// --------------------------
// Autorização
// --------------------------
builder.Services.AddAuthorization();


// ======================================================
// BUILD DA APLICAÇÃO
// ======================================================

var app = builder.Build();


// ======================================================
// PIPELINE HTTP (MIDDLEWARES)
// ======================================================


// --------------------------
// Swagger (Somente em Dev)
// --------------------------
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}


// --------------------------
// HTTPS
// --------------------------
app.UseHttpsRedirection();


// --------------------------
// Autenticação / Autorização
// --------------------------
app.UseAuthentication();
app.UseAuthorization();


// --------------------------
// Rotas
// --------------------------
app.MapControllers();


// ======================================================
// EXECUÇÃO
// ======================================================

app.Run();
