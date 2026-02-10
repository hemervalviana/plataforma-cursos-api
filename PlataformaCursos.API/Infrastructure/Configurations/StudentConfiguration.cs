using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlataformaCursos.API.Domain.Entities;

namespace PlataformaCursos.API.Infrastructure.Data.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
	public void Configure(EntityTypeBuilder<Student> builder)
	{
		builder.ToTable("Students");

		builder.Property(s => s.FullName)
			   .IsRequired()
			   .HasMaxLength(200);

		builder.Property(s => s.CreatedAt)
			   .IsRequired()
			   .HasDefaultValueSql("GETUTCDATE()");

		builder.Property(s => s.IsActive)
			   .HasDefaultValue(true);

		builder.Property(s => s.IsDeleted)
			   .HasDefaultValue(false);

		// ============================
		// Email único (Identity)
		// ============================

		builder.HasIndex(s => s.NormalizedEmail)
			   .IsUnique();

		builder.HasIndex(s => s.NormalizedUserName)
			   .IsUnique();

		// ============================
		// Soft Delete Global Filter
		// ============================

		builder.HasQueryFilter(s => !s.IsDeleted);
	}
}
