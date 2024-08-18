namespace Space.CrewManagement.Services.Validation;
public class ValidationException(ICollection<ValidationError> validationErrors) : Exception
{
    public ICollection<ValidationError> ValidationErrors { get; set; } = validationErrors;
}
