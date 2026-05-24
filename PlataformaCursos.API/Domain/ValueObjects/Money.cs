namespace PlataformaCursos.API.Domain.ValueObjects;

/// <summary>
/// Value Object que representa um valor monetário com moeda.
/// 
/// Por ser um record, duas instâncias com mesmo Amount e Currency
/// são consideradas iguais — comportamento correto para VOs.
/// 
/// Imutável por design: uma vez criado, não pode ser alterado.
/// Para mudar o valor, cria-se uma nova instância.
/// </summary>
public record Money
{
	/// <summary>
	/// Valor monetário com precisão decimal.
	/// Nunca use float ou double para dinheiro — perda de precisão.
	/// </summary>
	public decimal Amount { get; init; }

	/// <summary>
	/// Código da moeda no padrão ISO 4217 (ex: BRL, USD, EUR).
	/// Padrão: BRL.
	/// </summary>
	public string Currency { get; init; }

	/// <summary>
	/// Cria um valor monetário válido.
	/// Lança exceção se o valor for zero ou negativo,
	/// ou se a moeda não for informada.
	/// </summary>
	/// <param name="amount">Valor maior que zero.</param>
	/// <param name="currency">Código da moeda (padrão: BRL).</param>
	public Money(decimal amount, string currency = "BRL")
	{
		// Regra de negócio: valor deve ser positivo
		if (amount <= 0)
			throw new ArgumentException(
				"Valor deve ser maior que zero.", nameof(amount));

		// Moeda é obrigatória para identificar a unidade monetária
		if (string.IsNullOrWhiteSpace(currency))
			throw new ArgumentException(
				"Moeda é obrigatória.", nameof(currency));

		Amount = amount;

		// Sempre armazena em maiúsculo para consistência (brl → BRL)
		Currency = currency.ToUpper();
	}
}