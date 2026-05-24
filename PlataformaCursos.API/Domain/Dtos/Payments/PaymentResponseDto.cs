using PlataformaCursos.API.Domain.Enums;

namespace PlataformaCursos.API.Domain.DTOs.Payments;

/// <summary>
/// Dados retornados ao cliente sobre um pagamento.
/// Não expõe campos internos sensíveis.
/// </summary>
public class PaymentResponseDto
{
	/// <summary>Id único do pagamento.</summary>
	public Guid Id { get; set; }

	/// <summary>Id da matrícula relacionada.</summary>
	public Guid EnrollmentId { get; set; }

	/// <summary>Id do estudante dono do pagamento.</summary>
	public string StudentId { get; set; } = string.Empty;

	/// <summary>Valor do pagamento.</summary>
	public decimal Amount { get; set; }

	/// <summary>Moeda do pagamento (ex: BRL).</summary>
	public string Currency { get; set; } = string.Empty;

	/// <summary>Estado atual do pagamento.</summary>
	public string Status { get; set; } = string.Empty;

	/// <summary>Método de pagamento utilizado.</summary>
	public string Method { get; set; } = string.Empty;

	/// <summary>Id da transação no gateway (após confirmação).</summary>
	public string? TransactionId { get; set; }

	/// <summary>Motivo da falha (quando Status = Failed).</summary>
	public string? FailureReason { get; set; }

	/// <summary>Data de criação do pagamento.</summary>
	public DateTime CreatedAt { get; set; }

	/// <summary>Data de confirmação (quando Status = Paid).</summary>
	public DateTime? PaidAt { get; set; }
}