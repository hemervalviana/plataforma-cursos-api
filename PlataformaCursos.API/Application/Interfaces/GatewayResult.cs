namespace PlataformaCursos.API.Application.Interfaces;

/// <summary>
/// Resultado padronizado retornado pelo gateway de pagamento.
/// Mapeia a resposta externa para o domínio da aplicação.
/// Independente de qual gateway for usado (Stripe, PagSeguro, Fake).
/// </summary>
public class GatewayResult
{
	/// <summary>
	/// Indica se a operação foi bem-sucedida no gateway.
	/// </summary>
	public bool Success { get; init; }

	/// <summary>
	/// Id da transação gerado pelo gateway.
	/// Usado para rastreio e conciliação financeira.
	/// Preenchido apenas quando Success = true.
	/// </summary>
	public string? TransactionId { get; init; }

	/// <summary>
	/// Mensagem de erro retornada pelo gateway.
	/// Preenchida apenas quando Success = false.
	/// Nunca expor dados sensíveis aqui.
	/// </summary>
	public string? ErrorMessage { get; init; }

	/// <summary>
	/// Data/hora da transação no gateway (UTC).
	/// </summary>
	public DateTime ProcessedAt { get; init; }

	// ==================================================
	// Factory Methods — criam resultados de forma legível
	// ==================================================

	/// <summary>
	/// Cria um resultado de sucesso com o id da transação.
	/// </summary>
	public static GatewayResult Ok(string transactionId) => new()
	{
		Success = true,
		TransactionId = transactionId,
		ProcessedAt = DateTime.UtcNow
	};

	/// <summary>
	/// Cria um resultado de falha com o motivo.
	/// </summary>
	public static GatewayResult Fail(string errorMessage) => new()
	{
		Success = false,
		ErrorMessage = errorMessage,
		ProcessedAt = DateTime.UtcNow
	};
}