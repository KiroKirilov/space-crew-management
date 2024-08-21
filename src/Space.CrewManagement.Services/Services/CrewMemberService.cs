using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Space.CrewManagement.Data.Models;
using Space.CrewManagement.Data.Repositories;
using Space.CrewManagement.Services.Dtos;
using Space.CrewManagement.Services.Exceptions;
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

    public async Task Update(Guid id, UpdateCrewMemberDto dto)
    {
        _validator.ThrowIfInvalid(dto);

        var crewMember = await _crewRepo.FindAsync(id)
            ?? throw new EntityNotFoundException("CrewMember");

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

        var emailAlreadyExists = await _crewRepo.AllAsNoTracking().AnyAsync(x => x.Email == dto.Email && x.Id != id);

        if (emailAlreadyExists)
        {
            throw new DuplicateEntityException("Crew.Email");
        }

        crewMember.Name = dto.Name;
        crewMember.Email = dto.Email;
        crewMember.CountryCode = dto.CountryCode;
        crewMember.ProfileImageUrl = dto.ProfileImageUrl;
        crewMember.LicenseId = dto.LicenseId;
        crewMember.MemberTypeId = dto.MemberTypeId;

        _crewRepo.Update(crewMember);
        await _crewRepo.SaveAsync();
    }

    public async Task RenewLicense(Guid id, DateOnly newCertificationDate)
    {
        var crewMember = await _crewRepo.FindAsync(id)
            ?? throw new EntityNotFoundException("CrewMember");

        if (crewMember.Status == CrewMemberStatus.CrewRetired)
        {
            throw new ValidationException([new ValidationError("Status", "Crew member is retired")]);
        }

        var newStatus = await GetCrewStatus(crewMember.Birthday, newCertificationDate);

        crewMember.LastCertificationDate = newCertificationDate;
        crewMember.Status = newStatus.Status;
        crewMember.StatusDescription = newStatus.StatusDescription;

        _crewRepo.Update(crewMember);
        await _crewRepo.SaveAsync();
    }

    public async Task<CrewMemberListDto> GetAll(int? page, int? pageSize)
    {
        var query = _crewRepo
            .AllAsNoTracking()
            .Select(x => new CrewMemberResponseDto(
                x.Id,
                x.Name,
                x.Email,
                x.CountryCode,
                x.ProfileImageUrl,
                x.Birthday.ToString("yyyy-MM-dd"),
                x.LastCertificationDate.ToString("yyyy-MM-dd"),
                x.Status,
                x.StatusDescription,
                new LicenseDto(x.LicenseId, x.License.Name, x.License.Description),
                new MemberTypeDto(x.MemberTypeId, x.MemberType.Name, x.MemberType.Type)));

        var totalCount = await query.CountAsync();

        if (page.HasValue && pageSize.HasValue && page > 0 && pageSize > 0)
        {
            query = query.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value);
        }

        var crewMembers = await query.ToListAsync();
        var totalPages = (int)Math.Ceiling((double)totalCount / (pageSize ?? totalCount));

        return new CrewMemberListDto(totalCount, totalPages, crewMembers);
    }

    public async Task<CrewMemberResponseDto> GetById(Guid id)
    {
        var crewMember = await _crewRepo
            .AllAsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new CrewMemberResponseDto(
                x.Id,
                x.Name,
                x.Email,
                x.CountryCode,
                x.ProfileImageUrl,
                x.Birthday.ToString("yyyy-MM-dd"),
                x.LastCertificationDate.ToString("yyyy-MM-dd"),
                x.Status,
                x.StatusDescription,
                new LicenseDto(x.LicenseId, x.License.Name, x.License.Description),
                new MemberTypeDto(x.MemberTypeId, x.MemberType.Name, x.MemberType.Type)))
            .FirstOrDefaultAsync();

        return crewMember is null 
            ? throw new EntityNotFoundException("CrewMember") 
            : crewMember;
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
        try
        {
            var formattedBirthday = birthday.ToString("yyyy-MM-dd");
            var formattedCertificationDate = lastCertificationDate.ToString("yyyy-MM-dd");
            var response = await _httpClient.GetAsync($"/api/crew-members/status?certificationDate={formattedCertificationDate}&birthday={formattedBirthday}");

            if (!response.IsSuccessStatusCode)
            {
                throw new ExternalServiceException($"External service returned bad status code: {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync();
            var statusDto = JsonConvert.DeserializeObject<CrewMemberStatusDto>(content);

            return statusDto is null
                ? throw new ExternalServiceException("External service returned an invalid response body")
                : statusDto;
        }
        catch (HttpRequestException e)
        {
            throw new ExternalServiceException($"Network error occurred: {e.Message}", e);
        }
    }
}
