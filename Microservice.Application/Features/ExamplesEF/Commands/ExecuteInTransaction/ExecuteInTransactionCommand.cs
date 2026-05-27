using MediatR;
using Microservice.Application.Common.Results;

namespace Microservice.Application.Features.ExamplesEF.Commands.ExecuteInTransaction
{
    public record ExecuteInTransactionCommand(
        string Description
    ) : IRequest<Result<int>>;
}
