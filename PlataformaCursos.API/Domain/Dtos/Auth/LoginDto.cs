using System.ComponentModel.DataAnnotations;

namespace PlataformaCursos.API.Domain.DTOs.Auth;

public class LoginDto
{
	[Required]
	public string UserName { get; set; } = null!;

	[Required]
	public string Password { get; set; } = null!;
}
