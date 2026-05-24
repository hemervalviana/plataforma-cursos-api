using Microsoft.EntityFrameworkCore;
using PlataformaCursos.API.Application.Common;
using PlataformaCursos.API.Application.Interfaces;
using PlataformaCursos.API.Domain.Entities;
using PlataformaCursos.API.Domain.Enums;

namespace PlataformaCursos.API.Infrastructure.Data;

/// <summary>
/// Implementação EF Core do repositório de pagamentos.
/// 
/// Fica na Infrastructure — a Application nunca referencia esta classe diretamente,
/// apenas a interface IPaymentRepository.
/// 
/// Toda a complexidade de consulta EF Core fica aqui,
/// mantendo o PaymentService limpo e focado nas regras de negócio.
/// </summary>
public class PaymentRepository : IPaymentRepository
{
	private readonly ApplicationDbContext _context;

	public PaymentRepository(ApplicationDbContext context)
	{
		_context = context;
	}

	/// <summary>
	/// Busca por idempotency key ignorando soft delete.
	/// Necessário para retornar pagamentos já processados mesmo que deletados.
	/// </summary>
	public async Task<Payment?> GetByIdempotencyKeyAsync(
		string idempotencyKey,
		CancellationToken cancellationToken = default)
	{
		return await _context.Payments
			.IgnoreQueryFilters() // ignora o global filter de IsDeleted
			.FirstOrDefaultAsync(
				p => p.IdempotencyKey == idempotencyKey,
				cancellationToken);
	}

	/// <summary>
	/// Busca por Id respeitando soft delete.
	/// </summary>
	public async Task<Payment?> GetByIdAsync(
		Guid id,
		CancellationToken cancellationToken = default)
	{
		return await _context.Payments
			.FirstOrDefaultAsync(
				p => p.Id == id && !p.IsDeleted,
				cancellationToken);
	}

	/// <summary>
	/// Verifica se existe pagamento ativo (Pending ou Paid) para a matrícula.
	/// </summary>
	public async Task<bool> HasActivePaymentAsync(
		Guid enrollmentId,
		CancellationToken cancellationToken = default)
	{
		return await _context.Payments
			.AnyAsync(p =>
				p.EnrollmentId == enrollmentId &&
				(p.Status == PaymentStatus.Pending ||
				 p.Status == PaymentStatus.Paid) &&
				!p.IsDeleted,
				cancellationToken);
	}

	/// <summary>
	/// Lista pagamentos de um estudante com filtro opcional por status e paginação.
	/// Retorna os dados e o total para paginação.
	/// </summary>
	public async Task<(List<Payment> Data, int Total)> GetByStudentAsync(
		string studentId,
		PaymentStatus? status,
		int page,
		int pageSize,
		CancellationToken cancellationToken = default)
	{
		var query = _context.Payments
			.Where(p => p.StudentId == studentId && !p.IsDeleted)
			.AsQueryable();

		if (status.HasValue)
			query = query.Where(p => p.Status == status.Value);

		var total = await query.CountAsync(cancellationToken);

		var data = await query
			.OrderByDescending(p => p.CreatedAt)
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.ToListAsync(cancellationToken);

		return (data, total);
	}

	/// <summary>
	/// Lista todos os pagamentos com filtros opcionais (Admin).
	/// </summary>
	public async Task<(List<Payment> Data, int Total)> GetAllAsync(
		PaymentStatus? status,
		Guid? enrollmentId,
		int page,
		int pageSize,
		CancellationToken cancellationToken = default)
	{
		var query = _context.Payments
			.Where(p => !p.IsDeleted)
			.AsQueryable();

		if (status.HasValue)
			query = query.Where(p => p.Status == status.Value);

		if (enrollmentId.HasValue)
			query = query.Where(p => p.EnrollmentId == enrollmentId.Value);

		var total = await query.CountAsync(cancellationToken);

		var data = await query
			.OrderByDescending(p => p.CreatedAt)
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.ToListAsync(cancellationToken);

		return (data, total);
	}

	/// <summary>
	/// Adiciona pagamento ao contexto — não salva ainda.
	/// Chame SaveChangesAsync após.
	/// </summary>
	public async Task AddAsync(
		Payment payment,
		CancellationToken cancellationToken = default)
	{
		await _context.Payments.AddAsync(payment, cancellationToken);
	}

	/// <summary>
	/// Persiste todas as alterações pendentes no banco.
	/// </summary>
	public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
	{
		await _context.SaveChangesAsync(cancellationToken);
	}
}