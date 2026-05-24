namespace PlataformaCursos.API.Application.Common;

public class BusinessException : Exception
{
	public BusinessException(string message)
		: base(message)
	{
	}
}
