using FluentValidation;
using PlataformaCursos.API.Domain.DTOs.Payments;

namespace PlataformaCursos.API.Application.Validators.Payments;

public class ConfirmPaymentDtoValidator : AbstractValidator<ConfirmPaymentDto>
{
	public ConfirmPaymentDtoValidator()
	{
		RuleFor(x => x.TransactionId)
			.NotEmpty()
			.WithMessage("TransactionId é obrigatório.")
			.MaximumLength(200)
			.WithMessage("TransactionId não pode exceder 200 caracteres.");
	}
}