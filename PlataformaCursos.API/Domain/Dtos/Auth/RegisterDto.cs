using System.ComponentModel.DataAnnotations;

namespace PlataformaCursos.API.Domain.DTOs.Auth;

public class RegisterDto
{
	[Required]
	[MaxLength(256, ErrorMessage = "Nome de usuário deve possuir no máximo 256 caracteres")]
	[MinLength(3, ErrorMessage = "Nome de usuário deve possuir no mínimo 3 caracteres")]
	public string UserName { get; set; } = null!;

	[Required]
	[EmailAddress (ErrorMessage = "O email fornecido não é válido")]
	public string Email { get; set; } = null!;

	// ✅ ADICIONAR
	[Required]
	[MaxLength(200, ErrorMessage = "Nome completo deve possuir no máximo 200 caracteres")]
	[MinLength(3, ErrorMessage = "Nome completo deve possuir no mínimo 3 caracteres")]
	public string FullName { get; set; } = null!;

	[Required]
	[MinLength(8,ErrorMessage = "A senha deve possuir no mínimo 8 caracteres")]
	public string Password { get; set; } = null!;
}
