using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using PlataformaCursos.API.Domain.Entities;

namespace PlataformaCursos.API.Application.Services;

public class TokenService
{
	private readonly IConfiguration _configuration;

	public TokenService(IConfiguration configuration)
	{
		_configuration = configuration;
	}

	public string GenerateToken(
		Student user,
		IList<string> roles)
	{
		var claims = new List<Claim>
		{
			new Claim(
				ClaimTypes.NameIdentifier,
				user.Id),

			new Claim(
				ClaimTypes.Name,
				user.UserName!),

			new Claim(
				ClaimTypes.Email,
				user.Email!)
		};

		foreach (var role in roles)
		{
			claims.Add(
				new Claim(ClaimTypes.Role, role));
		}

		var key = new SymmetricSecurityKey(
			Encoding.UTF8.GetBytes(
				_configuration["Jwt:Key"]!));

		var credentials =
			new SigningCredentials(
				key,
				SecurityAlgorithms.HmacSha256);

		var token = new JwtSecurityToken(
			issuer: _configuration["Jwt:Issuer"],
			audience: _configuration["Jwt:Audience"],
			claims: claims,
			expires: DateTime.UtcNow.AddHours(2),
			signingCredentials: credentials);

		return new JwtSecurityTokenHandler()
			.WriteToken(token);
	}
}
