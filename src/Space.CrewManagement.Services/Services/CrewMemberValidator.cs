using Space.CrewManagement.Services.Dtos;
using Space.CrewManagement.Services.Exceptions;
using Space.CrewManagement.Services.Interfaces;
using Space.CrewManagement.Services.Validation;
using System.Net.Mail;

namespace Space.CrewManagement.Services.Services;
public class CrewMemberValidator(ICountryService _countryService, IDateProvider _dateProvider) : ICrewMemberValidator
{
    public void ThrowIfInvalid(CreateCrewMemberDto dto)
    {
        var errors = ValidateBase(dto);

        if (!DateOnly.TryParseExact(dto.Birthday, "yyyy-MM-dd", out var birthdayDate))
        {
            errors.Add(new ValidationError("Birthday", "Birthday is not in the correct format"));
        }
        else
        {
            if (birthdayDate > _dateProvider.Now)
            {
                errors.Add(new ValidationError("Birthday", "Birthday is in the future"));
            }
            else if (birthdayDate.AddYears(18) > _dateProvider.Now)
            {
                errors.Add(new ValidationError("Birthday", "Crew member is younger than 18"));
            }
        }

        if (!DateOnly.TryParseExact(dto.LastCertificationDate, "yyyy-MM-dd", out var _))
        {
            errors.Add(new ValidationError("LastCertificationDate", "Last certification date is not in the correct format"));
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }
    }

    public void ThrowIfInvalid(UpdateCrewMemberDto dto)
    {
        var errors = ValidateBase(dto);

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }
    }

    public List<ValidationError> ValidateBase(CrewMemberBaseDto dto)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            errors.Add(new ValidationError("Name", "Name is missing"));
        }

        if (string.IsNullOrWhiteSpace(dto.Email))
        {
            errors.Add(new ValidationError("Email", "Email is missing"));
        }
        else if (dto.Email.Length > 100)
        {
            errors.Add(new ValidationError("Email", "Email is more than 100 characters"));
        }
        else if (!MailAddress.TryCreate(dto.Email, null, out var _))
        {
            errors.Add(new ValidationError("Email", "Email is not a valid email"));
        }

        if (string.IsNullOrWhiteSpace(dto.CountryCode))
        {
            errors.Add(new ValidationError("CountryCode", "Country code is missing"));
        }
        else if (dto.CountryCode.Length > 3)
        {
            errors.Add(new ValidationError("CountryCode", "Country code is more than 3 characters"));
        }
        else if (!_countryService.GetAllCodes().Contains(dto.CountryCode))
        {
            errors.Add(new ValidationError("CountryCode", "Country code is not valid"));
        }

        return errors;
    }
}
