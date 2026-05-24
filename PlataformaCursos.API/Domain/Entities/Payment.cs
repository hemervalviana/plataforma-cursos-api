using PlataformaCursos.API.Domain.Enums;
using PlataformaCursos.API.Domain.ValueObjects;

namespace PlataformaCursos.API.Domain.Entities;

/// <summary>
/// Entidade de domínio que representa um pagamento de matrícula.
/// 
/// Tratada como agregado separado — referencia Enrollment pelo Id,
/// sem depender diretamente da lógica de matrícula.
/// 
/// Todas as propriedades têm setter privado para garantir
/// que o estado só mude através dos métodos do domínio
/// (Confirm, Fail, Refund), nunca de fora da classe.
/// </summary>
public class Payment
{
	/// <summary>Identificador único do pagamento (GUID).</summary>
	public Guid Id { get; private set; }

	/// <summary>
	/// Referência à matrícula que originou este pagamento.
	/// Um pagamento sempre pertence a uma matrícula.
	/// </summary>
	public Guid EnrollmentId { get; private set; }

	/// <summary>
	/// Id do estudante dono do pagamento.
	/// Usado para autorização — Student só acessa os próprios pagamentos.
	/// </summary>
	public string StudentId { get; private set; } = string.Empty;

	/// <summary>
	/// Value Object que encapsula valor e moeda.
	/// Garante que nunca teremos um pagamento com valor inválido.
	/// </summary>
	public Money Amount { get; private set; } = null!;

	/// <summary>
	/// Estado atual do pagamento.
	/// Fluxo válido: Pending → Paid ou Failed → Refunded.
	/// </summary>
	public PaymentStatus Status { get; private set; }

	/// <summary>Método de pagamento escolhido pelo estudante.</summary>
	public PaymentMethod Method { get; private set; }

	/// <summary>
	/// Chave de idempotência enviada pelo cliente no header.
	/// Garante que a mesma requisição não crie dois pagamentos.
	/// Se já existir um pagamento com essa chave, retorna o existente.
	/// </summary>
	public string IdempotencyKey { get; private set; } = string.Empty;

	/// <summary>
	/// Id da transação retornado pelo gateway de pagamento.
	/// Preenchido apenas após confirmação (Status = Paid).
	/// </summary>
	public string? TransactionId { get; private set; }

	/// <summary>
	/// Motivo da falha retornado pelo gateway.
	/// Preenchido apenas quando Status = Failed.
	/// </summary>
	public string? FailureReason { get; private set; }

	/// <summary>Data/hora de criação do pagamento (UTC).</summary>
	public DateTime CreatedAt { get; private set; }

	/// <summary>
	/// Data/hora em que o pagamento foi confirmado (UTC).
	/// Null enquanto o pagamento não estiver Paid.
	/// </summary>
	public DateTime? PaidAt { get; private set; }

	/// <summary>Data/hora da última atualização de estado (UTC).</summary>
	public DateTime? UpdatedAt { get; private set; }

	/// <summary>
	/// Soft delete — o registro não é removido fisicamente do banco.
	/// Mantém histórico de pagamentos para auditoria.
	/// </summary>
	public bool IsDeleted { get; private set; }

	// ======================================================
	// Propriedades de navegação (EF Core)
	// ======================================================

	/// <summary>Matrícula relacionada (navigation property do EF Core).</summary>
	public Enrollment Enrollment { get; private set; } = null!;

	/// <summary>Estudante relacionado (navigation property do EF Core).</summary>
	public Student Student { get; private set; } = null!;

	// ======================================================
	// Construtor protegido para o EF Core
	// O EF Core precisa de um construtor sem parâmetros para
	// materializar objetos vindos do banco. Protegido para
	// não permitir instâncias diretas fora do domínio.
	// ======================================================
	protected Payment() { }

	// ======================================================
	// Factory Method — único ponto de criação válido
	// ======================================================

	/// <summary>
	/// Cria uma nova intenção de pagamento no estado Pending.
	/// 
	/// É um Factory Method estático — garante que toda instância
	/// nasce válida, com todas as regras de negócio aplicadas.
	/// Nunca use o construtor diretamente fora do domínio.
	/// </summary>
	/// <param name="enrollmentId">Id da matrícula a ser paga.</param>
	/// <param name="studentId">Id do estudante dono do pagamento.</param>
	/// <param name="amount">Valor monetário (deve ser maior que zero).</param>
	/// <param name="method">Método de pagamento escolhido.</param>
	/// <param name="idempotencyKey">Chave única enviada pelo cliente.</param>
	public static Payment Create(
		Guid enrollmentId,
		string studentId,
		Money amount,
		PaymentMethod method,
		string idempotencyKey)
	{
		// Validações de entrada — falha rápido antes de criar o objeto
		if (enrollmentId == Guid.Empty)
			throw new ArgumentException(
				"EnrollmentId inválido.", nameof(enrollmentId));

		if (string.IsNullOrWhiteSpace(studentId))
			throw new ArgumentException(
				"StudentId inválido.", nameof(studentId));

		if (string.IsNullOrWhiteSpace(idempotencyKey))
			throw new ArgumentException(
				"IdempotencyKey é obrigatória.", nameof(idempotencyKey));

		// Cria o pagamento já com estado inicial Pending
		return new Payment
		{
			Id = Guid.NewGuid(),
			EnrollmentId = enrollmentId,
			StudentId = studentId,
			Amount = amount,
			Method = method,
			IdempotencyKey = idempotencyKey,
			Status = PaymentStatus.Pending,   // sempre começa Pending
			CreatedAt = DateTime.UtcNow,
			IsDeleted = false
		};
	}

	// ======================================================
	// Métodos de transição de estado
	// Encapsulam as regras de negócio de cada transição.
	// Lançam exceção se a transição for inválida.
	// ======================================================

	/// <summary>
	/// Confirma o pagamento após aprovação do gateway.
	/// Transição válida: Pending → Paid.
	/// </summary>
	/// <param name="transactionId">Id da transação retornado pelo gateway.</param>
	public void Confirm(string transactionId)
	{
		// Só pode confirmar se estiver Pending
		if (Status != PaymentStatus.Pending)
			throw new InvalidOperationException(
				"Apenas pagamentos pendentes podem ser confirmados.");

		Status = PaymentStatus.Paid;
		TransactionId = transactionId;
		PaidAt = DateTime.UtcNow;
		UpdatedAt = DateTime.UtcNow;
	}

	/// <summary>
	/// Marca o pagamento como falho após rejeição do gateway.
	/// Transição válida: Pending → Failed.
	/// </summary>
	/// <param name="reason">Motivo da falha retornado pelo gateway.</param>
	public void Fail(string reason)
	{
		// Só pode falhar se estiver Pending
		if (Status != PaymentStatus.Pending)
			throw new InvalidOperationException(
				"Apenas pagamentos pendentes podem ser marcados como falhos.");

		Status = PaymentStatus.Failed;
		FailureReason = reason;
		UpdatedAt = DateTime.UtcNow;
	}

	/// <summary>
	/// Estorna o pagamento após solicitação do estudante ou Admin.
	/// Transição válida: Paid → Refunded.
	/// </summary>
	public void Refund()
	{
		// Só pode estornar se já estiver Paid
		if (Status != PaymentStatus.Paid)
			throw new InvalidOperationException(
				"Apenas pagamentos confirmados podem ser estornados.");

		Status = PaymentStatus.Refunded;
		UpdatedAt = DateTime.UtcNow;
	}
}