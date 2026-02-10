using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PlataformaCursos.API.Application.Common;
using PlataformaCursos.API.Domain.DTOs.Courses;
using PlataformaCursos.API.Domain.Entities;
using PlataformaCursos.API.Infrastructure.Data;

namespace PlataformaCursos.API.Application.Services;

public class CourseService
{
	private readonly ApplicationDbContext _context;
	private readonly IMapper _mapper;

	public CourseService(
		ApplicationDbContext context,
		IMapper mapper)
	{
		_context = context;
		_mapper = mapper;
	}

	// =============================
	// CREATE
	// =============================
	public async Task<CourseResponseDto> CreateAsync(CreateCourseDto dto)
	{
		var exists = await _context.Courses
			.AnyAsync(c => c.Title == dto.Title);

		if (exists)
			throw new InvalidOperationException("Curso já existe.");

		var course = _mapper.Map<Course>(dto);

		course.Id = Guid.NewGuid();
		course.CreatedAt = DateTime.UtcNow;

		_context.Courses.Add(course);
		await _context.SaveChangesAsync();

		return _mapper.Map<CourseResponseDto>(course);
	}

	// =============================
	// GET LIST (Filtro + Paginação)
	// =============================
	public async Task<PagedResult<CourseResponseDto>> GetAsync(
		string? category,
		string? search,
		string? orderBy,
		int page,
		int pageSize)
	{
		var query = _context.Courses.AsQueryable();

		// Filtro
		if (!string.IsNullOrEmpty(category))
			query = query.Where(c => c.Category == category);

		// Busca
		if (!string.IsNullOrEmpty(search))
			query = query.Where(c =>
				c.Title.Contains(search) ||
				c.Category.Contains(search));

		// Ordenação
		query = orderBy switch
		{
			"title" => query.OrderBy(c => c.Title),
			"date" => query.OrderByDescending(c => c.CreatedAt),
			_ => query.OrderBy(c => c.Title)
		};

		var total = await query.CountAsync();

		var courses = await query
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.ToListAsync();

		return new PagedResult<CourseResponseDto>
		{
			Page = page,
			PageSize = pageSize,
			Total = total,
			Data = _mapper.Map<IEnumerable<CourseResponseDto>>(courses)
		};
	}

	// =============================
	// GET BY ID
	// =============================
	public async Task<CourseResponseDto?> GetByIdAsync(Guid id)
	{
		var course = await _context.Courses
			.FirstOrDefaultAsync(c => c.Id == id);

		if (course == null)
			return null;

		return _mapper.Map<CourseResponseDto>(course);
	}

	// =============================
	// UPDATE
	// =============================
	public async Task<bool> UpdateAsync(
		Guid id,
		UpdateCourseDto dto)
	{
		var course = await _context.Courses
			.FirstOrDefaultAsync(c => c.Id == id);

		if (course == null)
			return false;

		_mapper.Map(dto, course);

		await _context.SaveChangesAsync();

		return true;
	}

	// =============================
	// DELETE (Soft)
	// =============================
	public async Task<bool> DeleteAsync(Guid id)
	{
		var course = await _context.Courses
			.FirstOrDefaultAsync(c => c.Id == id);

		if (course == null)
			return false;

		course.IsDeleted = true;

		await _context.SaveChangesAsync();

		return true;
	}
}
