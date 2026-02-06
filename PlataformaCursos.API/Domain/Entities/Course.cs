using System.ComponentModel.DataAnnotations;

namespace PlataformaCursos.API.Domain.Entities;

/// <summary>
/// Representa um curso oferecido na plataforma.
/// </summary>
public class Course
{
	[Key]
	[Required]
	public Guid Id { get; set; }

	[Required]
	[MaxLength(200)]
	[MinLength(10)]
	public string Title { get; set; } = null!;

	[MaxLength(2000)]
	public string? Description { get; set; } = "";

	[Required]
	[MaxLength(100)]
	[MinLength(3)]
	public string Category { get; set; } = null!;

	[Range(1, int.MaxValue)]
	public int Workload { get; set; }

	[Required]
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

	public bool IsDeleted { get; set; } = false;

	public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
