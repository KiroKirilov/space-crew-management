using Space.CrewManagement.Services.Dtos;

namespace Space.CrewManagement.Services.Interfaces;
public interface ICrewMemberValidator
{
    void ThrowIfInvalid(CreateCrewMemberDto dto);
    void ThrowIfInvalid(UpdateCrewMemberDto dto);
}
