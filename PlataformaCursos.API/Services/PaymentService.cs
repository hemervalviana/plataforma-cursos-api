using PlataformaCursos.API.Application.Common;
using PlataformaCursos.API.Application.Interfaces;
using PlataformaCursos.API.Domain.DTOs.Payments;
using PlataformaCursos.API.Domain.Entities;
using PlataformaCursos.API.Domain.Enums;
using PlataformaCursos.API.Domain.ValueObjects;
using PlataformaCursos.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace PlataformaCursos.API.Application.Services;

public class PaymentService
{
	private readonly IPaymentRepository _repository;
	private readonly IPaymentGateway _gateway;
	private readonly ApplicationDbContext _context;
	private readonly ILogger<PaymentService> _logger;

	public PaymentService(
		IPaymentRepository repository,
		IPaymentGateway gateway,
		ApplicationDbContext context,
		ILogger<PaymentService> logger)
	{
		_repository = repository;
		_gateway = gateway;
		_context = context;
		_logger = logger;
	}

	// ==================================================
	// CREATE
	// ==================================================
	public async Task<PaymentResponseDto> CreateAsync(
		CreatePaymentDto dto,
		ClaimsPrincipal user,
		string idempotencyKey,
		CancellationToken cancellationToken = default)
	{
		var isAdmin = user.IsInRole("Admin");
		var studentId = user.FindFirstValue(ClaimTypes.NameIdentifier)
			?? throw new ApplicationException("Usuário inválido.");

		_logger.LogInformation(
			"Criando pagamento. StudentId={StudentId} EnrollmentId={EnrollmentId} Key={Key}",
			studentId, dto.EnrollmentId, idempotencyKey);

		// Idempotência
		var existing = await _repository.GetByIdempotencyKeyAsync(
			idempotencyKey, cancellationToken);

		if (existing != null)
		{
			_logger.LogInformation(
				"Pagamento idempotente retornado. PaymentId={PaymentId} Key={Key}",
				existing.Id, idempotencyKey);

			return ToDto(existing);
		}

		// Valida matrícula
		var enrollment = await _context.Enrollments
			.FirstOrDefaultAsync(e =>
				e.Id == dto.EnrollmentId && !e.IsDeleted,
				cancellationToken);

		if (enrollment == null)
		{
			_logger.LogWarning(
				"Matrícula não encontrada. EnrollmentId={EnrollmentId} StudentId={StudentId}",
				dto.EnrollmentId, studentId);

			throw new ApplicationException("Matrícula não encontrada.");
		}

		if (!isAdmin && enrollment.StudentId != studentId)
		{
			_logger.LogWarning(
				"Acesso negado ao pagamento. StudentId={StudentId} EnrollmentOwner={Owner}",
				studentId, enrollment.StudentId);

			throw new UnauthorizedAccessException(
				"Sem permissão para pagar esta matrícula.");
		}

		// Unicidade
		var hasActive = await _repository.HasActivePaymentAsync(
			dto.EnrollmentId, cancellationToken);

		if (hasActive)
		{
			_logger.LogWarning(
				"Pagamento duplicado bloqueado. EnrollmentId={EnrollmentId} StudentId={StudentId}",
				dto.EnrollmentId, studentId);

			throw new InvalidOperationException(
				"Já existe um pagamento ativo para esta matrícula.");
		}

		// Gateway
		_logger.LogInformation(
			"Chamando gateway. EnrollmentId={EnrollmentId} Amount={Amount} Method={Method}",
			dto.EnrollmentId, dto.Amount, dto.Method);

		var gatewayResult = await _gateway.ProcessAsync(
			idempotencyKey, dto.Amount, dto.Currency, dto.Method.ToString());

		var money = new Money(dto.Amount, dto.Currency);
		var payment = Payment.Create(
			enrollmentId: dto.EnrollmentId,
			studentId: enrollment.StudentId,
			amount: money,
			method: dto.Method,
			idempotencyKey: idempotencyKey);

		if (gatewayResult.Success)
		{
			payment.Confirm(gatewayResult.TransactionId!);

			_logger.LogInformation(
				"Pagamento confirmado pelo gateway. PaymentId={PaymentId} TransactionId={TxId}",
				payment.Id, gatewayResult.TransactionId);
		}
		else
		{
			payment.Fail(gatewayResult.ErrorMessage!);

			_logger.LogWarning(
				"Pagamento recusado pelo gateway. PaymentId={PaymentId} Reason={Reason}",
				payment.Id, gatewayResult.ErrorMessage);
		}

		await _repository.AddAsync(payment, cancellationToken);
		await _repository.SaveChangesAsync(cancellationToken);

		return ToDto(payment);
	}

	// ==================================================
	// CONFIRM
	// ==================================================
	public async Task<PaymentResponseDto> ConfirmAsync(
		Guid id,
		ConfirmPaymentDto dto,
		CancellationToken cancellationToken = default)
	{
		_logger.LogInformation(
			"Confirmando pagamento. PaymentId={PaymentId}", id);

		var payment = await _repository.GetByIdAsync(id, cancellationToken)
			?? throw new ApplicationException("Pagamento não encontrado.");

		var gatewayResult = await _gateway.ConfirmAsync(dto.TransactionId);

		if (!gatewayResult.Success)
		{
			_logger.LogError(
				"Falha ao confirmar no gateway. PaymentId={PaymentId} Reason={Reason}",
				id, gatewayResult.ErrorMessage);

			throw new InvalidOperationException(
				gatewayResult.ErrorMessage ?? "Erro ao confirmar no gateway.");
		}

		payment.Confirm(gatewayResult.TransactionId!);

		await _repository.SaveChangesAsync(cancellationToken);

		_logger.LogInformation(
			"Pagamento confirmado com sucesso. PaymentId={PaymentId} TransactionId={TxId}",
			id, gatewayResult.TransactionId);

		return ToDto(payment);
	}

	// ==================================================
	// FAIL
	// ==================================================
	public async Task<PaymentResponseDto> FailAsync(
		Guid id,
		string reason,
		CancellationToken cancellationToken = default)
	{
		_logger.LogWarning(
			"Marcando pagamento como falho. PaymentId={PaymentId} Reason={Reason}",
			id, reason);

		var payment = await _repository.GetByIdAsync(id, cancellationToken)
			?? throw new ApplicationException("Pagamento não encontrado.");

		payment.Fail(reason);

		await _repository.SaveChangesAsync(cancellationToken);

		return ToDto(payment);
	}

	// ==================================================
	// REFUND
	// ==================================================
	public async Task<PaymentResponseDto> RefundAsync(
		Guid id,
		CancellationToken cancellationToken = default)
	{
		_logger.LogInformation(
			"Iniciando estorno. PaymentId={PaymentId}", id);

		var payment = await _repository.GetByIdAsync(id, cancellationToken)
			?? throw new ApplicationException("Pagamento não encontrado.");

		var gatewayResult = await _gateway.RefundAsync(payment.TransactionId!);

		if (!gatewayResult.Success)
		{
			_logger.LogError(
				"Falha ao estornar no gateway. PaymentId={PaymentId} Reason={Reason}",
				id, gatewayResult.ErrorMessage);

			throw new InvalidOperationException(
				gatewayResult.ErrorMessage ?? "Erro ao estornar no gateway.");
		}

		payment.Refund();

		await _repository.SaveChangesAsync(cancellationToken);

		_logger.LogInformation(
			"Estorno concluído. PaymentId={PaymentId} RefundId={RefundId}",
			id, gatewayResult.TransactionId);

		return ToDto(payment);
	}

	// ==================================================
	// GET BY ID
	// ==================================================
	public async Task<PaymentResponseDto> GetByIdAsync(
		Guid id,
		ClaimsPrincipal user,
		CancellationToken cancellationToken = default)
	{
		var payment = await _repository.GetByIdAsync(id, cancellationToken)
			?? throw new ApplicationException("Pagamento não encontrado.");

		var studentId = user.FindFirstValue(ClaimTypes.NameIdentifier);
		var isAdmin = user.IsInRole("Admin");

		if (!isAdmin && payment.StudentId != studentId)
		{
			_logger.LogWarning(
				"Acesso negado ao pagamento. PaymentId={PaymentId} StudentId={StudentId}",
				id, studentId);

			throw new UnauthorizedAccessException(
				"Sem permissão para acessar este pagamento.");
		}

		return ToDto(payment);
	}

	// ==================================================
	// LIST — por estudante
	// ==================================================
	public async Task<PagedResult<PaymentResponseDto>> GetByStudentAsync(
		string studentId,
		ClaimsPrincipal user,
		int page,
		int pageSize,
		string? status,
		CancellationToken cancellationToken = default)
	{
		var currentUserId = user.FindFirstValue(ClaimTypes.NameIdentifier);
		var isAdmin = user.IsInRole("Admin");

		if (!isAdmin && currentUserId != studentId)
			throw new UnauthorizedAccessException(
				"Sem permissão para acessar estes pagamentos.");

		PaymentStatus? parsedStatus = null;
		if (!string.IsNullOrWhiteSpace(status) &&
			Enum.TryParse<PaymentStatus>(status, true, out var s))
			parsedStatus = s;

		var (data, total) = await _repository.GetByStudentAsync(
			studentId, parsedStatus, page, pageSize, cancellationToken);

		return new PagedResult<PaymentResponseDto>
		{
			Page = page,
			PageSize = pageSize,
			Total = total,
			Data = data.Select(ToDto).ToList()
		};
	}

	// ==================================================
	// LIST ALL — Admin
	// ==================================================
	public async Task<PagedResult<PaymentResponseDto>> GetAllAsync(
		int page,
		int pageSize,
		string? status,
		Guid? enrollmentId,
		CancellationToken cancellationToken = default)
	{
		PaymentStatus? parsedStatus = null;
		if (!string.IsNullOrWhiteSpace(status) &&
			Enum.TryParse<PaymentStatus>(status, true, out var s))
			parsedStatus = s;

		var (data, total) = await _repository.GetAllAsync(
			parsedStatus, enrollmentId, page, pageSize, cancellationToken);

		return new PagedResult<PaymentResponseDto>
		{
			Page = page,
			PageSize = pageSize,
			Total = total,
			Data = data.Select(ToDto).ToList()
		};
	}

	// ==================================================
	// Mapeamento interno
	// ==================================================
	private static PaymentResponseDto ToDto(Payment payment) => new()
	{
		Id = payment.Id,
		EnrollmentId = payment.EnrollmentId,
		StudentId = payment.StudentId,
		Amount = payment.Amount.Amount,
		Currency = payment.Amount.Currency,
		Status = payment.Status.ToString(),
		Method = payment.Method.ToString(),
		TransactionId = payment.TransactionId,
		FailureReason = payment.FailureReason,
		CreatedAt = payment.CreatedAt,
		PaidAt = payment.PaidAt
	};
}