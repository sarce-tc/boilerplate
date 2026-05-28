namespace Microservice.Application.Exceptions;
public sealed class NotFoundException : ApplicationException
{
    public NotFoundException(string name, object key) : base($"Entity \"{name}\" ({key}) no fue encontrada")
    {
    }
}
