using System.ComponentModel.DataAnnotations;

namespace PlataformaCursos.API.Domain.DTOs.Courses;

public class CreateCourseDto
{
	[Required]
	[MaxLength(200)]
	[MinLength(10)]
	public string Title { get; set; } = null!;

	[MaxLength(2000)]
	public string? Description { get; set; }

	[Required]
	[MaxLength(100)]
	public string Category { get; set; } = null!;

	[Range(1, int.MaxValue)]
	public int Workload { get; set; }
}
