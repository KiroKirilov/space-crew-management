using Microsoft.EntityFrameworkCore;
using Space.CrewManagement.Data.Models;
using Space.CrewManagement.Data.Repositories;
using Space.CrewManagement.Services.Dtos;
using Space.CrewManagement.Services.Interfaces;

namespace Space.CrewManagement.Services.Services;
public class MemberTypeService(IRepository<MemberType> _memberTypes) : IMemberTypeService
{
    public async Task<List<MemberTypeDto>> GetAll()
    {
        return await _memberTypes.AllAsNoTracking()
            .Select(mt => new MemberTypeDto(mt.Id, mt.Name, mt.Type))
            .ToListAsync();
    }
}
