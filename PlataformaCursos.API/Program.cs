using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PlataformaCursos.API.Application.Services;
using PlataformaCursos.API.Domain.Entities;
using PlataformaCursos.API.Infrastructure.Data;
using PlataformaCursos.API.Infrastructure.Middleware;
using System.Reflection;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

#region ==========================
// CONFIGURAÇÃO DE SERVIÇOS
#endregion ==========================


// ======================================================
// Controllers
// ======================================================
builder.Services.AddControllers();


// ======================================================
// ProblemDetails
// ======================================================
builder.Services.AddProblemDetails();


// ======================================================
// Swagger + JWT
// ======================================================
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(opt =>
{
	opt.SwaggerDoc("v1",
		new OpenApiInfo
		{
			Title = "Plataforma Cursos API",
			Version = "v1"
		});

	opt.AddSecurityDefinition("Bearer",
		new OpenApiSecurityScheme
		{
			Name = "Authorization",
			Type = SecuritySchemeType.Http,
			Scheme = "bearer",
			BearerFormat = "JWT",
			In = ParameterLocation.Header,
			Description = "Bearer {seu token}"
		});

	opt.AddSecurityRequirement(new OpenApiSecurityRequirement
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
			Array.Empty<string>()
		}
	});

	var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
	var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

	opt.IncludeXmlComments(xmlPath);
});


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
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());


// ======================================================
// Application Services
// ======================================================
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<CourseService>();
builder.Services.AddScoped<StudentService>();
builder.Services.AddScoped<EnrollmentService>();


// ======================================================
// JWT
// ======================================================
var jwtKey = builder.Configuration["Jwt:Key"]
	?? throw new InvalidOperationException("JWT:Key não configurada.");

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
	?? throw new InvalidOperationException("JWT:Issuer não configurado.");

var jwtAudience = builder.Configuration["Jwt:Audience"]
	?? throw new InvalidOperationException("JWT:Audience não configurado.");

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

			// 🔴 ISSO É O MAIS IMPORTANTE
			RoleClaimType = ClaimTypes.Role,
			NameClaimType = ClaimTypes.Name
		};
	});


// ======================================================
// Authorization
// ======================================================
builder.Services.AddAuthorization();


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

	var userManager =
		services.GetRequiredService<UserManager<Student>>();

	var roleManager =
		services.GetRequiredService<RoleManager<IdentityRole>>();

	await DbInitializer.SeedAsync(userManager, roleManager);
}


// ======================================================
// Error Handling
// ======================================================
app.UseExceptionHandler();

app.UseStatusCodePages();


// ======================================================
// Swagger
// ======================================================
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}


// ======================================================
// Middlewares
// ======================================================
app.UseHttpsRedirection();

app.UseMiddleware<ExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();


// ======================================================
// Rotas
// ======================================================
app.MapControllers();


// ======================================================
// Run
// ======================================================
app.Run();
