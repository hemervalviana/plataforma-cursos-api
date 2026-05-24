using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlataformaCursos.API.Domain.Entities;
using PlataformaCursos.API.Domain.Enums;

namespace PlataformaCursos.API.Infrastructure.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
	public void Configure(EntityTypeBuilder<Payment> builder)
	{
		builder.ToTable("Payments");

		// ======================================================
		// Chave primária
		// ======================================================
		builder.HasKey(p => p.Id);

		// ======================================================
		// Idempotência — chave única por cliente
		// Garante que a mesma requisição não crie dois pagamentos
		// ======================================================
		builder.HasIndex(p => p.IdempotencyKey)
			   .IsUnique();

		// ======================================================
		// Unicidade: apenas um pagamento ativo (Pending ou Paid)
		// por matrícula — regra de negócio crítica
		// ======================================================
		builder.HasIndex(p => new { p.EnrollmentId, p.Status })
			   .HasFilter("[Status] IN (1, 2)") // Pending = 1, Paid = 2
			   .IsUnique();

		// ======================================================
		// Índices de performance para consultas frequentes
		// ======================================================
		builder.HasIndex(p => p.StudentId);
		builder.HasIndex(p => p.EnrollmentId);
		builder.HasIndex(p => p.Status);

		// ======================================================
		// Value Object Money — mapeado como owned entity
		// Armazena Amount e Currency como colunas da tabela Payments
		// sem criar uma tabela separada
		// ======================================================
		builder.OwnsOne(p => p.Amount, money =>
		{
			money.Property(m => m.Amount)
				 .HasColumnName("Amount")
				 .HasColumnType("decimal(18,2)") // precisão financeira
				 .IsRequired();

			money.Property(m => m.Currency)
				 .HasColumnName("Currency")
				 .HasMaxLength(3)               // BRL, USD, EUR
				 .IsRequired();
		});

		// ======================================================
		// Status — armazenado como int, enum no domínio
		// ======================================================
		builder.Property(p => p.Status)
	   .IsRequired();

		// ======================================================
		// Método de pagamento
		// ======================================================
		builder.Property(p => p.Method)
			   .IsRequired();

		// ======================================================
		// Idempotency Key
		// ======================================================
		builder.Property(p => p.IdempotencyKey)
			   .IsRequired()
			   .HasMaxLength(100);

		// ======================================================
		// Campos opcionais
		// ======================================================
		builder.Property(p => p.TransactionId)
			   .HasMaxLength(200);

		builder.Property(p => p.FailureReason)
			   .HasMaxLength(500);

		// ======================================================
		// Datas
		// ======================================================
		builder.Property(p => p.CreatedAt).IsRequired();
		builder.Property(p => p.PaidAt);
		builder.Property(p => p.UpdatedAt);

		// ======================================================
		// Soft delete
		// ======================================================
		builder.Property(p => p.IsDeleted)
			   .HasDefaultValue(false);

		// ======================================================
		// Relacionamentos
		// ======================================================

		// Payment → Enrollment (N:1)
		builder.HasOne(p => p.Enrollment)
			   .WithMany()
			   .HasForeignKey(p => p.EnrollmentId)
			   .OnDelete(DeleteBehavior.Restrict); // não apaga pagamento em cascata

		// Payment → Student (N:1)
		builder.HasOne(p => p.Student)
			   .WithMany()
			   .HasForeignKey(p => p.StudentId)
			   .OnDelete(DeleteBehavior.Restrict);

		// ======================================================
		// Global filter — exclui soft deleted de todas as queries
		// ======================================================
		builder.HasQueryFilter(p => !p.IsDeleted);
	}
}