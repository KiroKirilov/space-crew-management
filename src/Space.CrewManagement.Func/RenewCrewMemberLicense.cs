using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Space.CrewManagement.Services.Exceptions;
using Space.CrewManagement.Services.Interfaces;
using System.Net;
using System.Web.Http;

namespace Space.CrewManagement.Func;

public class RenewCrewMemberLicense(ILogger<RenewCrewMemberLicense> _logger, ICrewMemberService _crewMemberService)
{
    [OpenApiOperation(operationId: "RenewCrewMemberLicense", tags: ["crew-members"])]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
    [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "The ID of the crew member to be renewed")]
    [OpenApiParameter(name: "newCertificationDate", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The new certification date")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK)]
    [Function("RenewCrewMemberLicense")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "patch", Route = "crew-members/{id}/license/renew")] HttpRequest req, string id)
    {
        if (!Guid.TryParse(id, out var parsedId))
        {
            return new BadRequestResult();
        }

        if (!DateOnly.TryParseExact(req.Query["newCertificationDate"], "yyyy-MM-dd", out var newCertificationDate))
        {
            return new BadRequestObjectResult(new { Message = "Invalid date." });
        }

        try
        {
            await _crewMemberService.RenewLicense(parsedId, newCertificationDate);
            return new OkResult();
        }
        catch (ValidationException valEx)
        {
            return new BadRequestObjectResult(valEx.ValidationErrors);
        }
        catch (EntityNotFoundException nfEx)
        {
            return new NotFoundObjectResult(nfEx.ResponseObject);
        }
        catch (ExternalServiceException ex)
        {
            return new BadRequestObjectResult(new { ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Following error occured: {message}", ex.Message);
            return new InternalServerErrorResult();
        }
    }
}
