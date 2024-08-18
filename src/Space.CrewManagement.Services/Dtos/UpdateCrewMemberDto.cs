namespace Space.CrewManagement.Services.Dtos;
public record UpdateCrewMemberDto(
    Guid LicenseId,
    Guid MemberTypeId,
    string Name,
    string Email,
    string CountryCode,
    string ProfileImageUrl
) : CrewMemberBaseDto(Name, Email, CountryCode, ProfileImageUrl);