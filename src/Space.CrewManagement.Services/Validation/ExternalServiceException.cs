namespace Space.CrewManagement.Services.Validation;
public class ExternalServiceException: Exception
{
    public ExternalServiceException(string? message)
        : base(message)
    {

    }

    public ExternalServiceException(string? message, Exception? innerException)
        : base(message, innerException)
    {

    }
}
