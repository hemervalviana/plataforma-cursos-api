namespace PlataformaCursos.API.Domain.DTOs.Enrollments;

public class CreateEnrollmentDto
{
	// Só Admin pode usar
	public string? StudentId { get; set; }

	public Guid CourseId { get; set; }
}
