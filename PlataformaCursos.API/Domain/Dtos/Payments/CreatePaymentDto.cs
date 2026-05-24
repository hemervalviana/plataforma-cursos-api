using PlataformaCursos.API.Domain.Enums;

namespace PlataformaCursos.API.Domain.DTOs.Payments;

/// <summary>
/// Dados necessários para criar uma intenção de pagamento.
/// </summary>
public class CreatePaymentDto
{
	/// <summary>
	/// Id da matrícula que será paga.
	/// Deve ser uma matrícula ativa do estudante.
	/// </summary>
	public Guid EnrollmentId { get; set; }

	/// <summary>
	/// Valor do pagamento. Deve ser maior que zero.
	/// </summary>
	public decimal Amount { get; set; }

	/// <summary>
	/// Moeda do pagamento. Padrão: BRL.
	/// </summary>
	public string Currency { get; set; } = "BRL";

	/// <summary>
	/// Método de pagamento escolhido pelo estudante.
	/// 1=CreditCard, 2=DebitCard, 3=Pix, 4=BankSlip
	/// </summary>
	public PaymentMethod Method { get; set; }
}