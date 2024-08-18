using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Space.CrewManagement.Data;
using Space.CrewManagement.Data.Repositories;
using Space.CrewManagement.Services.Interfaces;
using Space.CrewManagement.Services.Services;

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
        services.AddTransient<ILicenseService, LicenseService>();
        services.AddTransient<IMemberTypeService, MemberTypeService>();
        services.AddSingleton<ICountryService, CountryService>();
        services.AddSingleton<ICrewMemberValidator, CrewMemberValidator>();
        services.AddSingleton<IDateProvider, DateProvider>();
        services.AddSingleton<IBodyParser, BodyParser>();   

        services.AddHttpClient<ICrewMemberService, CrewMemberService>(httpClient =>
        {
            var crewMemberServiceUrl = hostContext.Configuration["CrewMemberServiceUrl"];
            if (string.IsNullOrWhiteSpace(crewMemberServiceUrl))
            {
                throw new InvalidOperationException("CrewMemberServiceUrl is missing.");
            }

            httpClient.BaseAddress = new Uri(crewMemberServiceUrl);
            httpClient.DefaultRequestHeaders.Add("x-functions-key", hostContext.Configuration["CrewMemberServiceKey"]);
        });

        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
    })
    .Build();

host.Run();
