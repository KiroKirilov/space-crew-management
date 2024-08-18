namespace Space.CrewManagement.Services.Interfaces;
public interface IDateProvider
{
    DateOnly Now { get; }
}
