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

public class GetAllLicenses(ILogger<GetAllLicenses> _logger, ILicenseService _licenseService)
{

    [OpenApiOperation(operationId: "GetAllLicenses", tags: ["licenses"])]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<LicenseDto>))]
    [Function("GetAllLicenses")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "licenses")] HttpRequest req)
    {
        try
        {
            var licenses = await _licenseService.GetAll();
            return new OkObjectResult(licenses);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while getting licenses.");
            return new InternalServerErrorResult();
        }
    }
}
