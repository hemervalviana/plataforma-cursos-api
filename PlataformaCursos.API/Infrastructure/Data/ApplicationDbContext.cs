using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PlataformaCursos.API.Domain.Entities;
using PlataformaCursos.API.Infrastructure.Data.Configurations;

namespace PlataformaCursos.API.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<Student>
{
	public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
		: base(options)
	{
	}

	// DbSets do domínio
	public DbSet<Course> Courses { get; set; } = null!;
	public DbSet<Enrollment> Enrollments { get; set; } = null!;

	// DbSet<Student> não é necessário, Identity já gerencia

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.ApplyConfiguration(new CourseConfiguration());
		modelBuilder.ApplyConfiguration(new StudentConfiguration());
		modelBuilder.ApplyConfiguration(new EnrollmentConfiguration());
	}
}
