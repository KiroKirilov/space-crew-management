namespace Space.CrewManagement.Data.Models;
public class MemberType : BaseModel<Guid>
{
    public string Name { get; set; } = string.Empty;
    public MemberTypeEnum Type { get; set; }

    public virtual ICollection<CrewMember> CrewMembers { get; set; } = [];
}

public enum MemberTypeEnum
{
    Pilot = 0,
    Regular = 1, 
    Steward = 2
}