namespace Space.CrewManagement.Services.Interfaces;
public interface IBodyParser
{
    Task<T?> Parse<T>(Stream body) 
        where T : class;
}
