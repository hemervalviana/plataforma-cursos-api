using Microsoft.AspNetCore.Mvc;
using PlataformaCursos.API.Application.Services;
using PlataformaCursos.API.Domain.DTOs.Auth;

namespace PlataformaCursos.API.Controllers;

/// <summary>
/// Gerencia autenticação e registro de usuários
/// </summary>
[ApiController]
[Tags("Auth")]
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

	/// <summary>
	/// Registra um novo usuário
	/// </summary>
	/// <remarks>
	/// Regras:
	/// - Email deve ser único
	/// - Senha deve atender requisitos do Identity
	/// - Role padrão: Student
	///
	/// Retorna:
	/// - 400: dados inválidos
	/// - 409: email duplicado
	/// </remarks>
	/// <response code="200">Usuário registrado</response>
	/// <response code="400">Dados inválidos</response>
	/// <response code="409">E-mail já cadastrado</response>
	[HttpPost("register")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status409Conflict)]
	public async Task<IActionResult> Register(RegisterDto dto)
	{
		var result = await _authService.RegisterAsync(dto);

		if (!result.Succeeded)
		{
			var errors = result.Errors
				.Select(e => e.Description)
				.ToList();

			// Detecta duplicidade de email
			if (errors.Any(e => e.Contains("email", StringComparison.OrdinalIgnoreCase)))
			{
				return Conflict(new
				{
					message = "E-mail já cadastrado."
				});
			}

			return BadRequest(new
			{
				errors
			});
		}

		return Ok(new
		{
			message = "Usuário registrado com sucesso."
		});
	}

	// ================================
	// POST: api/auth/login
	// ================================

	/// <summary>
	/// Autentica o usuário e gera JWT
	/// </summary>
	/// <remarks>
	/// Retorna token JWT para autenticação
	/// </remarks>
	/// <response code="200">Login realizado</response>
	/// <response code="401">Credenciais inválidas</response>
	[HttpPost("login")]
	[ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> Login(LoginDto dto)
	{
		var token = await _authService.LoginAsync(dto);

		if (token == null)
			return Unauthorized(new
			{
				message = "Usuário ou senha inválidos."
			});

		return Ok(new
		{
			token
		});
	}
}
