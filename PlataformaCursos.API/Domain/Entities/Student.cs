using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace PlataformaCursos.API.Domain.Entities;

/// <summary>
/// Representa um aluno da plataforma.
/// Extende IdentityUser para integrar autenticação e dados do domínio.
/// </summary>
public class Student : IdentityUser
{
	/// <summary>
	/// Nome completo do aluno.
	/// </summary>
	[Required]
	[MaxLength(200, ErrorMessage = "Nome completo deve possuir no máximo 200 caracteres")]
	[MinLength(3, ErrorMessage = "Nome completo deve possuir no mínimo 3 caracteres")]
	public string FullName { get; set; } = null!;

	/// <summary>
	/// Data de cadastro.
	/// </summary>
	[Required]
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

	/// <summary>
	/// Indica se a conta está ativa.
	/// </summary>
	[Required]
	public bool IsActive { get; set; } = true;

	/// <summary>
	/// Soft delete.
	/// </summary>
	[Required]
	public bool IsDeleted { get; set; } = false;

	/// <summary>
	/// Matrículas do aluno.
	/// </summary>
	public ICollection<Enrollment> Enrollments { get; set; }
		= new List<Enrollment>();
}
