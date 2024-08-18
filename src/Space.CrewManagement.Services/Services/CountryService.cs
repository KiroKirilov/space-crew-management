using ISO3166;
using Space.CrewManagement.Services.Dtos;
using Space.CrewManagement.Services.Interfaces;

namespace Space.CrewManagement.Services.Services;
public class CountryService : ICountryService
{
    public HashSet<CountryDto> GetAll()
    {
        return Country.List
            .Select(c => new CountryDto(c.ThreeLetterCode,c.Name))
            .ToHashSet();
    }

    public HashSet<string> GetAllCodes()
    {
        return Country.List
            .Select(c => c.ThreeLetterCode)
            .ToHashSet();
    }
}
