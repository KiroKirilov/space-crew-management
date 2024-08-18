namespace Space.CrewManagement.Services.Dtos;
public record CreateCrewMemberDto(
    string Name,
    string Email,
    string CountryCode,
    string ProfileImageUrl,
    string Birthday,
    string LastCertificationDate,
    Guid LicenseId,
    Guid MemberTypeId) 
    : CrewMemberBaseDto(Name, Email, CountryCode, ProfileImageUrl);