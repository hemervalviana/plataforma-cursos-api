using AutoMapper;
using global::PlataformaCursos.API.Domain.DTOs.Courses;
using global::PlataformaCursos.API.Domain.Entities;
using PlataformaCursos.API.Domain.DTOs.Courses;
using PlataformaCursos.API.Domain.Entities;

namespace PlataformaCursos.API.Application.Mapping;

public class CourseProfile : Profile
{
	public CourseProfile()
	{
		CreateMap<CreateCourseDto, Course>();

		CreateMap<UpdateCourseDto, Course>();

		CreateMap<Course, CourseResponseDto>();
	}
}
