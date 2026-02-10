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

	// =============================
	// DbSets
	// =============================

	public DbSet<Course> Courses { get; set; } = null!;
	public DbSet<Enrollment> Enrollments { get; set; } = null!;

	// =============================
	// Model Creating
	// =============================

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		// ⚠️ Sempre primeiro
		base.OnModelCreating(modelBuilder);

		// =============================
		// Domain Configurations
		// =============================

		modelBuilder.ApplyConfiguration(new CourseConfiguration());
		modelBuilder.ApplyConfiguration(new EnrollmentConfiguration());

		// ⚠️ Student herda IdentityUser
		// Cuidado para não sobrescrever mapeamento padrão
		modelBuilder.ApplyConfiguration(new StudentConfiguration());
	}
}
