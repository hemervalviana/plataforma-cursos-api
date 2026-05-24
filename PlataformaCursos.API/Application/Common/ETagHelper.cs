using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace PlataformaCursos.API.Application.Common;

public static class ETagHelper
{
	public static string Generate(object data)
	{
		var json = JsonSerializer.Serialize(data);

		using var sha = SHA256.Create();

		var hash = sha.ComputeHash(
			Encoding.UTF8.GetBytes(json));

		return Convert.ToBase64String(hash);
	}
}
