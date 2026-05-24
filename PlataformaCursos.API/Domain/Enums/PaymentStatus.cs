namespace PlataformaCursos.API.Domain.Enums;

/// <summary>
/// Representa os possíveis estados de um pagamento.
/// O fluxo segue: Pending → Paid ou Failed → Refunded (opcional).
/// </summary>
public enum PaymentStatus
{
	/// <summary>Pagamento criado, aguardando confirmação.</summary>
	Pending = 1,

	/// <summary>Pagamento confirmado com sucesso.</summary>
	Paid = 2,

	/// <summary>Pagamento falhou (ex: cartão recusado).</summary>
	Failed = 3,

	/// <summary>Pagamento estornado após confirmação.</summary>
	Refunded = 4
}