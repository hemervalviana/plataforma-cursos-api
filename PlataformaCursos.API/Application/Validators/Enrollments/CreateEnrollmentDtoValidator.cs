using FluentValidation;
using PlataformaCursos.API.Domain.DTOs.Enrollments;

namespace PlataformaCursos.API.Application.Validators.Enrollments;

public class CreateEnrollmentDtoValidator
	: AbstractValidator<CreateEnrollmentDto>
{
	public CreateEnrollmentDtoValidator()
	{
		RuleFor(x => x.CourseId)
			.NotEmpty();
	}
}
