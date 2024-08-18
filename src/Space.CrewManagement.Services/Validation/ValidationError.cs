namespace Space.CrewManagement.Services.Validation;
public class ValidationError(string fieldName, string validationMessage)
{
    public string FieldName { get; private set; } = fieldName;

    public string ValidationMessage { get; private set; } = validationMessage;
}
