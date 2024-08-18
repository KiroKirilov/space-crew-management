using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Space.CrewManagement.Data.Models;
using Space.CrewManagement.Data.Repositories;
using Space.CrewManagement.Services.Dtos;
using Space.CrewManagement.Services.Interfaces;
using Space.CrewManagement.Services.Validation;

namespace Space.CrewManagement.Services.Services;
public class CrewMemberService(
    HttpClient _httpClient,
    ICrewMemberValidator _validator,
    IRepository<CrewMember> _crewRepo,
    IRepository<License> _licenseRepo,
    IRepository<MemberType> _memberTypeRepo) : ICrewMemberService
{
    public async Task Create(CreateCrewMemberDto dto)
    {
        _validator.ThrowIfInvalid(dto);

        var licenseExists = await _licenseRepo.AllAsNoTracking().AnyAsync(x => x.Id == dto.LicenseId);
        if (!licenseExists)
        {
            throw new EntityNotFoundException("License");
        }

        var memberTypeExists = await _memberTypeRepo.AllAsNoTracking().AnyAsync(x => x.Id == dto.MemberTypeId);
        if (!memberTypeExists)
        {
            throw new EntityNotFoundException("MemberType");
        }

        var emailAlreadyExists = await _crewRepo.AllAsNoTracking().AnyAsync(x => x.Email == dto.Email);

        if (emailAlreadyExists)
        {
            throw new DuplicateEntityException("Crew.Email");
        }

        var lastCertificationDate = DateOnly.ParseExact(dto.LastCertificationDate, "yyyy-MM-dd");
        var birthday = DateOnly.ParseExact(dto.Birthday, "yyyy-MM-dd");
        var crewStatus = await GetCrewStatus(birthday, lastCertificationDate);

        var crewMember = new CrewMember
        {
            Name = dto.Name,
            Birthday = birthday,
            Email = dto.Email,
            CountryCode = dto.CountryCode,
            ProfileImageUrl = dto.ProfileImageUrl,
            LastCertificationDate = lastCertificationDate,
            Status = crewStatus.Status,
            StatusDescription = crewStatus.StatusDescription,
            LicenseId = dto.LicenseId,
            MemberTypeId = dto.MemberTypeId
        };

        _crewRepo.Add(crewMember);
        await _crewRepo.SaveAsync();
    }

    public async Task Delete(Guid id)
    {
        var crewMember = await _crewRepo.FindAsync(id) 
            ?? throw new EntityNotFoundException("CrewMember");
        
        _crewRepo.Delete(crewMember);
        await _crewRepo.SaveAsync();
    }

    private async Task<CrewMemberStatusDto> GetCrewStatus(DateOnly birthday, DateOnly lastCertificationDate)
    {
        var formattedBirthday = birthday.ToString("yyyy-MM-dd");
        var formattedCertificationDate = lastCertificationDate.ToString("yyyy-MM-dd");
        var response = await _httpClient.GetAsync($"/api/crew-members/status?certificationDate={formattedCertificationDate}&birthday={formattedBirthday}");

        if (!response.IsSuccessStatusCode)
        {
            throw new ExternalServiceException($"Returned bad status code: {response.StatusCode}");
        }

        var content = await response.Content.ReadAsStringAsync();
        var statusDto = JsonConvert.DeserializeObject<CrewMemberStatusDto>(content);

        return statusDto is null 
            ? throw new ExternalServiceException("Returned invalid response") 
            : statusDto;
    }
}
