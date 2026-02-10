using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlataformaCursos.API.Application.Services;
using PlataformaCursos.API.Domain.DTOs.Students;
using System.Security.Claims;

namespace PlataformaCursos.API.Controllers;

[ApiController]
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
	[Authorize(Roles = "Admin")]
	[HttpPost]
	public async Task<IActionResult> Create(
		CreateStudentDto dto)
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
	[Authorize(Roles = "Admin")]
	[HttpGet]
	public async Task<IActionResult> GetAll()
	{
		return Ok(await _service.GetAllAsync());
	}

	// =========================
	// GET BY ID (Admin ou Dono)
	// =========================
	[Authorize]
	[HttpGet("{id}")]
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
	[Authorize]
	[HttpPut("{id}")]
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
	[Authorize(Roles = "Admin")]
	[HttpDelete("{id}")]
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
	[Authorize]
	[HttpGet("me")]
	public async Task<IActionResult> Me()
	{
		var result = await _service.GetMeAsync(User);

		if (result == null)
			return NotFound();

		return Ok(result);
	}
}
