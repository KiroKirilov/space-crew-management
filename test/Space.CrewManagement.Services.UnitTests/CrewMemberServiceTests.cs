using FluentAssertions;
using MockQueryable.NSubstitute;
using Newtonsoft.Json;
using NSubstitute;
using RichardSzalay.MockHttp;
using Space.CrewManagement.Data.Models;
using Space.CrewManagement.Data.Repositories;
using Space.CrewManagement.Services.Dtos;
using Space.CrewManagement.Services.Interfaces;
using Space.CrewManagement.Services.Services;
using Space.CrewManagement.Services.Validation;
using System.Net;
using Xunit;

namespace Space.CrewManagement.Services.UnitTests;
public class CrewMemberServiceTests
{
    private readonly ICrewMemberValidator _validator;
    private readonly IRepository<CrewMember> _crewRepo;
    private readonly IRepository<License> _licenseRepo;
    private readonly IRepository<MemberType> _memberTypeRepo;

    public CrewMemberServiceTests()
    {
        _validator = Substitute.For<ICrewMemberValidator>();
        _crewRepo = Substitute.For<IRepository<CrewMember>>();
        _licenseRepo = Substitute.For<IRepository<License>>();
        _memberTypeRepo = Substitute.For<IRepository<MemberType>>();
    }

    [Fact]
    public async Task GetAll_WhenCalledWithNoPaging_ReturnsAllCrewMembers()
    {
        var sut = new CrewMemberService(null!, _validator, _crewRepo, _licenseRepo, _memberTypeRepo);

        _crewRepo.AllAsNoTracking().Returns(GetCrewMembers().BuildMock());

        var result = await sut.GetAll(null, null);

        result.CrewMembers.Should().HaveCount(7);
        result.TotalCount.Should().Be(7);
        result.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task GetAll_WhenCalledWithPaging_ReturnsPagedCrewMembers()
    {
        var sut = new CrewMemberService(null!, _validator, _crewRepo, _licenseRepo, _memberTypeRepo);

        _crewRepo.AllAsNoTracking().Returns(GetCrewMembers().BuildMock());

        var result = await sut.GetAll(2, 2);

        result.CrewMembers.Should().HaveCount(2);
        result.TotalCount.Should().Be(7);
        result.TotalPages.Should().Be(4);

        result = await sut.GetAll(4, 2);

        result.CrewMembers.Should().HaveCount(1);
        result.TotalCount.Should().Be(7);
        result.TotalPages.Should().Be(4);

        result = await sut.GetAll(10, 2);

        result.CrewMembers.Should().HaveCount(0);
        result.TotalCount.Should().Be(7);
        result.TotalPages.Should().Be(4);
    }

    [Fact]
    public async Task GetById_WhenCalledWithValidId_ReturnsCorrectCrewMember()
    {
        var sut = new CrewMemberService(null!, _validator, _crewRepo, _licenseRepo, _memberTypeRepo);

        _crewRepo.AllAsNoTracking().Returns(GetCrewMembers().BuildMock());

        var result = await sut.GetById(Guid.Parse("00000000-0000-0000-0000-000000000001"));

        result.Name.Should().Be("John Doe");
    }

    [Fact]
    public async Task GetById_WhenCalledWithInvalidId_ThrowsNotFoundException()
    {
        var sut = new CrewMemberService(null!, _validator, _crewRepo, _licenseRepo, _memberTypeRepo);

        _crewRepo.AllAsNoTracking().Returns(GetCrewMembers().BuildMock());

        var act = async () => await sut.GetById(Guid.Parse("00000000-0000-0000-0000-000000000999"));

        await act.Should().ThrowExactlyAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task Create_WhenCalledWithValidIdAndData_UpdatesCorrectCrewMember()
    {
        var mockHttp = new MockHttpMessageHandler();
        var statusResponse = new CrewMemberStatusDto(CrewMemberStatus.Ok, "All good");
        mockHttp.When("/api/crew-members/status")
            .Respond("application/json", JsonConvert.SerializeObject(statusResponse));

        var client = mockHttp.ToHttpClient();
        client.BaseAddress = new Uri("http://localhost");
        var sut = new CrewMemberService(client, _validator, _crewRepo, _licenseRepo, _memberTypeRepo);
        var dto = new CreateCrewMemberDto(
            "John Doe2",
            "john2@doe.com",
            "BGR",
            "image",
            "2000-01-01",
            "2099-01-01",
            Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Guid.Parse("00000000-0000-0000-0000-000000000001"));

        _licenseRepo.AllAsNoTracking().Returns(GetLicenses().BuildMock());
        _memberTypeRepo.AllAsNoTracking().Returns(GetMemberTypes().BuildMock());
        _crewRepo.AllAsNoTracking().Returns(GetCrewMembers().BuildMock());

        await sut.Create(dto);

        _crewRepo.Received(1).Add(Arg.Is<CrewMember>(c =>
            c.Name == dto.Name &&
            c.Email == dto.Email &&
            c.CountryCode == dto.CountryCode &&
            c.ProfileImageUrl == dto.ProfileImageUrl &&
            c.LicenseId == dto.LicenseId &&
            c.MemberTypeId == dto.MemberTypeId &&
            c.Status == statusResponse.Status &&
            c.StatusDescription == statusResponse.StatusDescription));
        await _crewRepo.Received(1).SaveAsync();
    }

    [Fact]
    public async Task Create_WhenExternalServiceReturnsInvalidBody_ThrowsCorrectError()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp
            .When("/api/crew-members/status")
            .Respond("application/json", "");

        var client = mockHttp.ToHttpClient();
        client.BaseAddress = new Uri("http://localhost");
        var sut = new CrewMemberService(client, _validator, _crewRepo, _licenseRepo, _memberTypeRepo);
        var dto = new CreateCrewMemberDto(
            "John Doe2",
            "john2@doe.com",
            "BGR",
            "image",
            "2000-01-01",
            "2099-01-01",
            Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Guid.Parse("00000000-0000-0000-0000-000000000001"));

        _licenseRepo.AllAsNoTracking().Returns(GetLicenses().BuildMock());
        _memberTypeRepo.AllAsNoTracking().Returns(GetMemberTypes().BuildMock());
        _crewRepo.AllAsNoTracking().Returns(GetCrewMembers().BuildMock());

        var act = async () => await sut.Create(dto);

        await act.Should()
            .ThrowExactlyAsync<ExternalServiceException>()
            .WithMessage("External service returned an invalid response body");
        _crewRepo.DidNotReceive().Add(Arg.Any<CrewMember>());
        await _crewRepo.DidNotReceive().SaveAsync();
    }

    [Fact]
    public async Task Create_WhenExternalServiceReturnsErrorStatusCode_ThrowsCorrectError()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp
            .When("/api/crew-members/status")
            .Respond(HttpStatusCode.BadRequest);

        var client = mockHttp.ToHttpClient();
        client.BaseAddress = new Uri("http://localhost");
        var sut = new CrewMemberService(client, _validator, _crewRepo, _licenseRepo, _memberTypeRepo);
        var dto = new CreateCrewMemberDto(
            "John Doe2",
            "john2@doe.com",
            "BGR",
            "image",
            "2000-01-01",
            "2099-01-01",
            Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Guid.Parse("00000000-0000-0000-0000-000000000001"));

        _licenseRepo.AllAsNoTracking().Returns(GetLicenses().BuildMock());
        _memberTypeRepo.AllAsNoTracking().Returns(GetMemberTypes().BuildMock());
        _crewRepo.AllAsNoTracking().Returns(GetCrewMembers().BuildMock());

        var act = async () => await sut.Create(dto);

        await act.Should()
            .ThrowExactlyAsync<ExternalServiceException>()
            .WithMessage("External service returned bad status code: BadRequest");
        _crewRepo.DidNotReceive().Add(Arg.Any<CrewMember>());
        await _crewRepo.DidNotReceive().SaveAsync();
    }

    [Fact]
    public async Task Create_WhenValidationDoesntPass_ThrowsCorrectError()
    {
        var sut = new CrewMemberService(null!, _validator, _crewRepo, _licenseRepo, _memberTypeRepo);
        var dto = new CreateCrewMemberDto(
            "John Doe2",
            "john2@doe.com",
            "BGR",
            "image",
            "2000-01-01",
            "2099-01-01",
            Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Guid.Parse("00000000-0000-0000-0000-000000000001"));

        _validator.ThrowIfInvalid(Arg.Any<CreateCrewMemberDto>());

        _validator.When(x => x.ThrowIfInvalid(Arg.Any<CreateCrewMemberDto>()))
            .Do(x => throw new ValidationException([new ValidationError("Arg", "Msg")]));

        var act = async () => await sut.Create(dto);

        await act.Should().ThrowExactlyAsync<ValidationException>();
        _crewRepo.DidNotReceive().Add(Arg.Any<CrewMember>());
        await _crewRepo.DidNotReceive().SaveAsync();
    }

    [Fact]
    public async Task Create_GivenInvalidLicenseId_ThrowsCorrectError()
    {
        var sut = new CrewMemberService(null!, _validator, _crewRepo, _licenseRepo, _memberTypeRepo);
        var dto = new CreateCrewMemberDto(
            "John Doe2",
            "john2@doe.com",
            "BGR",
            "image",
            "2000-01-01",
            "2099-01-01",
            Guid.Parse("00000000-0000-0000-0000-000000000999"),
            Guid.Parse("00000000-0000-0000-0000-000000000001"));

        _licenseRepo.AllAsNoTracking().Returns(GetLicenses().BuildMock());
        _memberTypeRepo.AllAsNoTracking().Returns(GetMemberTypes().BuildMock());
        _crewRepo.AllAsNoTracking().Returns(GetCrewMembers().BuildMock());

        var act = async () => await sut.Create(dto);

        await act.Should().ThrowExactlyAsync<EntityNotFoundException>();
        _crewRepo.DidNotReceive().Add(Arg.Any<CrewMember>());
        await _crewRepo.DidNotReceive().SaveAsync();
    }

    [Fact]
    public async Task Create_GivenInvalidMemberTypeId_ThrowsCorrectError()
    {
        var sut = new CrewMemberService(null!, _validator, _crewRepo, _licenseRepo, _memberTypeRepo);
        var dto = new CreateCrewMemberDto(
            "John Doe2",
            "john2@doe.com",
            "BGR",
            "image",
            "2000-01-01",
            "2099-01-01",
            Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Guid.Parse("00000000-0000-0000-0000-000000000999"));

        _licenseRepo.AllAsNoTracking().Returns(GetLicenses().BuildMock());
        _memberTypeRepo.AllAsNoTracking().Returns(GetMemberTypes().BuildMock());
        _crewRepo.AllAsNoTracking().Returns(GetCrewMembers().BuildMock());

        var act = async () => await sut.Create(dto);

        await act.Should().ThrowExactlyAsync<EntityNotFoundException>();
        _crewRepo.DidNotReceive().Add(Arg.Any<CrewMember>());
        await _crewRepo.DidNotReceive().SaveAsync();
    }

    [Fact]
    public async Task Create_GivenDuplicateEmail_ThrowsCorrectError()
    {
        var sut = new CrewMemberService(null!, _validator, _crewRepo, _licenseRepo, _memberTypeRepo);
        var dto = new CreateCrewMemberDto(
            "John Doe2",
            GetCrewMembers().Last().Email,
            "BGR",
            "image",
            "2000-01-01",
            "2099-01-01",
            Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Guid.Parse("00000000-0000-0000-0000-000000000001"));

        _licenseRepo.AllAsNoTracking().Returns(GetLicenses().BuildMock());
        _memberTypeRepo.AllAsNoTracking().Returns(GetMemberTypes().BuildMock());
        _crewRepo.AllAsNoTracking().Returns(GetCrewMembers().BuildMock());

        var act = async () => await sut.Create(dto);

        await act.Should().ThrowExactlyAsync<DuplicateEntityException>();
        _crewRepo.DidNotReceive().Add(Arg.Any<CrewMember>());
        await _crewRepo.DidNotReceive().SaveAsync();
    }

    [Fact]
    public async Task Update_WhenCalledWithValidIdAndData_UpdatesCorrectCrewMember()
    {
        var sut = new CrewMemberService(null!, _validator, _crewRepo, _licenseRepo, _memberTypeRepo);
        var idToUpdate = Guid.Parse("00000000-0000-0000-0000-000000000999");
        var crewToUpdate = GetCrewMembers().First();
        var dto = new UpdateCrewMemberDto(
            Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Guid.Parse("00000000-0000-0000-0000-000000000001"),
            "John Doe2",
            "john2@doe.com",
            "BGR",
            "update_image");

        _crewRepo
           .FindAsync(Arg.Is<Guid>(c => c == idToUpdate))
           .Returns(crewToUpdate);

        _licenseRepo.AllAsNoTracking().Returns(GetLicenses().BuildMock());
        _memberTypeRepo.AllAsNoTracking().Returns(GetMemberTypes().BuildMock());
        _crewRepo.AllAsNoTracking().Returns(GetCrewMembers().BuildMock());

        await sut.Update(idToUpdate, dto);

        _crewRepo.Received(1).Update(Arg.Is<CrewMember>(c =>
            c.Id == crewToUpdate.Id &&
            c.Name == dto.Name &&
            c.Email == dto.Email &&
            c.CountryCode == dto.CountryCode &&
            c.ProfileImageUrl == dto.ProfileImageUrl &&
            c.LicenseId == dto.LicenseId &&
            c.MemberTypeId == dto.MemberTypeId));
        await _crewRepo.Received(1).SaveAsync();
    }

    [Fact]
    public async Task Update_WhenValidationDoesntPass_ThrowsCorrectError()
    {
        var sut = new CrewMemberService(null!, _validator, _crewRepo, _licenseRepo, _memberTypeRepo);
        var idToUpdate = Guid.Parse("00000000-0000-0000-0000-000000000999");
        var dto = new UpdateCrewMemberDto(
            Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Guid.Parse("00000000-0000-0000-0000-000000000001"),
            "John Doe2",
            "john2@doe.com",
            "BGR",
            "update_image");

        _validator.ThrowIfInvalid(Arg.Any<UpdateCrewMemberDto>());

        _validator.When(x => x.ThrowIfInvalid(Arg.Any<UpdateCrewMemberDto>()))
            .Do(x => throw new ValidationException([new ValidationError("Arg", "Msg")]));

        var act = async () => await sut.Update(idToUpdate, dto);

        await act.Should().ThrowExactlyAsync<ValidationException>();
        _crewRepo.DidNotReceive().Update(Arg.Any<CrewMember>());
        await _crewRepo.DidNotReceive().SaveAsync();
    }

    [Fact]
    public async Task Update_GivenInvalidCrewMemberId_ThrowsCorrectError()
    {
        var sut = new CrewMemberService(null!, _validator, _crewRepo, _licenseRepo, _memberTypeRepo);
        var idToUpdate = Guid.Parse("00000000-0000-0000-0000-000000000999");
        var crewToUpdate = GetCrewMembers().First();
        var dto = new UpdateCrewMemberDto(
            Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Guid.Parse("00000000-0000-0000-0000-000000000001"),
            "John Doe2",
            "john2@doe.com",
            "BGR",
            "update_image");

        _crewRepo
           .FindAsync(Arg.Is<Guid>(c => c != idToUpdate))
           .Returns(crewToUpdate);


        var act = async () => await sut.Update(idToUpdate, dto);

        await act.Should().ThrowExactlyAsync<EntityNotFoundException>();
        _crewRepo.DidNotReceive().Update(Arg.Any<CrewMember>());
        await _crewRepo.DidNotReceive().SaveAsync();
    }

    [Fact]
    public async Task Update_GivenInvalidLicenseId_ThrowsCorrectError()
    {
        var sut = new CrewMemberService(null!, _validator, _crewRepo, _licenseRepo, _memberTypeRepo);
        var idToUpdate = Guid.Parse("00000000-0000-0000-0000-000000000999");
        var crewToUpdate = GetCrewMembers().First();
        var dto = new UpdateCrewMemberDto(
            Guid.Parse("00000000-0000-0000-0000-000000000999"),
            Guid.Parse("00000000-0000-0000-0000-000000000001"),
            "John Doe2",
            "john2@doe.com",
            "BGR",
            "update_image");

        _crewRepo
           .FindAsync(Arg.Is<Guid>(c => c == idToUpdate))
           .Returns(crewToUpdate);

        _licenseRepo.AllAsNoTracking().Returns(GetLicenses().BuildMock());
        _memberTypeRepo.AllAsNoTracking().Returns(GetMemberTypes().BuildMock());
        _crewRepo.AllAsNoTracking().Returns(GetCrewMembers().BuildMock());

        var act = async () => await sut.Update(idToUpdate, dto);

        await act.Should().ThrowExactlyAsync<EntityNotFoundException>();
        _crewRepo.DidNotReceive().Update(Arg.Any<CrewMember>());
        await _crewRepo.DidNotReceive().SaveAsync();
    }

    [Fact]
    public async Task Update_GivenInvalidMemberTypeId_ThrowsCorrectError()
    {
        var sut = new CrewMemberService(null!, _validator, _crewRepo, _licenseRepo, _memberTypeRepo);
        var idToUpdate = Guid.Parse("00000000-0000-0000-0000-000000000999");
        var crewToUpdate = GetCrewMembers().First();
        var dto = new UpdateCrewMemberDto(
            Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Guid.Parse("00000000-0000-0000-0000-000000000999"),
            "John Doe2",
            "john2@doe.com",
            "BGR",
            "update_image");

        _crewRepo
           .FindAsync(Arg.Is<Guid>(c => c == idToUpdate))
           .Returns(crewToUpdate);

        _licenseRepo.AllAsNoTracking().Returns(GetLicenses().BuildMock());
        _memberTypeRepo.AllAsNoTracking().Returns(GetMemberTypes().BuildMock());
        _crewRepo.AllAsNoTracking().Returns(GetCrewMembers().BuildMock());

        var act = async () => await sut.Update(idToUpdate, dto);

        await act.Should().ThrowExactlyAsync<EntityNotFoundException>();
        _crewRepo.DidNotReceive().Update(Arg.Any<CrewMember>());
        await _crewRepo.DidNotReceive().SaveAsync();
    }

    [Fact]
    public async Task Update_GivenDuplicateEmail_ThrowsCorrectError()
    {
        var sut = new CrewMemberService(null!, _validator, _crewRepo, _licenseRepo, _memberTypeRepo);
        var idToUpdate = Guid.Parse("00000000-0000-0000-0000-000000000999");
        var crewToUpdate = GetCrewMembers().First();
        var dto = new UpdateCrewMemberDto(
            Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Guid.Parse("00000000-0000-0000-0000-000000000001"),
            "John Doe2",
            GetCrewMembers().Last().Email,
            "BGR",
            "update_image");

        _crewRepo
           .FindAsync(Arg.Is<Guid>(c => c == idToUpdate))
           .Returns(crewToUpdate);

        _licenseRepo.AllAsNoTracking().Returns(GetLicenses().BuildMock());
        _memberTypeRepo.AllAsNoTracking().Returns(GetMemberTypes().BuildMock());
        _crewRepo.AllAsNoTracking().Returns(GetCrewMembers().BuildMock());

        var act = async () => await sut.Update(idToUpdate, dto);

        await act.Should().ThrowExactlyAsync<DuplicateEntityException>();
        _crewRepo.DidNotReceive().Update(Arg.Any<CrewMember>());
        await _crewRepo.DidNotReceive().SaveAsync();
    }

    [Fact]
    public async Task Delete_WhenCalledWithValidId_DeletesCorrectCrewMember()
    {
        var sut = new CrewMemberService(null!, _validator, _crewRepo, _licenseRepo, _memberTypeRepo);

        var idToDelete = Guid.Parse("00000000-0000-0000-0000-000000000999");
        var crewToDelete = GetCrewMembers().First();

        _crewRepo
           .FindAsync(Arg.Is<Guid>(c => c == idToDelete))
           .Returns(crewToDelete);

        await sut.Delete(idToDelete);

        _crewRepo.Received(1).Delete(Arg.Is<CrewMember>(c => c.Id == crewToDelete.Id && c.Name == crewToDelete.Name));
        await _crewRepo.Received(1).SaveAsync();
    }

    [Fact]
    public async Task Delete_WhenCalledWithInvalidId_ThrowsNotFoundException()
    {
        var sut = new CrewMemberService(null!, _validator, _crewRepo, _licenseRepo, _memberTypeRepo);

        var idToDelete = Guid.Parse("00000000-0000-0000-0000-000000000999");

        _crewRepo
           .FindAsync(Arg.Is<Guid>(c => c != idToDelete))
           .Returns(GetCrewMembers().First());

        var act = async () => await sut.Delete(idToDelete);

        await act.Should().ThrowExactlyAsync<EntityNotFoundException>();
        _crewRepo.DidNotReceive().Delete(Arg.Any<CrewMember>());
        await _crewRepo.DidNotReceive().SaveAsync();
    }

    [Fact]
    public async Task RenewLicense_WhenCalledWithValidId_UpdatesCrewMember()
    {
        var mockHttp = new MockHttpMessageHandler();
        var statusResponse = new CrewMemberStatusDto(CrewMemberStatus.Ok, "All good");
        var request = mockHttp.When("/api/crew-members/status")
            .Respond("application/json", JsonConvert.SerializeObject(statusResponse));

        var client = mockHttp.ToHttpClient();
        client.BaseAddress = new Uri("http://localhost");
        var sut = new CrewMemberService(client, _validator, _crewRepo, _licenseRepo, _memberTypeRepo);

        var idToRenew = Guid.Parse("00000000-0000-0000-0000-000000000999");

        var newDate = new DateOnly(2090, 1, 1);

        _crewRepo
           .FindAsync(Arg.Is<Guid>(c => c == idToRenew))
           .Returns(GetCrewMembers().First());

        await sut.RenewLicense(idToRenew, newDate);

        _crewRepo.Received(1).Update(Arg.Is<CrewMember>(c => 
            c.Status == statusResponse.Status &&
            c.StatusDescription == statusResponse.StatusDescription &&
            c.LastCertificationDate == newDate));
        await _crewRepo.Received(1).SaveAsync();
        mockHttp.GetMatchCount(request).Should().Be(1);
    }

    [Fact]
    public async Task RenewLicense_WhenCrewMemberIsRetired_ThrowsCorrectError()
    {
        var mockHttp = new MockHttpMessageHandler();
        var statusResponse = new CrewMemberStatusDto(CrewMemberStatus.Ok, "All good");
        var request = mockHttp.When("/api/crew-members/status")
            .Respond("application/json", JsonConvert.SerializeObject(statusResponse));

        var client = mockHttp.ToHttpClient();
        client.BaseAddress = new Uri("http://localhost");
        var sut = new CrewMemberService(client, _validator, _crewRepo, _licenseRepo, _memberTypeRepo);

        var idToRenew = Guid.Parse("00000000-0000-0000-0000-000000000999");

        var newDate = new DateOnly(2090, 1, 1);

        _crewRepo
           .FindAsync(Arg.Is<Guid>(c => c == idToRenew))
           .Returns(GetCrewMembers().Where(c => c.Status == CrewMemberStatus.CrewRetired).First());

        var act = async () => await sut.RenewLicense(idToRenew, newDate);

        await act.Should().ThrowExactlyAsync<ValidationException>();
        _crewRepo.DidNotReceive().Update(Arg.Any<CrewMember>());
        await _crewRepo.DidNotReceive().SaveAsync();
        mockHttp.GetMatchCount(request).Should().Be(0);
    }

    [Fact]
    public async Task RenewLicense_WhenCalledWithInvalidId_ThrowsNotFoundException()
    {
        var mockHttp = new MockHttpMessageHandler();
        var statusResponse = new CrewMemberStatusDto(CrewMemberStatus.Ok, "All good");
        var request = mockHttp.When("/api/crew-members/status")
            .Respond("application/json", JsonConvert.SerializeObject(statusResponse));

        var client = mockHttp.ToHttpClient();
        client.BaseAddress = new Uri("http://localhost");
        var sut = new CrewMemberService(client, _validator, _crewRepo, _licenseRepo, _memberTypeRepo);

        var idToRenew = Guid.Parse("00000000-0000-0000-0000-000000000999");

        _crewRepo
           .FindAsync(Arg.Is<Guid>(c => c != idToRenew))
           .Returns(GetCrewMembers().First());

        var act = async () => await sut.RenewLicense(idToRenew, new DateOnly());

        await act.Should().ThrowExactlyAsync<EntityNotFoundException>();
        _crewRepo.DidNotReceive().Update(Arg.Any<CrewMember>());
        await _crewRepo.DidNotReceive().SaveAsync();
        mockHttp.GetMatchCount(request).Should().Be(0);
    }

    private static IQueryable<License> GetLicenses()
    {
        return new List<License>
        {
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Name = "License 1",
                Description = "License 1 Description"
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                Name = "License 2",
                Description = "License 2 Description"
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                Name = "License 3",
                Description = "License 3 Description"
            }
        }.AsQueryable();
    }

    private static IQueryable<MemberType> GetMemberTypes()
    {
        return new List<MemberType>
        {
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Name = "MT 1",
                Type = MemberTypeEnum.Steward
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                Name = "MT 2",
                Type = MemberTypeEnum.Pilot
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                Name = "MT 3",
                Type = MemberTypeEnum.Regular
            }
        }.AsQueryable();
    }

    private static IQueryable<CrewMember> GetCrewMembers()
    {
        return new List<CrewMember>
        {
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Name = "John Doe",
                Email = "john@doe.com",
                CountryCode = "USA",
                Birthday = new DateOnly(1980, 1, 1),
                LastCertificationDate = new DateOnly(2025, 1, 1),
                LicenseId = Guid.Parse("00000000-0000-0000-0000-000000000011"),
                MemberTypeId = Guid.Parse("00000000-0000-0000-0000-000000000021"),
                ProfileImageUrl = "https://www.example.com/john.jpg",
                Status = CrewMemberStatus.Ok,
                StatusDescription = "All good",
                License = new License
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000011"),
                    Name = "License 1",
                    Description = "License 1 description"
                },
                MemberType = new MemberType
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000021"),
                    Name = "Member Type 1",
                    Type = MemberTypeEnum.Pilot
                }
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                Name = "Jane Doe",
                Email = "Jane@doe.com",
                CountryCode = "USA",
                Birthday = new DateOnly(1980, 1, 1),
                LastCertificationDate = new DateOnly(2025, 1, 1),
                LicenseId = Guid.Parse("00000000-0000-0000-0000-000000000011"),
                MemberTypeId = Guid.Parse("00000000-0000-0000-0000-000000000021"),
                ProfileImageUrl = "https://www.example.com/jane.jpg",
                Status = CrewMemberStatus.Ok,
                StatusDescription = "All good",
                License = new License
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000011"),
                    Name = "License 1",
                    Description = "License 1 description"
                },
                MemberType = new MemberType
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000021"),
                    Name = "Member Type 1",
                    Type = MemberTypeEnum.Pilot
                }
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                Name = "Jack Doe",
                Email = "jack@doe.com",
                CountryCode = "USA",
                Birthday = new DateOnly(1980, 1, 1),
                LastCertificationDate = new DateOnly(2025, 1, 1),
                LicenseId = Guid.Parse("00000000-0000-0000-0000-000000000011"),
                MemberTypeId = Guid.Parse("00000000-0000-0000-0000-000000000021"),
                ProfileImageUrl = "https://www.example.com/jack.jpg",
                Status = CrewMemberStatus.Ok,
                StatusDescription = "All good",
                License = new License
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000011"),
                    Name = "License 1",
                    Description = "License 1 description"
                },
                MemberType = new MemberType
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000021"),
                    Name = "Member Type 1",
                    Type = MemberTypeEnum.Pilot
                }
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                Name = "Ivan Doe",
                Email = "ivan@doe.com",
                CountryCode = "USA",
                Birthday = new DateOnly(1980, 1, 1),
                LastCertificationDate = new DateOnly(2025, 1, 1),
                LicenseId = Guid.Parse("00000000-0000-0000-0000-000000000011"),
                MemberTypeId = Guid.Parse("00000000-0000-0000-0000-000000000021"),
                ProfileImageUrl = "https://www.example.com/ivan.jpg",
                Status = CrewMemberStatus.Ok,
                StatusDescription = "All good",
                License = new License
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000011"),
                    Name = "License 1",
                    Description = "License 1 description"
                },
                MemberType = new MemberType
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000021"),
                    Name = "Member Type 1",
                    Type = MemberTypeEnum.Pilot
                }
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                Name = "Pesho Doe",
                Email = "Pesho@doe.com",
                CountryCode = "USA",
                Birthday = new DateOnly(1980, 1, 1),
                LastCertificationDate = new DateOnly(2025, 1, 1),
                LicenseId = Guid.Parse("00000000-0000-0000-0000-000000000011"),
                MemberTypeId = Guid.Parse("00000000-0000-0000-0000-000000000021"),
                ProfileImageUrl = "https://www.example.com/Pesho.jpg",
                Status = CrewMemberStatus.Ok,
                StatusDescription = "All good",
                License = new License
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000011"),
                    Name = "License 1",
                    Description = "License 1 description"
                },
                MemberType = new MemberType
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000021"),
                    Name = "Member Type 1",
                    Type = MemberTypeEnum.Pilot
                }
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                Name = "Gosho Doe",
                Email = "Gosho@doe.com",
                CountryCode = "USA",
                Birthday = new DateOnly(1980, 1, 1),
                LastCertificationDate = new DateOnly(2025, 1, 1),
                LicenseId = Guid.Parse("00000000-0000-0000-0000-000000000011"),
                MemberTypeId = Guid.Parse("00000000-0000-0000-0000-000000000021"),
                ProfileImageUrl = "https://www.example.com/Gosho.jpg",
                Status = CrewMemberStatus.CrewRetired,
                StatusDescription = "All good",
                License = new License
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000011"),
                    Name = "License 1",
                    Description = "License 1 description"
                },
                MemberType = new MemberType
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000021"),
                    Name = "Member Type 1",
                    Type = MemberTypeEnum.Pilot
                }
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                Name = "Tosho Doe",
                Email = "Tosho@doe.com",
                CountryCode = "USA",
                Birthday = new DateOnly(1980, 1, 1),
                LastCertificationDate = new DateOnly(2025, 1, 1),
                LicenseId = Guid.Parse("00000000-0000-0000-0000-000000000011"),
                MemberTypeId = Guid.Parse("00000000-0000-0000-0000-000000000021"),
                ProfileImageUrl = "https://www.example.com/Tosho.jpg",
                Status = CrewMemberStatus.Ok,
                StatusDescription = "All good",
                License = new License
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000011"),
                    Name = "License 1",
                    Description = "License 1 description"
                },
                MemberType = new MemberType
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000021"),
                    Name = "Member Type 1",
                    Type = MemberTypeEnum.Pilot
                }
            },

        }.AsQueryable();
    }

}
