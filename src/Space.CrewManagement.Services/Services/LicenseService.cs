using Microsoft.EntityFrameworkCore;
using Space.CrewManagement.Data.Models;
using Space.CrewManagement.Data.Repositories;
using Space.CrewManagement.Services.Dtos;
using Space.CrewManagement.Services.Interfaces;

namespace Space.CrewManagement.Services.Services;
public class LicenseService(IRepository<License> _licenses) : ILicenseService
{
    public async Task<List<LicenseDto>> GetAll()
    {
        return await _licenses.AllAsNoTracking()
            .Select(l => new LicenseDto(l.Id, l.Name, l.Description))
            .ToListAsync();
    }
}
