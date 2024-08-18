using Space.CrewManagement.Services.Dtos;

namespace Space.CrewManagement.Services.Interfaces;
public interface IMemberTypeService
{
    Task<List<MemberTypeDto>> GetAll();
}
