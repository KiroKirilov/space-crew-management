using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Space.CrewManagement.Services.Dtos;
using Space.CrewManagement.Services.Exceptions;
using Space.CrewManagement.Services.Interfaces;
using System.Net;
using System.Web.Http;

namespace Space.CrewManagement.Func;

public class GetCrewMemberbyId(ILogger<GetCrewMemberbyId> _logger, ICrewMemberService _crewMemberService)
{
    [OpenApiOperation(operationId: "GetCrewMemberbyId", tags: ["crew-members"])]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
    [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "The ID of the crew member to be deleted")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(CrewMemberResponseDto))]
    [Function("GetCrewMemberbyId")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "crew-members/{id}")] HttpRequest req, string id)
    {
        if (!Guid.TryParse(id, out var parsedId))
        {
            return new BadRequestResult();
        }

        try
        {
            var crewMember = await _crewMemberService.GetById(parsedId);
            return new OkObjectResult(crewMember);
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
