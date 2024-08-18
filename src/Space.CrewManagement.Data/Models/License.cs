namespace Space.CrewManagement.Data.Models;
public class License : BaseModel<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public virtual ICollection<CrewMember> CrewMembers { get; set; } = [];
 }
