using Space.CrewManagement.Services.Validation;

namespace Space.CrewManagement.Services.Exceptions;
public class ValidationException(ICollection<ValidationError> validationErrors) : Exception
{
    public ICollection<ValidationError> ValidationErrors { get; set; } = validationErrors;
}
