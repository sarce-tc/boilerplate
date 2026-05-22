using MediatR;
using Microservice.Application.Common.Results;

namespace Microservice.Application.Features.Examples.Commands.ExecuteInTransaction
{
    public record ExecuteInTransactionCommand(
        string Description
    ) : IRequest<Result<int>>;
}
