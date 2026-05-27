using MediatR;
using Microservice.Application.Common.Results;

namespace Microservice.Application.Features.ExamplesEF.Commands.ExecuteStoredProcedure
{
    public record ExecuteStoredProcedureCommand(
        FormattableString Sql
    ) : IRequest<Result<int>>;
}
