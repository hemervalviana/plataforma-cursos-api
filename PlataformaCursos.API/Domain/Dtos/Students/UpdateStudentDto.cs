using System.ComponentModel.DataAnnotations;

namespace PlataformaCursos.API.Domain.DTOs.Students;

public class UpdateStudentDto
{
	[Required]
	[MaxLength(200)]
	[MinLength(3)]
	public string FullName { get; set; } = null!;
}
