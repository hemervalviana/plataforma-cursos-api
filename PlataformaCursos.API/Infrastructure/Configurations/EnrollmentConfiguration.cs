using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlataformaCursos.API.Domain.Entities;

namespace PlataformaCursos.API.Infrastructure.Data.Configurations;

public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
	public void Configure(EntityTypeBuilder<Enrollment> builder)
	{
		builder.ToTable("Enrollments");

		builder.HasKey(e => e.Id);

		builder.Property(e => e.Status)
			   .IsRequired()
			   .HasMaxLength(50)
			   .HasDefaultValue("Ativo");

		builder.Property(e => e.CreatedAt).IsRequired();
		builder.Property(e => e.IsDeleted).HasDefaultValue(false);

		builder.HasOne(e => e.Course)
			   .WithMany(c => c.Enrollments)
			   .HasForeignKey(e => e.CourseId)
			   .OnDelete(DeleteBehavior.Cascade);

		builder.HasOne(e => e.Student)
			   .WithMany(s => s.Enrollments)
			   .HasForeignKey(e => e.StudentId)
			   .OnDelete(DeleteBehavior.Cascade);

		builder.HasIndex(e => new { e.StudentId, e.CourseId }).IsUnique();

		// Global filter consistente
		builder.HasQueryFilter(e => !e.IsDeleted && !e.Course.IsDeleted && !e.Student.IsDeleted);
	}
}
