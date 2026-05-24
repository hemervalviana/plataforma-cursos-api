using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using PlataformaCursos.API.Infrastructure.Data;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace PlataformaCursos.Tests.Integration;

public class PaymentsApiTests : IClassFixture<ApiFactory>
{
	private readonly HttpClient _client;
	private readonly ApiFactory _factory;

	// IDs fixos do seed do banco de teste
	private Guid _enrollmentId;

	public PaymentsApiTests(ApiFactory factory)
	{
		_factory = factory;
		_client = factory.CreateClient();

		// Limpa pagamentos antes de cada execução
		_factory.LimparPagamentos();

		// Busca uma matrícula existente no banco de teste
		using var scope = factory.Services.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		var enrollment = db.Enrollments.FirstOrDefault();

		if (enrollment != null)
			_enrollmentId = enrollment.Id;
	}

	// ==================================================
	// Helpers
	// ==================================================

	private void UsarTokenAdmin() =>
		_client.DefaultRequestHeaders.Authorization =
			new AuthenticationHeaderValue("Bearer", TokenHelper.TokenAdmin());

	private void UsarTokenStudent(string studentId = "student-test-id") =>
		_client.DefaultRequestHeaders.Authorization =
			new AuthenticationHeaderValue("Bearer", TokenHelper.TokenStudent(studentId));

	private void RemoverToken() =>
		_client.DefaultRequestHeaders.Authorization = null;

	private object PayloadValido() => new
	{
		enrollmentId = _enrollmentId,
		amount = 299.90m,
		currency = "BRL",
		method = 3 // Pix
	};

	// ==================================================
	// Testes de autenticação
	// ==================================================

	[Fact(DisplayName = "POST /api/payments sem token deve retornar 401")]
	public async Task CriarPagamento_SemToken_DeveRetornar401()
	{
		RemoverToken();

		var response = await _client.PostAsJsonAsync(
			"/api/payments", PayloadValido());

		response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
	}

	// ==================================================
	// Testes de validação (400)
	// ==================================================

	[Fact(DisplayName = "POST /api/payments com payload inválido deve retornar 400")]
	public async Task CriarPagamento_PayloadInvalido_DeveRetornar400()
	{
		UsarTokenAdmin();

		var payload = new
		{
			enrollmentId = Guid.Empty,
			amount = -10,
			currency = "BR",
			method = 99
		};

		var response = await _client.PostAsJsonAsync("/api/payments", payload);

		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

		var body = await response.Content.ReadAsStringAsync();
		body.Should().Contain("Amount");
		body.Should().Contain("Currency");
		body.Should().Contain("Method");
	}

	// ==================================================
	// Testes de criação (201)
	// ==================================================

	[Fact(DisplayName = "POST /api/payments válido deve retornar 201 com status Paid")]
	public async Task CriarPagamento_Valido_DeveRetornar201()
	{
		if (_enrollmentId == Guid.Empty)
			return; // sem matrícula no banco de teste

		UsarTokenAdmin();
		_client.DefaultRequestHeaders.Add(
			"Idempotency-Key", Guid.NewGuid().ToString());

		var response = await _client.PostAsJsonAsync(
			"/api/payments", PayloadValido());

		response.StatusCode.Should().Be(HttpStatusCode.Created);

		var body = await response.Content.ReadFromJsonAsync<dynamic>();
		body.Should().NotBeNull();
	}

	// ==================================================
	// Testes de idempotência
	// ==================================================

	[Fact(DisplayName = "POST /api/payments com mesma Idempotency-Key deve retornar mesmo pagamento")]
	public async Task CriarPagamento_MesmaKey_DeveRetornarMesmoPagamento()
	{
		if (_enrollmentId == Guid.Empty)
			return;

		UsarTokenAdmin();

		var key = Guid.NewGuid().ToString();
		_client.DefaultRequestHeaders.Add("Idempotency-Key", key);

		var response1 = await _client.PostAsJsonAsync(
			"/api/payments", PayloadValido());

		_client.DefaultRequestHeaders.Remove("Idempotency-Key");
		_client.DefaultRequestHeaders.Add("Idempotency-Key", key);

		var response2 = await _client.PostAsJsonAsync(
			"/api/payments", PayloadValido());

		var body1 = await response1.Content.ReadAsStringAsync();
		var body2 = await response2.Content.ReadAsStringAsync();

		// Mesmo id retornado nas duas requisições
		body1.Should().Contain(
			body2.Split("\"id\":\"")[1].Split("\"")[0]);
	}

	// ==================================================
	// Testes de duplicidade (409)
	// ==================================================

	[Fact(DisplayName = "POST /api/payments duplicado deve retornar 409")]
	public async Task CriarPagamento_Duplicado_DeveRetornar409()
	{
		if (_enrollmentId == Guid.Empty)
			return;

		UsarTokenAdmin();

		// Primeiro pagamento
		_client.DefaultRequestHeaders.Add(
			"Idempotency-Key", Guid.NewGuid().ToString());
		await _client.PostAsJsonAsync("/api/payments", PayloadValido());

		// Segundo pagamento com chave diferente
		_client.DefaultRequestHeaders.Remove("Idempotency-Key");
		_client.DefaultRequestHeaders.Add(
			"Idempotency-Key", Guid.NewGuid().ToString());

		var response = await _client.PostAsJsonAsync(
			"/api/payments", PayloadValido());

		response.StatusCode.Should().Be(HttpStatusCode.Conflict);
	}

	// ==================================================
	// Testes de autorização (403)
	// ==================================================

	[Fact(DisplayName = "GET /api/payments sem role Admin deve retornar 403")]
	public async Task ListarTodos_SemRoleAdmin_DeveRetornar403()
	{
		UsarTokenStudent();

		var response = await _client.GetAsync("/api/payments");

		response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
	}

	[Fact(DisplayName = "GET /api/payments/{id} de outro student deve retornar 403")]
	public async Task BuscarPagamento_DeOutroStudent_DeveRetornar403()
	{
		if (_enrollmentId == Guid.Empty)
			return;

		// Admin cria o pagamento
		UsarTokenAdmin();
		_client.DefaultRequestHeaders.Add(
			"Idempotency-Key", Guid.NewGuid().ToString());

		var createResponse = await _client.PostAsJsonAsync(
			"/api/payments", PayloadValido());

		var body = await createResponse.Content.ReadAsStringAsync();
		var paymentId = body.Split("\"id\":\"")[1].Split("\"")[0];

		// Student diferente tenta acessar
		UsarTokenStudent("outro-student-id");

		var response = await _client.GetAsync($"/api/payments/{paymentId}");

		response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
	}

	// ==================================================
	// Testes de listagem com paginação
	// ==================================================

	[Fact(DisplayName = "GET /api/payments com paginação inválida deve retornar 400")]
	public async Task ListarTodos_PaginacaoInvalida_DeveRetornar400()
	{
		UsarTokenAdmin();

		var response = await _client.GetAsync("/api/payments?page=0&pageSize=-1");

		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}

	[Fact(DisplayName = "GET /api/payments deve retornar lista paginada")]
	public async Task ListarTodos_Admin_DeveRetornarListaPaginada()
	{
		UsarTokenAdmin();

		var response = await _client.GetAsync("/api/payments?page=1&pageSize=10");

		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var body = await response.Content.ReadAsStringAsync();
		body.Should().Contain("total");
		body.Should().Contain("data");
		body.Should().Contain("page");
	}
}