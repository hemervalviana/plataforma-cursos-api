namespace PlataformaCursos.API.Application.Exceptions;

public class BusinessRuleException : DomainException
{
	public BusinessRuleException(string message)
		: base(message, StatusCodes.Status422UnprocessableEntity)
	{
	}
}
