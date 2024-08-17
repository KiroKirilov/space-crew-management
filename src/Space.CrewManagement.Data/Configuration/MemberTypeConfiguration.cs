using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Space.CrewManagement.Data.Models;

namespace Space.CrewManagement.Data.Configuration;
public class MemberTypeConfiguration : IEntityTypeConfiguration<MemberType>
{
    public void Configure(EntityTypeBuilder<MemberType> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.HasData(
            new MemberType
            {
                Id = new Guid("f3b3b3b3-3b3b-3b3b-3b3b-3b3b3b3b3b3b"),
                Name = "Pilot",
                Type = MemberTypeEnum.Pilot
            },
            new MemberType
            {
                Id = new Guid("f3b3b3b3-3b3b-3b3b-3b3b-3b3b3b3b3b3c"),
                Name = "Regular",
                Type = MemberTypeEnum.Regular
            },
            new MemberType
            {
                Id = new Guid("f3b3b3b3-3b3b-3b3b-3b3b-3b3b3b3b3b3d"),
                Name = "Steward",
                Type = MemberTypeEnum.Steward
            });
    }
}
