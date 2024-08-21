namespace Space.CrewManagement.Services.Exceptions;
public class EntityNotFoundException(string entityName) : Exception
{
    public string EntityName { get; private set; } = entityName;

    public object ResponseObject => new { EntityName };
}
