using Microsoft.AspNetCore.Identity;
using PlataformaCursos.API.Domain.Entities;

namespace PlataformaCursos.API.Application.Services;

public class StudentService
{
	private readonly UserManager<Student> _userManager;

	public StudentService(UserManager<Student> userManager)
	{
		_userManager = userManager;
	}

	public async Task<Student?> GetByIdAsync(string id)
	{
		return await _userManager.FindByIdAsync(id);
	}

	public async Task<bool> UpdateNameAsync(
		string id,
		string fullName)
	{
		var student =
			await _userManager.FindByIdAsync(id);

		if (student == null || student.IsDeleted)
			return false;

		student.FullName = fullName;

		var result =
			await _userManager.UpdateAsync(student);

		return result.Succeeded;
	}

	public async Task<bool> SoftDeleteAsync(string id)
	{
		var student =
			await _userManager.FindByIdAsync(id);

		if (student == null)
			return false;

		student.IsDeleted = true;
		student.IsActive = false;

		var result =
			await _userManager.UpdateAsync(student);

		return result.Succeeded;
	}
}
