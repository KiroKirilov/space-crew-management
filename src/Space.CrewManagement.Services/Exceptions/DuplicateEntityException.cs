namespace Space.CrewManagement.Services.Exceptions;
public class DuplicateEntityException(string duplicateEntity) : Exception
{
    public string DuplicateEntity { get; private set; } = duplicateEntity;

    public object ResponseObject => new { DuplicateEntity };
}
