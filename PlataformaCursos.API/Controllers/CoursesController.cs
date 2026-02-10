using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlataformaCursos.API.Application.Common;
using PlataformaCursos.API.Application.Services;
using PlataformaCursos.API.Domain.DTOs.Courses;

namespace PlataformaCursos.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoursesController : ControllerBase
{
	private readonly CourseService _service;

	public CoursesController(CourseService service)
	{
		_service = service;
	}

	[Authorize(Roles = "Admin,Instructor")]
	[HttpGet("debug-user")]
	public IActionResult DebugUser()
	{
		return Ok(new
		{
			User = User.Identity!.Name,
			Claims = User.Claims.Select(c => new
			{
				c.Type,
				c.Value
			})
		});
	}


	[HttpGet("teste")]
	public IActionResult GetTest()
	{
		return Ok("API is working!");
	}

	// =============================
	// POST - Create
	// =============================
	[Authorize(Roles = "Admin,Instructor")]	
	[HttpPost]
	public async Task<IActionResult> Create(CreateCourseDto dto)
	{
		var result = await _service.CreateAsync(dto);		

		return CreatedAtAction(
			nameof(GetById),
			new { id = result.Id },
			result);
	}

	// =============================
	// GET - List
	// =============================
	[AllowAnonymous]
	[HttpGet]
	[ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
	public async Task<IActionResult> Get(
	string? category,
	string? search,
	string? orderBy = "title",
	int page = 1,
	int pageSize = 10)
	{
		var result = await _service.GetAsync(
			category,
			search,
			orderBy,
			page,
			pageSize);

		var etag = ETagHelper.Generate(result);

		if (Request.Headers.IfNoneMatch == etag)
			return StatusCode(StatusCodes.Status304NotModified);

		Response.Headers.ETag = etag;

		return Ok(result);
	}


	// =============================
	// GET - By Id
	// =============================
	[AllowAnonymous]
	[HttpGet("{id}")]
	[ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
	public async Task<IActionResult> GetById(Guid id)
	{
		var result = await _service.GetByIdAsync(id);

		if (result == null)
			return NotFound();

		var etag = ETagHelper.Generate(result);

		if (Request.Headers.IfNoneMatch == etag)
			return StatusCode(StatusCodes.Status304NotModified);

		Response.Headers.ETag = etag;

		return Ok(result);
	}


	// =============================
	// PUT
	// =============================
	[Authorize(Roles = "Admin,Instructor")]
	[HttpPut("{id}")]
	public async Task<IActionResult> Update(
		Guid id,
		UpdateCourseDto dto)
	{
		var updated = await _service.UpdateAsync(id, dto);

		if (!updated)
			return NotFound();

		return NoContent();
	}

	// =============================
	// DELETE
	// =============================
	[Authorize(Roles = "Admin")]
	[HttpDelete("{id}")]
	public async Task<IActionResult> Delete(Guid id)
	{
		var deleted = await _service.DeleteAsync(id);

		if (!deleted)
			return NotFound();

		return NoContent();
	}
}
