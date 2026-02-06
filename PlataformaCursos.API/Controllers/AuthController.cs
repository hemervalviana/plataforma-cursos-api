using Microsoft.AspNetCore.Mvc;
using PlataformaCursos.API.Application.Services;
using PlataformaCursos.API.Domain.DTOs.Auth;

namespace PlataformaCursos.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
	private readonly AuthService _authService;

	public AuthController(AuthService authService)
	{
		_authService = authService;
	}


	// ================================
	// POST: api/auth/register
	// ================================

	[HttpPost("register")]
	public async Task<IActionResult> Register(RegisterDto dto)
	{
		var result = await _authService.RegisterAsync(dto);

		if (!result.Succeeded)
			return BadRequest(result.Errors);

		return Ok("Usuário registrado com sucesso.");
	}


	// ================================
	// POST: api/auth/login
	// ================================

	[HttpPost("login")]
	public async Task<IActionResult> Login(LoginDto dto)
	{
		var token = await _authService.LoginAsync(dto);

		if (token == null)
			return Unauthorized("Usuário ou senha inválidos.");

		return Ok(new { token });
	}
}
