using Space.CrewManagement.Services.Dtos;

namespace Space.CrewManagement.Services.Interfaces;
public interface ICrewMemberService
{
    Task Create(CreateCrewMemberDto dto);
    Task Delete(Guid id);
    Task<CrewMemberResponseDto> GetById(Guid id);
    Task<CrewMemberListDto> GetAll(int? page, int? pageSize);
    Task Update(Guid id, UpdateCrewMemberDto dto);
    Task RenewLicense(Guid id, DateOnly newCertificationDate);
}
