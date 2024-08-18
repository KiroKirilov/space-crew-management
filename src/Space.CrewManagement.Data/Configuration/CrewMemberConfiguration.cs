using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Space.CrewManagement.Data.Models;

namespace Space.CrewManagement.Data.Configuration;

public class CrewMemberConfiguration : IEntityTypeConfiguration<CrewMember>
{
    public void Configure(EntityTypeBuilder<CrewMember> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Email).IsRequired().HasMaxLength(200);
        builder.Property(x => x.CountryCode).HasMaxLength(3);

        builder.HasOne(x => x.License).WithMany(x => x.CrewMembers).HasForeignKey(x => x.LicenseId);
        builder.HasOne(x => x.MemberType).WithMany(x => x.CrewMembers).HasForeignKey(x => x.MemberTypeId);

        builder.HasIndex(x => x.Email).IsUnique();
    }
}
