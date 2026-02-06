using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlataformaCursos.API.Domain.Entities;

namespace PlataformaCursos.API.Infrastructure.Data.Configurations;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
	public void Configure(EntityTypeBuilder<Course> builder)
	{
		builder.ToTable("Courses");

		builder.HasKey(c => c.Id);

		builder.Property(c => c.Title)
			   .IsRequired()
			   .HasMaxLength(200);
		builder.HasIndex(c => c.Title).IsUnique();

		builder.Property(c => c.Description)
			   .HasMaxLength(2000)
			   .IsRequired(false);

		builder.Property(c => c.Category)
			   .IsRequired()
			   .HasMaxLength(100);

		builder.Property(c => c.Workload).IsRequired();
		builder.Property(c => c.CreatedAt).IsRequired();
		builder.Property(c => c.IsDeleted).HasDefaultValue(false);

		builder.HasIndex(c => c.Category);

		builder.HasMany(c => c.Enrollments)
			   .WithOne(e => e.Course)
			   .HasForeignKey(e => e.CourseId)
			   .OnDelete(DeleteBehavior.Cascade);

		// Global filter para soft delete
		builder.HasQueryFilter(c => !c.IsDeleted);
	}
}
