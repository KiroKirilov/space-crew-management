using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Space.CrewManagement.Data.Models;

namespace Space.CrewManagement.Data.Configuration;
public class LicenseConfiguration : IEntityTypeConfiguration<License>
{
    public void Configure(EntityTypeBuilder<License> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);

        builder.HasData(
            new License
            {
                Id = new Guid("ead47c5e-c268-44ea-8837-30ff16e0ee10"),
                Name = "Airline transport pilot (ATP) license",
                Description = " Pilots with an ATP certificate are eligible to fly for an airline and will meet the hiring minimums of most regional airline pilot jobs."
            },
            new License
            {
                Id = new Guid("09917044-4413-43a4-82bf-4689ba49d2a2"),
                Name = "Cabin crew license",
                Description = " An individual wishing to work as a cabin crewmember in commercial air transport within an EC member State must hold a valid cabin crew attestation(CCA)."
            });
    }
}
