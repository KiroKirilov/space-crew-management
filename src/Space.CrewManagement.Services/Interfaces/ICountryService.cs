using Space.CrewManagement.Services.Dtos;

namespace Space.CrewManagement.Services.Interfaces;
public interface ICountryService
{
    HashSet<CountryDto> GetAll();
    HashSet<string> GetAllCodes();
}
