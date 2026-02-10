using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlataformaCursos.API.Application.Services;
using PlataformaCursos.API.Domain.DTOs.Students;
using System.Security.Claims;

namespace PlataformaCursos.API.Controllers;

/// <summary>
/// Gerencia estudantes da plataforma
/// </summary>
[ApiController]
[Tags("Students")]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
	private readonly StudentService _service;

	public StudentsController(StudentService service)
	{
		_service = service;
	}

	// =========================
	// POST (Admin)
	// =========================

	/// <summary>
	/// Cria um novo estudante
	/// </summary>
	/// <remarks>
	/// Apenas administradores podem criar estudantes.
	/// O e-mail deve ser único.
	/// </remarks>
	/// <response code="201">Estudante criado com sucesso</response>
	/// <response code="401">Não autenticado</response>
	/// <response code="403">Sem permissão</response>
	/// <response code="409">E-mail já existente</response>
	[Authorize(Roles = "Admin")]
	[HttpPost]
	[ProducesResponseType(typeof(StudentResponseDto), StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	[ProducesResponseType(StatusCodes.Status409Conflict)]
	public async Task<IActionResult> Create(CreateStudentDto dto)
	{
		var result = await _service.CreateAsync(dto);

		if (result == null)
			return Conflict("E-mail já existe.");

		return CreatedAtAction(
			nameof(GetById),
			new { id = result.Id },
			result);
	}

	// =========================
	// GET ALL (Admin)
	// =========================

	/// <summary>
	/// Lista todos os estudantes
	/// </summary>
	/// <remarks>
	/// Endpoint restrito para administradores.
	/// Retorna dados sensíveis.
	/// </remarks>
	/// <response code="200">Lista retornada</response>
	/// <response code="401">Não autenticado</response>
	/// <response code="403">Sem permissão</response>
	[Authorize(Roles = "Admin")]
	[HttpGet]
	[ProducesResponseType(typeof(IEnumerable<StudentResponseDto>), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	public async Task<IActionResult> GetAll()
	{
		return Ok(await _service.GetAllAsync());
	}

	// =========================
	// GET BY ID (Admin ou Dono)
	// =========================

	/// <summary>
	/// Busca um estudante por ID
	/// </summary>
	/// <remarks>
	/// Somente o próprio estudante ou um Admin pode acessar.
	/// </remarks>
	/// <param name="id">ID do estudante</param>
	/// <response code="200">Estudante encontrado</response>
	/// <response code="401">Não autenticado</response>
	/// <response code="403">Sem permissão</response>
	/// <response code="404">Não encontrado</response>
	[Authorize]
	[HttpGet("{id}")]
	[ProducesResponseType(typeof(StudentResponseDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> GetById(string id)
	{
		var userId =
			User.FindFirstValue(ClaimTypes.NameIdentifier);

		var isAdmin = User.IsInRole("Admin");

		if (!isAdmin && userId != id)
			return Forbid();

		var result = await _service.GetByIdAsync(id);

		if (result == null)
			return NotFound();

		return Ok(result);
	}

	// =========================
	// PUT (Admin ou Dono)
	// =========================

	/// <summary>
	/// Atualiza os dados do estudante
	/// </summary>
	/// <remarks>
	/// Apenas o próprio estudante ou Admin.
	/// </remarks>
	/// <param name="id">ID do estudante</param>
	/// <response code="204">Atualizado com sucesso</response>
	/// <response code="401">Não autenticado</response>
	/// <response code="403">Sem permissão</response>
	/// <response code="404">Não encontrado</response>
	[Authorize]
	[HttpPut("{id}")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> Update(
		string id,
		UpdateStudentDto dto)
	{
		var userId =
			User.FindFirstValue(ClaimTypes.NameIdentifier);

		var isAdmin = User.IsInRole("Admin");

		if (!isAdmin && userId != id)
			return Forbid();

		var updated =
			await _service.UpdateAsync(id, dto);

		if (!updated)
			return NotFound();

		return NoContent();
	}

	// =========================
	// DELETE (Admin)
	// =========================

	/// <summary>
	/// Remove (desativa) um estudante
	/// </summary>
	/// <remarks>
	/// Apenas administradores.
	/// Realiza soft delete.
	/// </remarks>
	/// <param name="id">ID do estudante</param>
	/// <response code="204">Removido com sucesso</response>
	/// <response code="401">Não autenticado</response>
	/// <response code="403">Sem permissão</response>
	/// <response code="404">Não encontrado</response>
	[Authorize(Roles = "Admin")]
	[HttpDelete("{id}")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> Delete(string id)
	{
		var deleted = await _service.DeleteAsync(id);

		if (!deleted)
			return NotFound();

		return NoContent();
	}

	// =========================
	// GET ME
	// =========================

	/// <summary>
	/// Retorna o perfil do usuário autenticado
	/// </summary>
	/// <remarks>
	/// Baseado no token JWT.
	/// </remarks>
	/// <response code="200">Perfil retornado</response>
	/// <response code="401">Não autenticado</response>
	/// <response code="404">Perfil não encontrado</response>
	[Authorize]
	[HttpGet("me")]
	[ProducesResponseType(typeof(StudentResponseDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> Me()
	{
		var result = await _service.GetMeAsync(User);

		if (result == null)
			return NotFound();

		return Ok(result);
	}
}
