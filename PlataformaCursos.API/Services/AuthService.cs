using Microsoft.AspNetCore.Identity;
using PlataformaCursos.API.Domain.DTOs.Auth;
using PlataformaCursos.API.Domain.Entities;

namespace PlataformaCursos.API.Application.Services;

public class AuthService
{
	private readonly UserManager<Student> _userManager;
	private readonly SignInManager<Student> _signInManager;
	private readonly TokenService _tokenService;

	public AuthService(
		UserManager<Student> userManager,
		SignInManager<Student> signInManager,
		TokenService tokenService)
	{
		_userManager = userManager;
		_signInManager = signInManager;
		_tokenService = tokenService;
	}


	// ==========================================
	// Registro
	// ==========================================

	public async Task<IdentityResult> RegisterAsync(RegisterDto dto)
	{
		var student = new Student
		{
			UserName = dto.UserName,
			Email = dto.Email,
			FullName = dto.FullName
		};

		return await _userManager.CreateAsync(
			student,
			dto.Password);
	}


	// ==========================================
	// Login
	// ==========================================

	public async Task<string?> LoginAsync(LoginDto dto)
	{
		var user = await _userManager
			.FindByNameAsync(dto.UserName);

		if (user == null)
			return null;

		if (user.IsDeleted || !user.IsActive)
			return null;

		var result =
			await _signInManager
				.CheckPasswordSignInAsync(
					user,
					dto.Password,
					false);

		if (!result.Succeeded)
			return null;

		var roles =
			await _userManager.GetRolesAsync(user);

		return _tokenService.GenerateToken(
			user,
			roles);
	}
}
