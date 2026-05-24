using FluentValidation;
using PlataformaCursos.API.Domain.DTOs.Payments;
using PlataformaCursos.API.Domain.Enums;

namespace PlataformaCursos.API.Application.Validators.Payments;

public class CreatePaymentDtoValidator : AbstractValidator<CreatePaymentDto>
{
	public CreatePaymentDtoValidator()
	{
		// EnrollmentId obrigatório e não pode ser vazio
		RuleFor(x => x.EnrollmentId)
			.NotEmpty()
			.WithMessage("EnrollmentId é obrigatório.");

		// Valor deve ser positivo
		RuleFor(x => x.Amount)
			.GreaterThan(0)
			.WithMessage("O valor do pagamento deve ser maior que zero.")
			.LessThanOrEqualTo(99999)
			.WithMessage("O valor do pagamento excede o limite permitido.");

		// Moeda obrigatória e formato válido (3 letras)
		RuleFor(x => x.Currency)
			.NotEmpty()
			.WithMessage("Moeda é obrigatória.")
			.Length(3)
			.WithMessage("Moeda deve ter 3 caracteres (ex: BRL, USD).")
			.Matches("^[A-Za-z]{3}$")
			.WithMessage("Moeda deve conter apenas letras (ex: BRL, USD).");

		// Método de pagamento deve ser um valor válido do enum
		RuleFor(x => x.Method)
			.IsInEnum()
			.WithMessage("Método de pagamento inválido. Use: 1=CreditCard, 2=DebitCard, 3=Pix, 4=BankSlip.");
	}
}