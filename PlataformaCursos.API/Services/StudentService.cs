using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PlataformaCursos.API.Domain.DTOs.Students;
using PlataformaCursos.API.Domain.Entities;
using System.Security.Claims;

namespace PlataformaCursos.API.Application.Services;

public class StudentService
{
	private readonly UserManager<Student> _userManager;

	public StudentService(UserManager<Student> userManager)
	{
		_userManager = userManager;
	}

	// =========================
	// CREATE (Admin)
	// =========================
	public async Task<StudentResponseDto?> CreateAsync(CreateStudentDto dto)
	{
		if (await _userManager.FindByEmailAsync(dto.Email) != null)
			return null;

		var student = new Student
		{
			UserName = dto.Email,
			Email = dto.Email,
			FullName = dto.FullName,
			CreatedAt = DateTime.UtcNow,
			IsActive = true
		};

		var result =
			await _userManager.CreateAsync(student, dto.Password);

		if (!result.Succeeded)
			return null;

		await _userManager.AddToRoleAsync(student, "Student");

		return Map(student);
	}

	// =========================
	// GET ALL (Admin)
	// =========================
	public async Task<List<StudentResponseDto>> GetAllAsync()
	{
		return await _userManager.Users
			.Where(x => !x.IsDeleted)
			.Select(x => Map(x))
			.ToListAsync();
	}

	// =========================
	// GET BY ID
	// =========================
	public async Task<StudentResponseDto?> GetByIdAsync(string id)
	{
		var student = await _userManager.Users
			.FirstOrDefaultAsync(x =>
				x.Id == id &&
				!x.IsDeleted);

		if (student == null)
			return null;

		return Map(student);
	}

	// =========================
	// UPDATE
	// =========================
	public async Task<bool> UpdateAsync(
		string id,
		UpdateStudentDto dto)
	{
		var student = await _userManager.FindByIdAsync(id);

		if (student == null || student.IsDeleted)
			return false;

		student.FullName = dto.FullName;

		var result = await _userManager.UpdateAsync(student);

		return result.Succeeded;
	}

	// =========================
	// DELETE (Soft)
	// =========================
	public async Task<bool> DeleteAsync(string id)
	{
		var student = await _userManager.FindByIdAsync(id);

		if (student == null)
			return false;

		student.IsDeleted = true;
		student.IsActive = false;

		var result = await _userManager.UpdateAsync(student);

		return result.Succeeded;
	}

	// =========================
	// GET ME
	// =========================
	public async Task<StudentResponseDto?> GetMeAsync(ClaimsPrincipal user)
	{
		var userId =
			user.FindFirstValue(ClaimTypes.NameIdentifier);

		if (userId == null)
			return null;

		var student =
			await _userManager.FindByIdAsync(userId);

		if (student == null || student.IsDeleted)
			return null;

		return Map(student);
	}

	// =========================
	// Mapper
	// =========================
	private static StudentResponseDto Map(Student s)
	{
		return new StudentResponseDto
		{
			Id = s.Id,
			FullName = s.FullName,
			Email = s.Email!,
			IsActive = s.IsActive,
			CreatedAt = s.CreatedAt
		};
	}
}
