using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Space.CrewManagement.Services.Dtos;
using Space.CrewManagement.Services.Interfaces;
using Space.CrewManagement.Services.Validation;
using System.Net;
using System.Web.Http;

namespace Space.CrewManagement.Func;

public class DeleteCrewMember(ILogger<DeleteCrewMember> _logger, ICrewMemberService _crewMemberService)
{
    [OpenApiOperation(operationId: "DeleteCrewMember", tags: ["crew-members"])]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
    [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "The ID of the crew member to be deleted")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK)]
    [Function("DeleteCrewMember")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "crew-members/{id}")] HttpRequest req, string id)
    {
        if (!Guid.TryParse(id, out var parsedId))
        {
            return new BadRequestResult();
        }

        try
        {
            await _crewMemberService.Delete(parsedId);
            return new CreatedResult();
        }
        catch (EntityNotFoundException nfEx)
        {
            return new NotFoundObjectResult(nfEx.ResponseObject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Following error occured: {message}", ex.Message);
            return new InternalServerErrorResult();
        }
    }
}
