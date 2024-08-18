using Microsoft.EntityFrameworkCore;
using Space.CrewManagement.Data.Models;

namespace Space.CrewManagement.Data;
public class CrewDbContext : DbContext
{
    public CrewDbContext()
    {
    }

    public CrewDbContext(DbContextOptions<CrewDbContext> options)
        : base(options)
    {
    }

    public DbSet<CrewMember> CrewMembers { get; set; } = null!;
    public DbSet<License> Licenses { get; set; } = null!;
    public DbSet<MemberType> MemberTypes { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .ApplyConfigurationsFromAssembly(typeof(CrewDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
