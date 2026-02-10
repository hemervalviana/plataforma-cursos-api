using FluentValidation;
using PlataformaCursos.API.Domain.DTOs.Courses;

namespace PlataformaCursos.API.Application.Validators.Courses;

public class CreateCourseDtoValidator
	: AbstractValidator<CreateCourseDto>
{
	public CreateCourseDtoValidator()
	{
		RuleFor(x => x.Title)
			.NotEmpty()
			.MinimumLength(3);

		RuleFor(x => x.Category)
			.NotEmpty();

		RuleFor(x => x.Workload)
			.GreaterThan(0);
	}
}
