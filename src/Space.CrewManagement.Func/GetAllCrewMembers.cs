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

public class GetAllCrewMembers(ILogger<GetAllCrewMembers> _logger, ICrewMemberService _crewMemberService)
{
    [OpenApiOperation(operationId: "GetAllCrewMembers", tags: ["crew-members"])]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
    [OpenApiParameter(name: "page", In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "Page to be retrieved")]
    [OpenApiParameter(name: "pageSize", In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "Size of the page to be retrieved")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(CrewMemberListDto))]
    [Function("GetAllCrewMembers")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "crew-members")] HttpRequest req)
    {
        var validPage = int.TryParse(req.Query["page"], out var parsedPage);
        var validPageSize = int.TryParse(req.Query["pageSize"], out var parsedPageSize);

        var page = validPage ? parsedPage : (int?)null;
        var pageSize = validPageSize ? parsedPageSize : (int?)null;

        try
        {
            var crewMember = await _crewMemberService.GetAll(page, pageSize);
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
