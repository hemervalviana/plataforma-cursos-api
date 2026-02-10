using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlataformaCursos.API.Application.Common;
using PlataformaCursos.API.Application.Services;
using PlataformaCursos.API.Domain.DTOs.Courses;

namespace PlataformaCursos.API.Controllers;

/// <summary>
/// Gerencia os cursos da plataforma
/// </summary>
[ApiController]
[Tags("Courses")]
[Route("api/[controller]")]
public class CoursesController : ControllerBase
{
	private readonly CourseService _service;

	public CoursesController(CourseService service)
	{
		_service = service;
	}

	// =============================
	// DEBUG
	// =============================

	/// <summary>
	/// Exibe informações do usuário autenticado (debug)
	/// </summary>
	/// <remarks>
	/// Usado para validar claims e roles no JWT.
	/// </remarks>
	[Authorize(Roles = "Admin,Instructor")]
	[HttpGet("debug-user")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
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

	// =============================
	// HEALTH CHECK
	// =============================	

	// =============================
	// POST - Create
	// =============================

	/// <summary>
	/// Cria um novo curso
	/// </summary>
	/// <remarks>
	/// Apenas Admin ou Instructor.
	/// O título deve ter no mínimo 3 caracteres.
	/// </remarks>
	/// <response code="201">Curso criado</response>
	/// <response code="400">Entrada inválida</response>
	/// <response code="401">Não autenticado</response>
	/// <response code="403">Sem permissão</response>
	[Authorize(Roles = "Admin,Instructor")]
	[HttpPost]
	[ProducesResponseType(typeof(CourseResponseDto), StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
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

	/// <summary>
	/// Lista cursos com paginação e filtros
	/// </summary>
	/// <remarks>
	/// Filtros disponíveis:
	/// - category
	/// - search (título)
	/// - orderBy (title, createdAt)
	///
	/// Cache: 60s com suporte a ETag.
	/// </remarks>
	/// <param name="category">Categoria do curso</param>
	/// <param name="search">Texto para busca</param>
	/// <param name="orderBy">Campo de ordenação</param>
	/// <param name="page">Página (default: 1)</param>
	/// <param name="pageSize">Tamanho da página (default: 10)</param>
	/// <response code="200">Lista retornada</response>
	/// <response code="304">Não modificado (cache)</response>
	[AllowAnonymous]
	[HttpGet]
	[ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
	[ProducesResponseType(typeof(PagedResult<CourseResponseDto>), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status304NotModified)]
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

	/// <summary>
	/// Busca um curso por ID
	/// </summary>
	/// <remarks>
	/// Suporta cache com ETag.
	/// </remarks>
	/// <param name="id">ID do curso</param>
	/// <response code="200">Curso encontrado</response>
	/// <response code="304">Não modificado</response>
	/// <response code="404">Não encontrado</response>
	[AllowAnonymous]
	[HttpGet("{id}")]
	[ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
	[ProducesResponseType(typeof(CourseResponseDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status304NotModified)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
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
	// PUT - Update
	// =============================

	/// <summary>
	/// Atualiza um curso
	/// </summary>
	/// <remarks>
	/// Apenas Admin ou Instructor.
	/// </remarks>
	/// <param name="id">ID do curso</param>
	/// <response code="204">Atualizado</response>
	/// <response code="401">Não autenticado</response>
	/// <response code="403">Sem permissão</response>
	/// <response code="404">Não encontrado</response>
	[Authorize(Roles = "Admin,Instructor")]
	[HttpPut("{id}")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
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

	/// <summary>
	/// Remove um curso
	/// </summary>
	/// <remarks>
	/// Apenas administradores.
	/// </remarks>
	/// <param name="id">ID do curso</param>
	/// <response code="204">Removido</response>
	/// <response code="401">Não autenticado</response>
	/// <response code="403">Sem permissão</response>
	/// <response code="404">Não encontrado</response>
	[Authorize(Roles = "Admin")]
	[HttpDelete("{id}")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> Delete(Guid id)
	{
		var deleted = await _service.DeleteAsync(id);

		if (!deleted)
			return NotFound();

		return NoContent();
	}
}
