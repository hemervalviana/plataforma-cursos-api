using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PlataformaCursos.API.Application.Common;
using PlataformaCursos.API.Domain.DTOs.Enrollments;
using PlataformaCursos.API.Domain.Entities;
using PlataformaCursos.API.Infrastructure.Data;
using System.Security.Claims;

namespace PlataformaCursos.API.Application.Services;

public class EnrollmentService
{
	private readonly ApplicationDbContext _context;
	private readonly UserManager<Student> _userManager;

	public EnrollmentService(
		ApplicationDbContext context,
		UserManager<Student> userManager)
	{
		_context = context;
		_userManager = userManager;
	}

	// ==================================================
	// CREATE - Matrícula
	// ==================================================
	public async Task<EnrollmentResponseDto> EnrollAsync(
		CreateEnrollmentDto dto,
		ClaimsPrincipal user)
	{
		var isAdmin = user.IsInRole("Admin");

		// Define o aluno
		var studentId = isAdmin
			? dto.StudentId
			: user.FindFirstValue(ClaimTypes.NameIdentifier);

		if (string.IsNullOrWhiteSpace(studentId))
			throw new ApplicationException("Aluno inválido.");

		// Valida aluno
		var student = await _userManager.FindByIdAsync(studentId);

		if (student == null || !student.IsActive || student.IsDeleted)
			throw new ApplicationException("Aluno não encontrado ou inativo.");

		// Valida curso
		var course = await _context.Courses
			.FirstOrDefaultAsync(c =>
				c.Id == dto.CourseId &&
				!c.IsDeleted);

		if (course == null)
			throw new ApplicationException("Curso não encontrado.");

		// Verifica duplicidade
		var exists = await _context.Enrollments
			.AnyAsync(e =>
				e.StudentId == studentId &&
				e.CourseId == dto.CourseId);

		if (exists)
			throw new InvalidOperationException("Aluno já matriculado neste curso.");

		// Cria matrícula
		var enrollment = new Enrollment
		{
			Id = Guid.NewGuid(),
			StudentId = studentId,
			CourseId = dto.CourseId,
			Status = "Ativo",
			CreatedAt = DateTime.UtcNow
		};

		_context.Enrollments.Add(enrollment);
		await _context.SaveChangesAsync();

		return new EnrollmentResponseDto
		{
			Id = enrollment.Id,
			CourseId = course.Id,
			CourseTitle = course.Title,
			Status = enrollment.Status,
			CreatedAt = enrollment.CreatedAt
		};
	}


	// ==================================================
	// READ - Matrículas do aluno (Paginado)
	// ==================================================
	public async Task<PagedResult<EnrollmentResponseDto>> GetByStudentAsync(
		string studentId,
		int page,
		int pageSize,
		string? status)
	{
		var query = _context.Enrollments
			.Include(e => e.Course)
			.Where(e =>
				e.StudentId == studentId &&
				!e.IsDeleted)
			.AsQueryable();

		// Filtro por status
		if (!string.IsNullOrWhiteSpace(status))
		{
			query = query.Where(e => e.Status == status);
		}

		var total = await query.CountAsync();

		var data = await query
			.OrderByDescending(e => e.CreatedAt)
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.Select(e => new EnrollmentResponseDto
			{
				Id = e.Id,
				CourseId = e.CourseId,
				CourseTitle = e.Course.Title,
				Status = e.Status,
				CreatedAt = e.CreatedAt
			})
			.ToListAsync();

		return new PagedResult<EnrollmentResponseDto>
		{
			Page = page,
			PageSize = pageSize,
			Total = total,
			Data = data
		};
	}


	// ==================================================
	// DELETE - Cancelamento
	// ==================================================
	public async Task<bool> CancelAsync(Guid id)
	{
		var enrollment = await _context.Enrollments
			.FirstOrDefaultAsync(e =>
				e.Id == id &&
				!e.IsDeleted);

		if (enrollment == null)
			return false;

		enrollment.Status = "Cancelado";
		enrollment.IsDeleted = true;

		await _context.SaveChangesAsync();

		return true;
	}
}
