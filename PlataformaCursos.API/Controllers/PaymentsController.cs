using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlataformaCursos.API.Application.Services;
using PlataformaCursos.API.Application.Common;
using PlataformaCursos.API.Domain.DTOs.Payments;
using System.Security.Claims;

namespace PlataformaCursos.API.Controllers;

/// <summary>
/// Gerencia pagamentos de matrículas
/// </summary>
[ApiController]
[Tags("Payments")]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
	private readonly PaymentService _service;
	private readonly IValidator<CreatePaymentDto> _createValidator;
	private readonly IValidator<ConfirmPaymentDto> _confirmValidator;

	public PaymentsController(
		PaymentService service,
		IValidator<CreatePaymentDto> createValidator,
		IValidator<ConfirmPaymentDto> confirmValidator)
	{
		_service = service;
		_createValidator = createValidator;
		_confirmValidator = confirmValidator;
	}

	// ==================================================
	// POST /api/payments
	// ==================================================

	/// <summary>
	/// Cria uma intenção de pagamento para uma matrícula
	/// </summary>
	/// <remarks>
	/// Regras:
	/// - A matrícula deve existir e estar ativa
	/// - Não pode haver outro pagamento ativo para a mesma matrícula
	/// - Idempotência: envie o header Idempotency-Key para evitar duplicatas
	///
	/// Retorna:
	/// - 201: pagamento criado
	/// - 400: campos inválidos
	/// - 409: já existe pagamento ativo para esta matrícula
	/// - 422: regra de negócio violada
	/// </remarks>
	/// <response code="201">Pagamento criado</response>
	/// <response code="400">Entrada inválida</response>
	/// <response code="401">Não autenticado</response>
	/// <response code="403">Sem permissão</response>
	/// <response code="409">Pagamento duplicado</response>
	/// <response code="422">Regra de negócio</response>
	[HttpPost]
	[Authorize(Roles = "Admin,Student")]
	[ProducesResponseType(typeof(PaymentResponseDto), StatusCodes.Status201Created)]
	[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	[ProducesResponseType(StatusCodes.Status409Conflict)]
	[ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
	public async Task<IActionResult> Create(
		[FromBody] CreatePaymentDto dto,
		[FromHeader(Name = "Idempotency-Key")] string? idempotencyKey)
	{
		// Validação da entrada via FluentValidation
		var validation = await _createValidator.ValidateAsync(dto);
		if (!validation.IsValid)
			return ValidationProblem(
				new ValidationProblemDetails(
					validation.ToDictionary()));

		var key = idempotencyKey ?? Guid.NewGuid().ToString();

		try
		{
			var result = await _service.CreateAsync(dto, User, key);
			return Created(string.Empty, result);
		}
		catch (UnauthorizedAccessException)
		{
			return Forbid();
		}
		catch (InvalidOperationException ex)
		{
			return Conflict(new { message = ex.Message });
		}
		catch (ApplicationException ex)
		{
			return UnprocessableEntity(new { message = ex.Message });
		}
	}

	// ==================================================
	// POST /api/payments/{id}/confirm
	// ==================================================

	/// <summary>
	/// Confirma um pagamento pendente (Admin)
	/// </summary>
	/// <remarks>
	/// Transição de estado: Pending → Paid.
	/// Apenas administradores podem confirmar pagamentos.
	/// </remarks>
	/// <param name="id">Id do pagamento</param>
	/// <param name="dto">Dados de confirmação (transactionId do gateway)</param>
	/// <response code="200">Pagamento confirmado</response>
	/// <response code="400">Entrada inválida</response>
	/// <response code="401">Não autenticado</response>
	/// <response code="403">Sem permissão</response>
	/// <response code="404">Pagamento não encontrado</response>
	/// <response code="422">Transição de estado inválida</response>
	[HttpPost("{id}/confirm")]
	[Authorize(Roles = "Admin")]
	[ProducesResponseType(typeof(PaymentResponseDto), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
	public async Task<IActionResult> Confirm(
		Guid id,
		[FromBody] ConfirmPaymentDto dto)
	{
		var validation = await _confirmValidator.ValidateAsync(dto);
		if (!validation.IsValid)
			return ValidationProblem(
				new ValidationProblemDetails(
					validation.ToDictionary()));

		try
		{
			var result = await _service.ConfirmAsync(id, dto);
			return Ok(result);
		}
		catch (InvalidOperationException ex)
		{
			return UnprocessableEntity(new { message = ex.Message });
		}
		catch (ApplicationException ex)
		{
			return NotFound(new { message = ex.Message });
		}
	}

	// ==================================================
	// POST /api/payments/{id}/fail
	// ==================================================

	/// <summary>
	/// Marca um pagamento como falho (Admin)
	/// </summary>
	/// <param name="id">Id do pagamento</param>
	/// <param name="reason">Motivo da falha</param>
	/// <response code="200">Pagamento marcado como falho</response>
	/// <response code="400">Motivo obrigatório</response>
	/// <response code="401">Não autenticado</response>
	/// <response code="403">Sem permissão</response>
	/// <response code="404">Pagamento não encontrado</response>
	/// <response code="422">Transição de estado inválida</response>
	[HttpPost("{id}/fail")]
	[Authorize(Roles = "Admin")]
	[ProducesResponseType(typeof(PaymentResponseDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
	public async Task<IActionResult> Fail(Guid id, [FromQuery] string reason)
	{
		if (string.IsNullOrWhiteSpace(reason))
			return BadRequest(new { message = "Motivo da falha é obrigatório." });

		try
		{
			var result = await _service.FailAsync(id, reason);
			return Ok(result);
		}
		catch (InvalidOperationException ex)
		{
			return UnprocessableEntity(new { message = ex.Message });
		}
		catch (ApplicationException ex)
		{
			return NotFound(new { message = ex.Message });
		}
	}

	// ==================================================
	// POST /api/payments/{id}/refund
	// ==================================================

	/// <summary>
	/// Estorna um pagamento confirmado (Admin)
	/// </summary>
	/// <param name="id">Id do pagamento</param>
	/// <response code="200">Pagamento estornado</response>
	/// <response code="401">Não autenticado</response>
	/// <response code="403">Sem permissão</response>
	/// <response code="404">Pagamento não encontrado</response>
	/// <response code="422">Transição de estado inválida</response>
	[HttpPost("{id}/refund")]
	[Authorize(Roles = "Admin")]
	[ProducesResponseType(typeof(PaymentResponseDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
	public async Task<IActionResult> Refund(Guid id)
	{
		try
		{
			var result = await _service.RefundAsync(id);
			return Ok(result);
		}
		catch (InvalidOperationException ex)
		{
			return UnprocessableEntity(new { message = ex.Message });
		}
		catch (ApplicationException ex)
		{
			return NotFound(new { message = ex.Message });
		}
	}

	// ==================================================
	// GET /api/payments/{id}
	// ==================================================

	/// <summary>
	/// Busca um pagamento pelo Id
	/// </summary>
	/// <remarks>
	/// Student acessa apenas os próprios pagamentos.
	/// Admin acessa qualquer pagamento.
	/// </remarks>
	/// <param name="id">Id do pagamento</param>
	/// <response code="200">Pagamento encontrado</response>
	/// <response code="401">Não autenticado</response>
	/// <response code="403">Sem permissão</response>
	/// <response code="404">Não encontrado</response>
	[HttpGet("{id}")]
	[Authorize(Roles = "Admin,Student")]
	[ProducesResponseType(typeof(PaymentResponseDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> GetById(Guid id)
	{
		try
		{
			var result = await _service.GetByIdAsync(id, User);
			return Ok(result);
		}
		catch (UnauthorizedAccessException)
		{
			return Forbid();
		}
		catch (ApplicationException ex)
		{
			return NotFound(new { message = ex.Message });
		}
	}

	// ==================================================
	// GET /api/students/{studentId}/payments
	// ==================================================

	/// <summary>
	/// Lista pagamentos de um estudante (paginado)
	/// </summary>
	/// <remarks>
	/// Student acessa apenas os próprios pagamentos.
	/// Admin acessa pagamentos de qualquer estudante.
	/// </remarks>
	/// <param name="studentId">Id do estudante</param>
	/// <param name="page">Página (default: 1)</param>
	/// <param name="pageSize">Itens por página (default: 10)</param>
	/// <param name="status">Filtro por status (Pending, Paid, Failed, Refunded)</param>
	/// <response code="200">Lista retornada</response>
	/// <response code="401">Não autenticado</response>
	/// <response code="403">Sem permissão</response>
	[HttpGet("/api/students/{studentId}/payments")]
	[Authorize(Roles = "Admin,Student")]
	[ProducesResponseType(typeof(PagedResult<PaymentResponseDto>), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	public async Task<IActionResult> GetByStudent(
		string studentId,
		int page = 1,
		int pageSize = 10,
		string? status = null)
	{
		if (page <= 0 || pageSize <= 0)
			return BadRequest(new { message = "Parâmetros de paginação inválidos." });

		try
		{
			var result = await _service.GetByStudentAsync(
				studentId, User, page, pageSize, status);
			return Ok(result);
		}
		catch (UnauthorizedAccessException)
		{
			return Forbid();
		}
	}

	// ==================================================
	// GET /api/payments (Admin only)
	// ==================================================

	/// <summary>
	/// Lista todos os pagamentos para auditoria (Admin)
	/// </summary>
	/// <param name="page">Página (default: 1)</param>
	/// <param name="pageSize">Itens por página (default: 10)</param>
	/// <param name="status">Filtro por status</param>
	/// <param name="enrollmentId">Filtro por matrícula</param>
	/// <response code="200">Lista retornada</response>
	/// <response code="401">Não autenticado</response>
	/// <response code="403">Sem permissão</response>
	[HttpGet]
	[Authorize(Roles = "Admin")]
	[ProducesResponseType(typeof(PagedResult<PaymentResponseDto>), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	public async Task<IActionResult> GetAll(
		int page = 1,
		int pageSize = 10,
		string? status = null,
		Guid? enrollmentId = null)
	{
		if (page <= 0 || pageSize <= 0)
			return BadRequest(new { message = "Parâmetros de paginação inválidos." });

		var result = await _service.GetAllAsync(page, pageSize, status, enrollmentId);
		return Ok(result);
	}
}