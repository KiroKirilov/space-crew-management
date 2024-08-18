using Space.CrewManagement.Data.Models;

namespace Space.CrewManagement.Services.Dtos;
public record CrewMemberResponseDto(
    Guid Id,
    string Name,
    string Email,
    string CountryCode,
    string ProfileImageUrl,
    string Birthday,
    string LastCertificationDate,
    CrewMemberStatus Status,
    string StatusDescription,
    LicenseDto License,
    MemberTypeDto MemberType
) : CrewMemberBaseDto(Name, Email, CountryCode, ProfileImageUrl);