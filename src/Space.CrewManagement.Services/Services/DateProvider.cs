using Space.CrewManagement.Services.Interfaces;

namespace Space.CrewManagement.Services.Services;
public class DateProvider : IDateProvider
{
    public DateOnly Now => DateOnly.FromDateTime(DateTime.Now);
}
