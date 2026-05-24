namespace PlataformaCursos.API.Domain.DTOs.Payments;

/// <summary>
/// Dados para confirmar um pagamento pendente.
/// Usado pelo Admin após aprovação do gateway.
/// </summary>
public class ConfirmPaymentDto
{
	/// <summary>
	/// Id da transação retornado pelo gateway de pagamento.
	/// Obrigatório para rastreabilidade.
	/// </summary>
	public string TransactionId { get; set; } = string.Empty;
}