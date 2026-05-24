using FluentAssertions;
using PlataformaCursos.API.Domain.Entities;
using PlataformaCursos.API.Domain.Enums;
using PlataformaCursos.API.Domain.ValueObjects;

namespace PlataformaCursos.Tests.Unit;

/// <summary>
/// Testes de UNIDADE do domínio de pagamentos.
/// 
/// O que são testes de unidade?
/// São testes que verificam uma única "unidade" de código — 
/// no nosso caso, as regras de negócio da entidade Payment e do VO Money.
/// 
/// Características:
/// - SEM banco de dados
/// - SEM HTTP
/// - SEM dependências externas
/// - Rápidos (milissegundos)
/// - Testam APENAS a lógica de negócio
/// </summary>
public class PaymentDomainTests
{
	// ==================================================
	// HELPER — método auxiliar para criar um pagamento
	// pronto para usar nos testes sem repetir código
	// ==================================================

	/// <summary>
	/// Cria um pagamento no estado Pending para usar nos testes.
	/// Usa Guid.NewGuid() para garantir que cada teste tenha dados únicos.
	/// </summary>
	private static Payment CriarPagamentoPendente() =>
		Payment.Create(
			enrollmentId: Guid.NewGuid(),       // id único de matrícula
			studentId: Guid.NewGuid().ToString(), // id único de estudante
			amount: new Money(299.90m, "BRL"),   // valor válido
			method: PaymentMethod.Pix,           // método de pagamento
			idempotencyKey: Guid.NewGuid().ToString()); // chave única

	// ==================================================
	// TESTES DO VALUE OBJECT: Money
	// 
	// Money representa um valor monetário.
	// Testamos se as regras de validação do construtor funcionam.
	// ==================================================

	/// <summary>
	/// Garante que não é possível criar um Money com valor zero.
	/// Regra: valor deve ser maior que zero.
	/// 
	/// "act" é uma função que executamos e esperamos que lance exceção.
	/// "Should().Throw" verifica se a exceção foi lançada.
	/// "WithMessage" verifica se a mensagem contém o texto esperado (* = qualquer coisa).
	/// </summary>
	[Fact(DisplayName = "Money: valor zero deve lançar exceção")]
	public void Money_ValorZero_DeveLancarExcecao()
	{
		// Tenta criar Money com valor 0
		var act = () => new Money(0, "BRL");

		// Verifica que lançou ArgumentException com a mensagem certa
		act.Should().Throw<ArgumentException>()
		   .WithMessage("*maior que zero*");
	}

	/// <summary>
	/// Garante que não é possível criar um Money com valor negativo.
	/// </summary>
	[Fact(DisplayName = "Money: valor negativo deve lançar exceção")]
	public void Money_ValorNegativo_DeveLancarExcecao()
	{
		var act = () => new Money(-10, "BRL");

		act.Should().Throw<ArgumentException>()
		   .WithMessage("*maior que zero*");
	}

	/// <summary>
	/// Garante que a moeda não pode ser vazia.
	/// </summary>
	[Fact(DisplayName = "Money: moeda vazia deve lançar exceção")]
	public void Money_MoedaVazia_DeveLancarExcecao()
	{
		var act = () => new Money(100, "");

		act.Should().Throw<ArgumentException>()
		   .WithMessage("*Moeda*");
	}

	/// <summary>
	/// Garante que a moeda é sempre armazenada em maiúsculo.
	/// "brl" deve virar "BRL".
	/// 
	/// "money.Currency.Should().Be("BRL")" verifica se o valor é exatamente "BRL".
	/// </summary>
	[Fact(DisplayName = "Money: moeda deve ser armazenada em maiúsculo")]
	public void Money_MoedaMinuscula_DeveArmazenarMaiusculo()
	{
		var money = new Money(100, "brl"); // passa minúsculo

		money.Currency.Should().Be("BRL"); // espera maiúsculo
	}

	/// <summary>
	/// Garante que dois Money com mesmo valor e moeda são iguais.
	/// Isso funciona porque Money é um "record" — compara por valor, não por referência.
	/// 
	/// Em classes normais: new Money(100) != new Money(100) (objetos diferentes na memória)
	/// Em records:         new Money(100) == new Money(100) (compara os dados)
	/// </summary>
	[Fact(DisplayName = "Money: dois valores iguais devem ser iguais (record)")]
	public void Money_MesmosValores_DevemSerIguais()
	{
		var m1 = new Money(100, "BRL");
		var m2 = new Money(100, "BRL");

		m1.Should().Be(m2); // devem ser iguais
	}

	// ==================================================
	// TESTES DO FACTORY METHOD: Payment.Create
	//
	// Payment.Create é o único ponto de criação de pagamentos.
	// Testamos se o pagamento nasce com o estado correto
	// e se as validações de entrada funcionam.
	// ==================================================

	/// <summary>
	/// Verifica se um pagamento recém-criado tem todos os campos corretos.
	/// 
	/// Todo pagamento deve nascer:
	/// - Status: Pending (aguardando processamento)
	/// - Id: preenchido (não pode ser vazio)
	/// - TransactionId: null (ainda não confirmado)
	/// - PaidAt: null (ainda não pago)
	/// - FailureReason: null (não houve falha)
	/// - IsDeleted: false (não deletado)
	/// </summary>
	[Fact(DisplayName = "Create: deve criar pagamento no estado Pending")]
	public void Create_DeveCriarPagamentoPending()
	{
		var payment = CriarPagamentoPendente();

		payment.Status.Should().Be(PaymentStatus.Pending);      // estado inicial
		payment.Id.Should().NotBe(Guid.Empty);                  // id gerado
		payment.TransactionId.Should().BeNull();                 // sem transação ainda
		payment.PaidAt.Should().BeNull();                        // sem data de pagamento
		payment.FailureReason.Should().BeNull();                 // sem motivo de falha
		payment.IsDeleted.Should().BeFalse();                    // não deletado
	}

	/// <summary>
	/// Verifica que não é possível criar um pagamento sem EnrollmentId.
	/// Guid.Empty é o valor padrão de Guid (todos zeros) — considerado inválido.
	/// </summary>
	[Fact(DisplayName = "Create: EnrollmentId vazio deve lançar exceção")]
	public void Create_EnrollmentIdVazio_DeveLancarExcecao()
	{
		var act = () => Payment.Create(
			enrollmentId: Guid.Empty, // inválido
			studentId: Guid.NewGuid().ToString(),
			amount: new Money(100, "BRL"),
			method: PaymentMethod.Pix,
			idempotencyKey: "key-001");

		act.Should().Throw<ArgumentException>()
		   .WithMessage("*EnrollmentId*");
	}

	/// <summary>
	/// Verifica que não é possível criar um pagamento sem StudentId.
	/// </summary>
	[Fact(DisplayName = "Create: StudentId vazio deve lançar exceção")]
	public void Create_StudentIdVazio_DeveLancarExcecao()
	{
		var act = () => Payment.Create(
			enrollmentId: Guid.NewGuid(),
			studentId: "", // inválido
			amount: new Money(100, "BRL"),
			method: PaymentMethod.Pix,
			idempotencyKey: "key-001");

		act.Should().Throw<ArgumentException>()
		   .WithMessage("*StudentId*");
	}

	/// <summary>
	/// Verifica que não é possível criar um pagamento sem IdempotencyKey.
	/// A chave de idempotência é obrigatória para evitar duplicatas.
	/// </summary>
	[Fact(DisplayName = "Create: IdempotencyKey vazia deve lançar exceção")]
	public void Create_IdempotencyKeyVazia_DeveLancarExcecao()
	{
		var act = () => Payment.Create(
			enrollmentId: Guid.NewGuid(),
			studentId: Guid.NewGuid().ToString(),
			amount: new Money(100, "BRL"),
			method: PaymentMethod.Pix,
			idempotencyKey: ""); // inválido

		act.Should().Throw<ArgumentException>()
		   .WithMessage("*IdempotencyKey*");
	}

	// ==================================================
	// TESTES DO MÉTODO: Payment.Confirm
	//
	// Confirm é a transição Pending → Paid.
	// Testamos se a transição funciona e se transições inválidas são bloqueadas.
	// ==================================================

	/// <summary>
	/// Verifica a transição Pending → Paid.
	/// Após confirmar, o pagamento deve ter:
	/// - Status: Paid
	/// - TransactionId: preenchido (vem do gateway)
	/// - PaidAt: preenchido com a data/hora da confirmação
	/// - UpdatedAt: preenchido
	/// </summary>
	[Fact(DisplayName = "Confirm: deve mudar status para Paid")]
	public void Confirm_DeveAlterarStatusParaPaid()
	{
		var payment = CriarPagamentoPendente();

		payment.Confirm("TXN-001"); // confirma com id da transação

		payment.Status.Should().Be(PaymentStatus.Paid);
		payment.TransactionId.Should().Be("TXN-001");
		payment.PaidAt.Should().NotBeNull();
		payment.UpdatedAt.Should().NotBeNull();
	}

	/// <summary>
	/// Verifica que não é possível confirmar um pagamento já confirmado.
	/// Fluxo inválido: Paid → Paid (não existe).
	/// </summary>
	[Fact(DisplayName = "Confirm: pagamento já Paid deve lançar exceção")]
	public void Confirm_PagamentoJaPago_DeveLancarExcecao()
	{
		var payment = CriarPagamentoPendente();
		payment.Confirm("TXN-001"); // primeiro confirm — OK

		var act = () => payment.Confirm("TXN-002"); // segundo confirm — INVÁLIDO

		act.Should().Throw<InvalidOperationException>()
		   .WithMessage("*pendentes*");
	}

	/// <summary>
	/// Verifica que não é possível confirmar um pagamento que falhou.
	/// Fluxo inválido: Failed → Paid (não existe).
	/// </summary>
	[Fact(DisplayName = "Confirm: pagamento Failed deve lançar exceção")]
	public void Confirm_PagamentoFailed_DeveLancarExcecao()
	{
		var payment = CriarPagamentoPendente();
		payment.Fail("Cartão recusado"); // marca como falho

		var act = () => payment.Confirm("TXN-001"); // tenta confirmar — INVÁLIDO

		act.Should().Throw<InvalidOperationException>()
		   .WithMessage("*pendentes*");
	}

	// ==================================================
	// TESTES DO MÉTODO: Payment.Fail
	//
	// Fail é a transição Pending → Failed.
	// ==================================================

	/// <summary>
	/// Verifica a transição Pending → Failed.
	/// Após falhar, o pagamento deve ter:
	/// - Status: Failed
	/// - FailureReason: o motivo da falha
	/// - UpdatedAt: preenchido
	/// </summary>
	[Fact(DisplayName = "Fail: deve mudar status para Failed")]
	public void Fail_DeveAlterarStatusParaFailed()
	{
		var payment = CriarPagamentoPendente();

		payment.Fail("Cartão recusado");

		payment.Status.Should().Be(PaymentStatus.Failed);
		payment.FailureReason.Should().Be("Cartão recusado");
		payment.UpdatedAt.Should().NotBeNull();
	}

	/// <summary>
	/// Verifica que não é possível marcar como falho um pagamento já confirmado.
	/// Fluxo inválido: Paid → Failed (não existe).
	/// </summary>
	[Fact(DisplayName = "Fail: pagamento já Paid deve lançar exceção")]
	public void Fail_PagamentoJaPago_DeveLancarExcecao()
	{
		var payment = CriarPagamentoPendente();
		payment.Confirm("TXN-001"); // confirma primeiro

		var act = () => payment.Fail("Erro"); // tenta falhar — INVÁLIDO

		act.Should().Throw<InvalidOperationException>()
		   .WithMessage("*pendentes*");
	}

	// ==================================================
	// TESTES DO MÉTODO: Payment.Refund
	//
	// Refund é a transição Paid → Refunded.
	// Só pagamentos confirmados podem ser estornados.
	// ==================================================

	/// <summary>
	/// Verifica a transição Paid → Refunded.
	/// Após estornar, o pagamento deve ter:
	/// - Status: Refunded
	/// - UpdatedAt: preenchido
	/// </summary>
	[Fact(DisplayName = "Refund: deve mudar status para Refunded")]
	public void Refund_DeveAlterarStatusParaRefunded()
	{
		var payment = CriarPagamentoPendente();
		payment.Confirm("TXN-001"); // precisa estar Paid para estornar

		payment.Refund();

		payment.Status.Should().Be(PaymentStatus.Refunded);
		payment.UpdatedAt.Should().NotBeNull();
	}

	/// <summary>
	/// Verifica que não é possível estornar um pagamento Pending.
	/// Fluxo inválido: Pending → Refunded (não existe).
	/// Só pode estornar o que foi pago.
	/// </summary>
	[Fact(DisplayName = "Refund: pagamento Pending deve lançar exceção")]
	public void Refund_PagamentoPending_DeveLancarExcecao()
	{
		var payment = CriarPagamentoPendente(); // ainda Pending

		var act = () => payment.Refund(); // tenta estornar — INVÁLIDO

		act.Should().Throw<InvalidOperationException>()
		   .WithMessage("*confirmados*");
	}

	/// <summary>
	/// Verifica que não é possível estornar um pagamento que falhou.
	/// Fluxo inválido: Failed → Refunded (não existe).
	/// </summary>
	[Fact(DisplayName = "Refund: pagamento Failed deve lançar exceção")]
	public void Refund_PagamentoFailed_DeveLancarExcecao()
	{
		var payment = CriarPagamentoPendente();
		payment.Fail("Erro"); // marca como falho

		var act = () => payment.Refund(); // tenta estornar — INVÁLIDO

		act.Should().Throw<InvalidOperationException>()
		   .WithMessage("*confirmados*");
	}
}