using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlataformaCursos.API.Application.Services;
using PlataformaCursos.API.Domain.DTOs.Enrollments;
using System.Security.Claims;

namespace PlataformaCursos.API.Controllers;

/// <summary>
/// Gerencia matrículas de estudantes em cursos
/// </summary>
[ApiController]
[Tags("Enrollments")]
[Route("api/[controller]")]
[Authorize]
public class EnrollmentsController : ControllerBase
{
	private readonly EnrollmentService _service;

	public EnrollmentsController(EnrollmentService service)
	{
		_service = service;
	}

	// ==================================================
	// POST /api/enrollments
	// ==================================================

	/// <summary>
	/// Realiza matrícula em um curso
	/// </summary>
	/// <remarks>
	/// Regras:
	/// - O aluno deve existir
	/// - O curso deve existir
	/// - Não pode haver matrícula duplicada
	/// - O aluno deve estar ativo
	///
	/// Retorna:
	/// - 409: matrícula duplicada
	/// - 422: regra de negócio violada
	/// </remarks>
	/// <response code="201">Matrícula criada</response>
	/// <response code="400">Entrada inválida</response>
	/// <response code="401">Não autenticado</response>
	/// <response code="409">Conflito</response>
	/// <response code="422">Regra de negócio</response>
	[HttpPost]
	[ProducesResponseType(typeof(EnrollmentResponseDto), StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status409Conflict)]
	[ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
	public async Task<IActionResult> Enroll(
		CreateEnrollmentDto dto)
	{
		try
		{
			var result =
				await _service.EnrollAsync(dto, User);

			return Created(string.Empty, result);
		}
		catch (InvalidOperationException ex)
		{
			return Conflict(new
			{
				message = ex.Message
			});
		}
		catch (ApplicationException ex)
		{
			return UnprocessableEntity(new
			{
				message = ex.Message
			});
		}
	}

	// ==================================================
	// GET /api/students/{id}/enrollments
	// ==================================================

	/// <summary>
	/// Lista matrículas de um estudante
	/// </summary>
	/// <remarks>
	/// Acesso:
	/// - Admin: pode consultar qualquer aluno
	/// - Student: apenas seus próprios dados
	///
	/// Filtros:
	/// - status (Active, Cancelled, Completed)
	/// - paginação
	/// </remarks>
	/// <param name="id">ID do estudante</param>
	/// <param name="page">Página (default: 1)</param>
	/// <param name="pageSize">Itens por página</param>
	/// <param name="status">Status da matrícula</param>
	/// <response code="200">Lista retornada</response>
	/// <response code="400">Parâmetros inválidos</response>
	/// <response code="401">Não autenticado</response>
	/// <response code="403">Sem permissão</response>
	[HttpGet("/api/students/{id}/enrollments")]
	[Authorize(Roles = "Admin,Student")]
	[ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	public async Task<IActionResult> GetByStudent(
		string id,
		int page = 1,
		int pageSize = 10,
		string? status = null)
	{
		var userId =
			User.FindFirstValue(ClaimTypes.NameIdentifier);

		var isAdmin =
			User.IsInRole("Admin");

		if (!isAdmin && userId != id)
			return Forbid();

		if (page <= 0 || pageSize <= 0)
			return BadRequest(new
			{
				message = "Parâmetros de paginação inválidos."
			});

		var result =
			await _service.GetByStudentAsync(
				id,
				page,
				pageSize,
				status);

		return Ok(result);
	}

	// ==================================================
	// DELETE /api/enrollments/{id}
	// ==================================================

	/// <summary>
	/// Cancela uma matrícula
	/// </summary>
	/// <remarks>
	/// Apenas administradores podem cancelar.
	/// </remarks>
	/// <param name="id">ID da matrícula</param>
	/// <response code="204">Cancelada</response>
	/// <response code="401">Não autenticado</response>
	/// <response code="403">Sem permissão</response>
	/// <response code="404">Não encontrada</response>
	[HttpDelete("{id}")]
	[Authorize(Roles = "Admin")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> Cancel(Guid id)
	{
		var success =
			await _service.CancelAsync(id);

		if (!success)
			return NotFound(new
			{
				message = "Matrícula não encontrada."
			});

		return NoContent();
	}
}
