namespace Space.CrewManagement.Services.Dtos;
public record CrewMemberListDto(
    int TotalCount,
    int TotalPages,
    List<CrewMemberResponseDto> CrewMembers
);