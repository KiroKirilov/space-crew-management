namespace Space.CrewManagement.Data.Models;

public class CrewMember : BaseModel<Guid>
{
    public string Name { get; set; } = string.Empty;
    public DateOnly Birthday { get; set; }
    public string Email { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public string ProfileImageUrl { get; set; } = string.Empty;
    public DateOnly LastCertificationDate { get; set; }

    public CrewMemberStatus Status { get; set; }
    public string StatusDescription { get; set; } = string.Empty;

    public Guid LicenseId { get; set; }
    public virtual License License { get; set; } = null!;

    public Guid MemberTypeId { get; set; }
    public virtual MemberType MemberType { get; set; } = null!;
}

public enum CrewMemberStatus
{
    Ok = 0,
    LicenseExpired = 1,
    CrewRetired = 2
}