namespace Space.CrewManagement.Services.Dtos;
public record CrewMemberBaseDto(
    string Name,
    string Email,
    string CountryCode,
    string ProfileImageUrl);
