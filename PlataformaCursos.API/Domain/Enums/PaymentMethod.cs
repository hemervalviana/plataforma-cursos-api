namespace PlataformaCursos.API.Domain.Enums;

/// <summary>
/// Representa os métodos de pagamento aceitos pela plataforma.
/// Novos métodos podem ser adicionados sem quebrar o domínio.
/// </summary>
public enum PaymentMethod
{
	/// <summary>Cartão de crédito.</summary>
	CreditCard = 1,

	/// <summary>Cartão de débito.</summary>
	DebitCard = 2,

	/// <summary>Pagamento instantâneo via Pix.</summary>
	Pix = 3,

	/// <summary>Boleto bancário.</summary>
	BankSlip = 4
}