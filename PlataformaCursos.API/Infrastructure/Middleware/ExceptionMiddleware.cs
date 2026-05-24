using Microsoft.AspNetCore.Mvc;
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
			// Erros de domínio — regras de negócio violadas
			_logger.LogWarning(ex, "Erro de domínio: {Message}", ex.Message);

			await WriteProblemDetails(
				context,
				statusCode: ex.StatusCode,
				title: "Erro de regra de negócio",
				detail: ex.Message);
		}
		catch (UnauthorizedAccessException ex)
		{
			// Acesso negado — estudante tentando acessar recurso de outro
			_logger.LogWarning(ex, "Acesso negado: {Message}", ex.Message);

			await WriteProblemDetails(
				context,
				statusCode: 403,
				title: "Acesso negado",
				detail: ex.Message);
		}
		catch (InvalidOperationException ex)
		{
			// Conflito — duplicidade, transição de estado inválida
			_logger.LogWarning(ex, "Conflito: {Message}", ex.Message);

			await WriteProblemDetails(
				context,
				statusCode: 409,
				title: "Conflito",
				detail: ex.Message);
		}
		catch (ApplicationException ex)
		{
			// Erros de aplicação — recurso não encontrado, dados inválidos
			_logger.LogWarning(ex, "Erro de aplicação: {Message}", ex.Message);

			await WriteProblemDetails(
				context,
				statusCode: 422,
				title: "Erro de processamento",
				detail: ex.Message);
		}
		catch (Exception ex)
		{
			// Erros inesperados — nunca expor detalhes internos
			_logger.LogError(ex, "Erro inesperado: {Message}", ex.Message);

			await WriteProblemDetails(
				context,
				statusCode: 500,
				title: "Erro interno do servidor",
				detail: "Ocorreu um erro inesperado. Tente novamente mais tarde.");
		}
	}

	// ==================================================
	// Escreve resposta no formato ProblemDetails (RFC 7807)
	// Padrão: { type, title, status, detail, traceId }
	// ==================================================
	private static async Task WriteProblemDetails(
		HttpContext context,
		int statusCode,
		string title,
		string detail)
	{
		context.Response.StatusCode = statusCode;
		context.Response.ContentType = "application/problem+json";

		var problem = new ProblemDetails
		{
			Type = $"https://tools.ietf.org/html/rfc9110#section-15.5.{statusCode - 399}",
			Title = title,
			Status = statusCode,
			Detail = detail,
			Extensions =
			{
                // TraceId para correlação de logs
                ["traceId"] = context.TraceIdentifier
			}
		};

		await context.Response.WriteAsJsonAsync(problem);
	}
}