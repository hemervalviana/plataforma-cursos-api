using PlataformaCursos.API.Application.Common;
using PlataformaCursos.API.Domain.Entities;
using PlataformaCursos.API.Domain.Enums;

namespace PlataformaCursos.API.Application.Interfaces;

/// <summary>
/// Contrato do repositório de pagamentos.
/// 
/// A Application depende desta interface — nunca da implementação EF Core.
/// Isso garante que o domínio e a aplicação ficam isolados do banco de dados.
/// 
/// Padrão: Repository + DIP (Dependency Inversion Principle)
/// </summary>
public interface IPaymentRepository
{
	/// <summary>
	/// Busca pagamento por chave de idempotência.
	/// Ignora soft delete — necessário para a lógica de idempotência.
	/// </summary>
	Task<Payment?> GetByIdempotencyKeyAsync(
		string idempotencyKey,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Busca pagamento pelo Id.
	/// Respeita soft delete — não retorna pagamentos deletados.
	/// </summary>
	Task<Payment?> GetByIdAsync(
		Guid id,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Verifica se já existe pagamento ativo (Pending ou Paid) para a matrícula.
	/// Usado para garantir unicidade de pagamento ativo por matrícula.
	/// </summary>
	Task<bool> HasActivePaymentAsync(
		Guid enrollmentId,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Lista pagamentos de um estudante com filtro e paginação.
	/// </summary>
	Task<(List<Payment> Data, int Total)> GetByStudentAsync(
		string studentId,
		PaymentStatus? status,
		int page,
		int pageSize,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Lista todos os pagamentos com filtros e paginação (Admin).
	/// </summary>
	Task<(List<Payment> Data, int Total)> GetAllAsync(
		PaymentStatus? status,
		Guid? enrollmentId,
		int page,
		int pageSize,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Adiciona um novo pagamento no banco.
	/// </summary>
	Task AddAsync(
		Payment payment,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Persiste alterações no banco.
	/// </summary>
	Task SaveChangesAsync(CancellationToken cancellationToken = default);
}