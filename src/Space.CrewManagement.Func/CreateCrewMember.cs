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

public class CreateCrewMember(ILogger<CreateCrewMember> _logger, IBodyParser _parser, ICrewMemberService _crewMemberService)
{
    [OpenApiOperation(operationId: "CreateCrewMember", tags: ["crew-members"])]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(CreateCrewMemberDto))]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Created)]
    [Function("CreateCrewMember")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "crew-members")] HttpRequest req)
    {
        var dto = await _parser.Parse<CreateCrewMemberDto>(req.Body);
        if (dto is null)
        {
            return new BadRequestResult();
        }

        try
        {
            await _crewMemberService.Create(dto);
            return new CreatedResult();
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
