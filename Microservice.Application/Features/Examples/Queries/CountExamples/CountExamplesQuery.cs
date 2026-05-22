using MediatR;
using Microservice.Application.Common.Results;

namespace Microservice.Application.Features.Examples.Queries.CountExamples
{
    public record CountExamplesQuery : IRequest<Result<int>>;
}
