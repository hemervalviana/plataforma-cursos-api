using Microsoft.AspNetCore.Identity;
using PlataformaCursos.API.Domain.Entities;

namespace PlataformaCursos.API.Infrastructure.Data;

public static class DbInitializer
{
	public static async Task SeedRolesAndAdminAsync(
		UserManager<Student> userManager,
		RoleManager<IdentityRole> roleManager)
	{
		// ===== 1. Criar Roles =====
		string[] roles = new[] { "Admin", "Instructor", "Student" };

		foreach (var role in roles)
		{
			if (!await roleManager.RoleExistsAsync(role))
			{
				await roleManager.CreateAsync(new IdentityRole(role));
			}
		}

		// ===== 2. Criar usuário admin =====
		string adminEmail = "admin@plataforma.com";
		string adminPassword = "Admin123!"; // Use segredo seguro em produção

		var adminUser = await userManager.FindByEmailAsync(adminEmail);
		if (adminUser == null)
		{
			var admin = new Student
			{
				UserName = adminEmail,
				Email = adminEmail,
				FullName = "Administrador",
				CreatedAt = DateTime.UtcNow,
				IsActive = true
			};

			var result = await userManager.CreateAsync(admin, adminPassword);
			if (result.Succeeded)
			{
				await userManager.AddToRoleAsync(admin, "Admin");
			}
		}
	}
}
