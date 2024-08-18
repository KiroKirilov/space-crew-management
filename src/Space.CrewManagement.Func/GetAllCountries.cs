using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Space.CrewManagement.Services.Dtos;
using Space.CrewManagement.Services.Interfaces;
using System.Net;
using System.Web.Http;

namespace Space.CrewManagement.Func;

public class GetAllCountries(ILogger<GetAllCountries> _logger, ICountryService _countryService)
{

    [OpenApiOperation(operationId: "GetAllCountries", tags: ["countries"])]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<CountryDto>))]
    [Function("GetAllCountries")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "countries")] HttpRequest req)
    {
        try
        {
            var countries = _countryService.GetAll();
            return new OkObjectResult(countries);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while getting licenses.");
            return new InternalServerErrorResult();
        }
    }
}
