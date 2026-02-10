using Microsoft.AspNetCore.Identity;
using PlataformaCursos.API.Domain.Entities;

namespace PlataformaCursos.API.Infrastructure.Data;

public static class DbInitializer
{
	public static async Task SeedAsync(
		UserManager<Student> userManager,
		RoleManager<IdentityRole> roleManager)
	{
		// ===============================
		// 1. Criar Roles
		// ===============================
		string[] roles = { "Admin", "Instructor", "Student" };

		foreach (var role in roles)
		{
			if (!await roleManager.RoleExistsAsync(role))
			{
				await roleManager.CreateAsync(new IdentityRole(role));
			}
		}


		// ===============================
		// 2. Criar Admin
		// ===============================
		await CreateUserIfNotExists(
			userManager,
			email: "admin@plataforma.com",
			password: "Admin123!",
			fullName: "Administrador",
			role: "Admin"
		);


		// ===============================
		// 3. Criar Instructor
		// ===============================
		await CreateUserIfNotExists(
			userManager,
			email: "instructor@plataforma.com",
			password: "Instructor123!",
			fullName: "Instrutor Padrão",
			role: "Instructor"
		);


		// ===============================
		// 4. Criar Student (usuário comum)
		// ===============================
		await CreateUserIfNotExists(
			userManager,
			email: "student@plataforma.com",
			password: "Student123!",
			fullName: "Aluno Padrão",
			role: "Student"
		);
	}


	// ===============================
	// Método auxiliar
	// ===============================
	private static async Task CreateUserIfNotExists(
		UserManager<Student> userManager,
		string email,
		string password,
		string fullName,
		string role)
	{
		var user = await userManager.FindByEmailAsync(email);

		if (user != null)
			return;

		var newUser = new Student
		{
			UserName = email,
			Email = email,
			FullName = fullName,
			CreatedAt = DateTime.UtcNow,
			IsActive = true
		};

		var result = await userManager.CreateAsync(newUser, password);

		if (!result.Succeeded)
			return;

		await userManager.AddToRoleAsync(newUser, role);
	}
}
