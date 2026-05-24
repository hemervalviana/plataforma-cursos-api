namespace PlataformaCursos.API.Infrastructure.Middleware;

/// <summary>
/// Middleware que garante um ID único por requisição (Correlation ID).
/// 
/// Usado para rastrear uma requisição em todos os logs.
/// Se o cliente enviar o header X-Correlation-ID, usa o valor dele.
/// Caso contrário, gera um novo GUID.
/// 
/// Exemplo de log com correlation ID:
/// [INFO] PaymentService | CorrelationId=abc-123 | UserId=xyz | Criando pagamento
/// </summary>
public class CorrelationIdMiddleware
{
	private const string CorrelationIdHeader = "X-Correlation-ID";

	private readonly RequestDelegate _next;
	private readonly ILogger<CorrelationIdMiddleware> _logger;

	public CorrelationIdMiddleware(
		RequestDelegate next,
		ILogger<CorrelationIdMiddleware> logger)
	{
		_next = next;
		_logger = logger;
	}

	public async Task Invoke(HttpContext context)
	{
		// Usa o header enviado pelo cliente ou gera um novo
		var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault()
			?? Guid.NewGuid().ToString();

		// Disponibiliza para toda a requisição
		context.Items["CorrelationId"] = correlationId;

		// Devolve no response para o cliente rastrear também
		context.Response.Headers[CorrelationIdHeader] = correlationId;

		// Adiciona ao escopo de log — aparece em todos os logs da requisição
		using (_logger.BeginScope(new Dictionary<string, object>
		{
			["CorrelationId"] = correlationId,
			["Method"] = context.Request.Method,
			["Path"] = context.Request.Path
		}))
		{
			await _next(context);
		}
	}
}