namespace PlataformaCursos.API.Application.Interfaces;

/// <summary>
/// Contrato do gateway de pagamento.
/// 
/// O domínio depende apenas desta interface — nunca da implementação concreta.
/// Isso permite trocar o gateway (Stripe, PagSeguro, Mercado Pago)
/// sem alterar nenhuma regra de negócio.
/// 
/// Padrão: Adapter (isola integração externa do domínio).
/// </summary>
public interface IPaymentGateway
{
	/// <summary>
	/// Processa uma intenção de pagamento no gateway.
	/// </summary>
	/// <param name="idempotencyKey">
	/// Chave única da operação — evita duplicatas no gateway externo.
	/// </param>
	/// <param name="amount">Valor do pagamento.</param>
	/// <param name="currency">Moeda (ex: BRL).</param>
	/// <param name="method">Método de pagamento (ex: Pix, CreditCard).</param>
	/// <returns>Resultado da operação no gateway.</returns>
	Task<GatewayResult> ProcessAsync(
		string idempotencyKey,
		decimal amount,
		string currency,
		string method);

	/// <summary>
	/// Confirma uma transação já processada no gateway.
	/// </summary>
	/// <param name="transactionId">Id da transação no gateway.</param>
	/// <returns>Resultado da confirmação.</returns>
	Task<GatewayResult> ConfirmAsync(string transactionId);

	/// <summary>
	/// Estorna uma transação confirmada no gateway.
	/// </summary>
	/// <param name="transactionId">Id da transação no gateway.</param>
	/// <returns>Resultado do estorno.</returns>
	Task<GatewayResult> RefundAsync(string transactionId);
}