namespace Space.CrewManagement.Services.Validation;
public class DuplicateEntityException(string duplicateEntity) : Exception
{
    public string DuplicateEntity { get; private set; } = duplicateEntity;

    public object ResponseObject => new { DuplicateEntity };
}
