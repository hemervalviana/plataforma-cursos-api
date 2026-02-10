using System.ComponentModel.DataAnnotations;

namespace PlataformaCursos.API.Domain.DTOs.Students;

public class CreateStudentDto
{
	[Required]
	[MaxLength(200)]
	[MinLength(3)]
	public string FullName { get; set; } = null!;

	[Required]
	[EmailAddress]
	public string Email { get; set; } = null!;

	[Required]
	[MinLength(6)]
	public string Password { get; set; } = null!;
}
