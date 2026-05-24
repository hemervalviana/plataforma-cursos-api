using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlataformaCursos.API.Infrastructure.Data;

namespace PlataformaCursos.Tests.Integration;

public class ApiFactory : WebApplicationFactory<Program>
{
	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.UseEnvironment("Test");

		builder.ConfigureAppConfiguration((context, config) =>
		{
			// Carrega o appsettings.Test.json do projeto de testes
			config.AddJsonFile(
				Path.Combine(Directory.GetCurrentDirectory(), "appsettings.Test.json"),
				optional: false,
				reloadOnChange: false);
		});

		builder.ConfigureServices(services =>
		{
			// Remove o DbContext original
			var descriptor = services.SingleOrDefault(d =>
				d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

			if (descriptor != null)
				services.Remove(descriptor);

			// Registra DbContext apontando para banco de teste
			services.AddDbContext<ApplicationDbContext>(opt =>
			{
				opt.UseSqlServer(
					"Server=127.0.0.1;Database=PlataformaCursosDb_Test;User Id=sa;Password=Hamashia191079#;TrustServerCertificate=True");
			});

			// Garante que o banco de teste existe e está migrado
			var sp = services.BuildServiceProvider();
			using var scope = sp.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
			db.Database.Migrate();
		});
	}

	public void LimparPagamentos()
	{
		using var scope = Services.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		db.Payments.RemoveRange(db.Payments.IgnoreQueryFilters());
		db.SaveChanges();
	}
}