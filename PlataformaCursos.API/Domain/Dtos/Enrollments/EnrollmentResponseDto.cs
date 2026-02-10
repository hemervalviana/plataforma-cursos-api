namespace PlataformaCursos.API.Domain.DTOs.Enrollments;

public class EnrollmentResponseDto
{
	public Guid Id { get; set; }

	public Guid CourseId { get; set; }
	public string CourseTitle { get; set; } = null!;

	public string Status { get; set; } = null!;

	public DateTime CreatedAt { get; set; }
}
