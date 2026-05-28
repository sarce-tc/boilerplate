using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs;

namespace Microservice.Application.Features.ExamplesEF.Queries.GetExampleSummary;

/// <summary>
/// Returns a lightweight summary of an <c>Example</c> aggregate including computed item statistics.
/// </summary>
public record GetExampleSummaryQuery(Guid PublicId) : IRequest<Result<GetExampleSummaryDto>>;
