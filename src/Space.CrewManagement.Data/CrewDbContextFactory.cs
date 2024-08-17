using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Space.CrewManagement.Data;

public class CrewDbContextFactory : IDesignTimeDbContextFactory<CrewDbContext>
{
    public CrewDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("local.settings.json")
            .Build();

        var builder = new DbContextOptionsBuilder<CrewDbContext>();
        var connectionString = configuration.GetConnectionString("CrewDb");
        builder.UseSqlServer(connectionString);
        return new CrewDbContext(builder.Options);
    }
}
