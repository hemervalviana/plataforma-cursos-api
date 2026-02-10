using FluentValidation;
using PlataformaCursos.API.Domain.DTOs.Students;

namespace PlataformaCursos.API.Application.Validators.Students;

public class CreateStudentDtoValidator
	: AbstractValidator<CreateStudentDto>
{
	public CreateStudentDtoValidator()
	{
		RuleFor(x => x.FullName)
			.NotEmpty()
			.MinimumLength(3);

		RuleFor(x => x.Email)
			.NotEmpty()
			.EmailAddress();
	}
}
