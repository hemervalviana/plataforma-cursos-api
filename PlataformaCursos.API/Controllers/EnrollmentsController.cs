using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlataformaCursos.API.Application.Services;
using PlataformaCursos.API.Domain.DTOs.Enrollments;
using System.Security.Claims;

namespace PlataformaCursos.API.Controllers;

[ApiController]
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
	[HttpPost]
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
	[HttpGet("/api/students/{id}/enrollments")]
	[Authorize(Roles = "Admin,Student")]
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
	[HttpDelete("{id}")]
	[Authorize(Roles = "Admin")]
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
