using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PlataformaCursos.Tests.Integration;

/// <summary>
/// Gera tokens JWT de teste por papel (role).
/// Usa a mesma chave e configuração da API.
/// </summary>
public static class TokenHelper
{
	private const string Key = "chave-secreta-para-testes-deve-ter-32-chars!!";
	private const string Issuer = "PlataformaCursosAPI";
	private const string Audience = "PlataformaCursosClient";

	public static string GerarToken(
		string userId,
		string email,
		string role)
	{
		var claims = new[]
		{
			new Claim(ClaimTypes.NameIdentifier, userId),
			new Claim(ClaimTypes.Name, email),
			new Claim(ClaimTypes.Email, email),
			new Claim(ClaimTypes.Role, role)
		};

		var keyBytes = new SymmetricSecurityKey(
			Encoding.UTF8.GetBytes(Key));

		var credentials = new SigningCredentials(
			keyBytes,
			SecurityAlgorithms.HmacSha256);

		var token = new JwtSecurityToken(
			issuer: Issuer,
			audience: Audience,
			claims: claims,
			expires: DateTime.UtcNow.AddHours(1),
			signingCredentials: credentials);

		return new JwtSecurityTokenHandler().WriteToken(token);
	}

	public static string TokenAdmin(string userId = "admin-test-id") =>
		GerarToken(userId, "admin@test.com", "Admin");

	public static string TokenStudent(string userId = "student-test-id") =>
		GerarToken(userId, "student@test.com", "Student");
}