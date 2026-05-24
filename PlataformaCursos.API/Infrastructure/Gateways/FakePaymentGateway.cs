using PlataformaCursos.API.Application.Interfaces;

namespace PlataformaCursos.API.Infrastructure.Gateways;

/// <summary>
/// Implementação simulada do gateway de pagamento.
/// 
/// Usada em desenvolvimento e testes — sem chamadas externas reais.
/// Respostas são determinísticas para cobrir todos os cenários:
/// 
/// - Valor terminando em .99  → simula falha (cartão recusado)
/// - Valor acima de 9999      → simula timeout
/// - Qualquer outro valor     → sucesso
/// 
/// Para trocar pelo gateway real (ex: Stripe):
/// 1. Crie StripePaymentGateway : IPaymentGateway
/// 2. No Program.cs troque: AddScoped&lt;IPaymentGateway, StripePaymentGateway&gt;
/// 3. Nenhuma outra alteração necessária.
/// </summary>
public class FakePaymentGateway : IPaymentGateway
{
	private readonly ILogger<FakePaymentGateway> _logger;

	public FakePaymentGateway(ILogger<FakePaymentGateway> logger)
	{
		_logger = logger;
	}

	/// <summary>
	/// Processa pagamento de forma simulada.
	/// </summary>
	public async Task<GatewayResult> ProcessAsync(
		string idempotencyKey,
		decimal amount,
		string currency,
		string method)
	{
		_logger.LogInformation(
			"FakeGateway: processando pagamento. Key={Key} Amount={Amount} {Currency} Method={Method}",
			idempotencyKey, amount, currency, method);

		// Simula latência de rede (50ms)
		await Task.Delay(50);

		// Simula timeout para valores muito altos
		if (amount > 9999)
		{
			_logger.LogWarning(
				"FakeGateway: timeout simulado para Amount={Amount}", amount);

			return GatewayResult.Fail("Gateway timeout: tente novamente.");
		}

		// Simula falha para valores terminando em .99
		if (amount % 1 == 0.99m)
		{
			_logger.LogWarning(
				"FakeGateway: pagamento recusado para Amount={Amount}", amount);

			return GatewayResult.Fail("Pagamento recusado pela operadora.");
		}

		// Gera um TransactionId simulado único
		var transactionId = $"FAKE-{idempotencyKey[..8].ToUpper()}-{DateTime.UtcNow:yyyyMMddHHmmss}";

		_logger.LogInformation(
			"FakeGateway: pagamento aprovado. TransactionId={TransactionId}",
			transactionId);

		return GatewayResult.Ok(transactionId);
	}

	/// <summary>
	/// Confirma transação de forma simulada.
	/// </summary>
	public async Task<GatewayResult> ConfirmAsync(string transactionId)
	{
		_logger.LogInformation(
			"FakeGateway: confirmando transação. TransactionId={TransactionId}",
			transactionId);

		await Task.Delay(30);

		if (string.IsNullOrWhiteSpace(transactionId))
			return GatewayResult.Fail("TransactionId inválido.");

		return GatewayResult.Ok(transactionId);
	}

	/// <summary>
	/// Estorna transação de forma simulada.
	/// </summary>
	public async Task<GatewayResult> RefundAsync(string transactionId)
	{
		_logger.LogInformation(
			"FakeGateway: estornando transação. TransactionId={TransactionId}",
			transactionId);

		await Task.Delay(30);

		if (string.IsNullOrWhiteSpace(transactionId))
			return GatewayResult.Fail("TransactionId inválido para estorno.");

		var refundId = $"REFUND-{transactionId}";

		_logger.LogInformation(
			"FakeGateway: estorno aprovado. RefundId={RefundId}", refundId);

		return GatewayResult.Ok(refundId);
	}
}