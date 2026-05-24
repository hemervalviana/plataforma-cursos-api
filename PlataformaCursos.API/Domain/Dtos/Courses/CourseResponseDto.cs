namespace PlataformaCursos.API.Domain.DTOs.Courses;

public class CourseResponseDto
{
	public Guid Id { get; set; }

	public string Title { get; set; } = null!;
	public string? Description { get; set; }

	public string Category { get; set; } = null!;
	public int Workload { get; set; }

	public DateTime CreatedAt { get; set; }
}
