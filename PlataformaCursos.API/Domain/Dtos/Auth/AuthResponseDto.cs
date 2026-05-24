namespace PlataformaCursos.API.Domain.Dtos.Auth
{
	/// <summary>
	/// DTO retornado ao cliente após login bem-sucedido.
	/// </summary>
	public class AuthResponseDto
	{
		public string Token { get; set; } = null!;
		public DateTime Expiration { get; set; }
	}
}
