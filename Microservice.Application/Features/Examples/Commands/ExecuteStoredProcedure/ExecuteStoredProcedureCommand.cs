using MediatR;
using Microservice.Application.Common.Results;

namespace Microservice.Application.Features.Examples.Commands.ExecuteStoredProcedure
{
    public record ExecuteStoredProcedureCommand(
        FormattableString Sql
    ) : IRequest<Result<int>>;
}
