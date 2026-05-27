using MediatR;
using Microservice.Application.Common.Results;

namespace Microservice.Application.Features.ExamplesEF.Queries.CountExamples
{
    public record CountExamplesQuery : IRequest<Result<int>>;
}
