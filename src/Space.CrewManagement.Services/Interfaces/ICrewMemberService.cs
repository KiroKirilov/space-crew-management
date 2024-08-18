using Space.CrewManagement.Services.Dtos;

namespace Space.CrewManagement.Services.Interfaces;
public interface ICrewMemberService
{
    Task Create(CreateCrewMemberDto dto);
    Task Delete(Guid id);
}
