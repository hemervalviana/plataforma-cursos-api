using System.ComponentModel.DataAnnotations;

namespace PlataformaCursos.API.Domain.Entities;

/// <summary>
/// Representa a matrícula de um aluno em um curso.
/// </summary>
public class Enrollment
{
	[Key]
	public Guid Id { get; set; }

	[Required]
	public Guid CourseId { get; set; }
	public Course Course { get; set; } = null!;

	[Required]
	public string StudentId { get; set; } = null!;
	public Student Student { get; set; } = null!;

	[Required]
	[MaxLength(50)]
	public string Status { get; set; } = "Ativo";

	[Required]
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

	public bool IsDeleted { get; set; } = false;
}
