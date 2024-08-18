using FluentAssertions;
using MockQueryable.NSubstitute;
using NSubstitute;
using Space.CrewManagement.Data.Models;
using Space.CrewManagement.Data.Repositories;
using Space.CrewManagement.Services.Services;
using Xunit;

namespace Space.CrewManagement.Services.UnitTests;

public class CountryServiceTests
{
    [Fact]
    public void GetAll_WhenCalled_ReturnsAllCountries()
    {
        var sut = new CountryService();

        var result = sut.GetAll();

        result.Should().HaveCount(249);
    }
}
