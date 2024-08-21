using FluentAssertions;
using NSubstitute;
using Space.CrewManagement.Services.Dtos;
using Space.CrewManagement.Services.Exceptions;
using Space.CrewManagement.Services.Interfaces;
using Space.CrewManagement.Services.Services;
using Space.CrewManagement.Services.Validation;
using Xunit;

namespace Space.CrewManagement.Services.UnitTests;
public class CrewMemberValidatorTests
{
    private readonly ICountryService _countryService;
    private readonly IDateProvider _dateProvider;

    public CrewMemberValidatorTests()
    {
        _countryService = new CountryService();
        _dateProvider = Substitute.For<IDateProvider>();
    }

    [Fact]
    public void ThrowIfInvalidCreate_WhenCrewIsValid_DoesNotThrow()
    {
        var sut = new CrewMemberValidator(_countryService, _dateProvider);

        _dateProvider.Now.Returns(new DateOnly(2024, 1, 1));

        var createDto = new CreateCrewMemberDto(
            "name",
            "email@ex.ample",
            "BGR",
            "http://example.com",
            "2000-01-01",
            "2020-01-01",
            Guid.NewGuid(),
            Guid.NewGuid());

        var act = () => sut.ThrowIfInvalid(createDto);

        act.Should().NotThrow();
    }

    [Fact]
    public void ThrowIfInvalidCreate_WhenMissingDataIsProvided_ThrowsCorrectExpection()
    {
        var sut = new CrewMemberValidator(_countryService, _dateProvider);

        _dateProvider.Now.Returns(new DateOnly(2024, 1, 1));

        var createDto = new CreateCrewMemberDto(
            "",
            "",
            "",
            "http://example.com",
            "",
            "",
            Guid.NewGuid(),
            Guid.NewGuid());

        var act = () => sut.ThrowIfInvalid(createDto);

        act.Should().ThrowExactly<ValidationException>()
            .Which.ValidationErrors.Should().BeEquivalentTo([
                new ValidationError("Name", "Name is missing"),
                new ValidationError("Email", "Email is missing"),
                new ValidationError("CountryCode", "Country code is missing"),
                new ValidationError("Birthday", "Birthday is not in the correct format"),
                new ValidationError("LastCertificationDate", "Last certification date is not in the correct format")
                ]);
    }

    [Theory]
    [InlineData("12312", "Email is not a valid email")]
    [InlineData("asd", "Email is not a valid email")]
    [InlineData("1@1:123", "Email is not a valid email")]
    [InlineData("abc@ab@a", "Email is not a valid email")]
    [InlineData("01234567890012345678900123456789001234567890012345678900123456789001234567890012345678901012345678901", "Email is more than 100 characters")]
    public void ThrowIfInvalidCreate_GivenInvalidEmail_ThrowsCorrectExpection(string email, string expectedMessage)
    {
        var sut = new CrewMemberValidator(_countryService, _dateProvider);

        _dateProvider.Now.Returns(new DateOnly(2024, 1, 1));

        var createDto = new CreateCrewMemberDto(
            "name",
            email,
            "BGR",
            "http://example.com",
            "2000-01-01",
            "2020-01-01",
            Guid.NewGuid(),
            Guid.NewGuid());

        var act = () => sut.ThrowIfInvalid(createDto);

        act.Should().ThrowExactly<ValidationException>()
            .Which.ValidationErrors.Should().BeEquivalentTo([
                new ValidationError("Email", expectedMessage)
                ]);
    }

    [Theory]
    [InlineData("ASDASDASD", "Country code is more than 3 characters")]
    [InlineData("ZZZ", "Country code is not valid")]
    public void ThrowIfInvalidCreate_GivenInvalidCountryCode_ThrowsCorrectExpection(string countryCode, string expectedMessage)
    {
        var sut = new CrewMemberValidator(_countryService, _dateProvider);

        _dateProvider.Now.Returns(new DateOnly(2024, 1, 1));

        var createDto = new CreateCrewMemberDto(
            "name",
            "email@example.com",
            countryCode,
            "http://example.com",
            "2000-01-01",
            "2020-01-01",
            Guid.NewGuid(),
            Guid.NewGuid());

        var act = () => sut.ThrowIfInvalid(createDto);

        act.Should().ThrowExactly<ValidationException>()
            .Which.ValidationErrors.Should().BeEquivalentTo([
                new ValidationError("CountryCode", expectedMessage)
                ]);
    }

    [Fact]
    public void ThrowIfInvalidCreate_GivenInvalidBirthday_ThrowsCorrectExpection()
    {
        var sut = new CrewMemberValidator(_countryService, _dateProvider);

        _dateProvider.Now.Returns(new DateOnly(2024, 1, 1));

        var birthdayInFuture = new CreateCrewMemberDto(
            "name",
            "email@example.com",
            "BGR",
            "http://example.com",
            "2025-01-01",
            "2020-01-01",
            Guid.NewGuid(),
            Guid.NewGuid());

        var act = () => sut.ThrowIfInvalid(birthdayInFuture);

        act.Should().ThrowExactly<ValidationException>()
            .Which.ValidationErrors.Should().BeEquivalentTo([
                new ValidationError("Birthday", "Birthday is in the future")
                ]);

        var lessThan18 = birthdayInFuture with { Birthday = "2010-01-01" };
        act = () => sut.ThrowIfInvalid(lessThan18);

        act.Should().ThrowExactly<ValidationException>()
            .Which.ValidationErrors.Should().BeEquivalentTo([
                new ValidationError("Birthday", "Crew member is younger than 18")
                ]);
    }
}
