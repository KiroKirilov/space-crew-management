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

public class UpdateCrewMember(ILogger<UpdateCrewMember> _logger, IBodyParser _parser, ICrewMemberService _crewMemberService)
{
    [OpenApiOperation(operationId: "UpdateCrewMember", tags: ["crew-members"])]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
    [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "The ID of the crew member to be updated")]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(UpdateCrewMemberDto))]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK)]
    [Function("UpdateCrewMember")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "patch", Route = "crew-members/{id}")] HttpRequest req, string id)
    {
        var dto = await _parser.Parse<UpdateCrewMemberDto>(req.Body);
        if (dto is null)
        {
            return new BadRequestResult();
        }

        if (!Guid.TryParse(id, out var parsedId))
        {
            return new BadRequestResult();
        }

        try
        {
            await _crewMemberService.Update(parsedId, dto);
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
        catch (DuplicateEntityException dEx)
        {
            return new ConflictObjectResult(dEx.ResponseObject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Following error occured: {message}", ex.Message);
            return new InternalServerErrorResult();
        }
    }
}
