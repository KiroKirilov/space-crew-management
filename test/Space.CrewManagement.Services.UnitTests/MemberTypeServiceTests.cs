using FluentAssertions;
using MockQueryable.NSubstitute;
using NSubstitute;
using Space.CrewManagement.Data.Models;
using Space.CrewManagement.Data.Repositories;
using Space.CrewManagement.Services.Services;
using Xunit;

namespace Space.CrewManagement.Services.UnitTests;

public class MemberTypeServiceTests
{
    [Fact]
    public async Task GetAll_WhenCalled_ReturnsAllMemberTypes()
    {
        var repo = Substitute.For<IRepository<MemberType>>();
        var sut = new MemberTypeService(repo);

        repo.AllAsNoTracking().Returns(GetMemberTypes().BuildMock());

        var result = await sut.GetAll();

        result.Should().HaveCount(3);
    }

    private static IQueryable<MemberType> GetMemberTypes()
    {
        return new List<MemberType>
        {
            new() { Id = Guid.NewGuid(), Name = "MT 1", Type = MemberTypeEnum.Steward },
            new() { Id = Guid.NewGuid(), Name = "MT 2", Type = MemberTypeEnum.Pilot },
            new() { Id = Guid.NewGuid(), Name = "MT 3", Type = MemberTypeEnum.Regular }
        }.AsQueryable();
    }
}
