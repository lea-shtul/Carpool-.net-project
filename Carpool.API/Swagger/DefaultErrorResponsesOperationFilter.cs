using Carpool.API.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Carpool.API.Swagger;

/// <summary>
/// Adds the error responses that every (or nearly every) endpoint can produce, so the
/// Swagger document shows them without repeating <c>[ProducesResponseType]</c> on 20
/// actions (spec §61). All are the shared <see cref="ErrorResponse"/> shape returned by
/// <see cref="ExceptionHandlingMiddleware"/>. <c>401</c>/<c>403</c> are omitted for
/// endpoints marked <c>[AllowAnonymous]</c>.
/// </summary>
public sealed class DefaultErrorResponsesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var errorSchema = context.SchemaGenerator.GenerateSchema(typeof(ErrorResponse), context.SchemaRepository);

        void AddIfMissing(string statusCode, string description)
        {
            if (operation.Responses.ContainsKey(statusCode))
            {
                return;
            }

            operation.Responses[statusCode] = new OpenApiResponse
            {
                Description = description,
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new() { Schema = errorSchema },
                },
            };
        }

        var isAnonymous = context.ApiDescription.ActionDescriptor.EndpointMetadata
            .OfType<IAllowAnonymous>()
            .Any();

        AddIfMissing("400", "The request is invalid — model validation failed or a business rule was violated.");

        if (!isAnonymous)
        {
            AddIfMissing("401", "Authentication is required, or the bearer token is missing or invalid.");
            AddIfMissing("403", "Authenticated, but not permitted to perform this action.");
        }

        AddIfMissing("404", "A referenced resource does not exist.");
        AddIfMissing("409", "The request conflicts with the current state of a resource.");
        AddIfMissing("500", "An unexpected server error occurred.");
    }
}
