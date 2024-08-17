using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Space.CrewManagement.Data;
using Space.CrewManagement.Data.Repositories;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication(w => w.UseNewtonsoftJson())
    .ConfigureOpenApi()
    .ConfigureServices((hostContext, services) =>
    {
        services.AddDbContext<CrewDbContext>(opts =>
        {
            opts.UseSqlServer(hostContext.Configuration.GetConnectionString("CrewDb"));
        });

        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));

        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
    })
    .Build();

host.Run();
