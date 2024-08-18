using Space.CrewManagement.Services.Dtos;

namespace Space.CrewManagement.Services.Interfaces;
public interface ILicenseService
{
    Task<List<LicenseDto>> GetAll();
}
