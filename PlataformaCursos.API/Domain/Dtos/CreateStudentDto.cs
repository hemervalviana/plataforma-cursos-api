using System.ComponentModel.DataAnnotations;

namespace PlataformaCursos.API.Domain.Dtos
{
	/// <summary>
	/// DTO para cadastro de um novo Student.
	/// </summary>
	public class CreateStudentDto
	{
		[Required]
		[MaxLength(200)]
		public string FullName { get; set; } = null!;

		[Required]
		[EmailAddress]
		public string Email { get; set; } = null!;

		[Required]
		[MinLength(8)]
		public string Password { get; set; } = null!;
	}
}
