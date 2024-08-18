namespace Space.CrewManagement.Services.Validation;
public class EntityNotFoundException(string entityName) : Exception
{
    public string EntityName { get; private set; } = entityName;

    public object ResponseObject => new { EntityName };
}
