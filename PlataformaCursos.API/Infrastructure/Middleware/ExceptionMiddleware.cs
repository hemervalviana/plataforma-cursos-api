using PlataformaCursos.API.Application.Exceptions;

namespace PlataformaCursos.API.Infrastructure.Middleware;

public class ExceptionMiddleware
{
	private readonly RequestDelegate _next;
	private readonly ILogger<ExceptionMiddleware> _logger;

	public ExceptionMiddleware(
		RequestDelegate next,
		ILogger<ExceptionMiddleware> logger)
	{
		_next = next;
		_logger = logger;
	}

	public async Task Invoke(HttpContext context)
	{
		try
		{
			await _next(context);
		}
		catch (DomainException ex)
		{
			_logger.LogWarning(ex, ex.Message);

			context.Response.StatusCode = ex.StatusCode;

			await context.Response.WriteAsJsonAsync(new
			{
				title = "Erro de regra de negócio",
				status = ex.StatusCode,
				detail = ex.Message
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, ex.Message);

			context.Response.StatusCode = 500;

			await context.Response.WriteAsJsonAsync(new
			{
				title = "Erro interno",
				status = 500,
				detail = "Erro inesperado no servidor"
			});
		}
	}
}
