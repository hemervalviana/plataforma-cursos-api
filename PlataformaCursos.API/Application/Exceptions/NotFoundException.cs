namespace PlataformaCursos.API.Application.Exceptions;

public class NotFoundException : DomainException
{
	public NotFoundException(string message)
		: base(message, StatusCodes.Status404NotFound)
	{
	}
}
