namespace PlataformaCursos.API.Domain.DTOs.Students;

public class StudentResponseDto
{
	public string Id { get; set; } = null!;
	public string FullName { get; set; } = null!;
	public string Email { get; set; } = null!;
	public bool IsActive { get; set; }
	public DateTime CreatedAt { get; set; }
}
