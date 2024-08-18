using FluentAssertions;
using MockQueryable.NSubstitute;
using NSubstitute;
using Space.CrewManagement.Data.Models;
using Space.CrewManagement.Data.Repositories;
using Space.CrewManagement.Services.Services;
using Xunit;

namespace Space.CrewManagement.Services.UnitTests;

public class LicenseServiceTests
{
    [Fact]
    public async Task GetAll_WhenCalled_ReturnsAllLicenses()
    {
        var repo = Substitute.For<IRepository<License>>();
        var sut = new LicenseService(repo);

        repo.AllAsNoTracking().Returns(GetLicenses().BuildMock());

        var result = await sut.GetAll();

        result.Should().HaveCount(3);
    }

    private static IQueryable<License> GetLicenses()
    {
        return new List<License>
        {
            new() { Id = Guid.NewGuid(), Name = "License 1", Description = "License 1 Description" },
            new() { Id = Guid.NewGuid(), Name = "License 2", Description = "License 2 Description" },
            new() { Id = Guid.NewGuid(), Name = "License 3", Description = "License 3 Description" }
        }.AsQueryable();
    }
}
