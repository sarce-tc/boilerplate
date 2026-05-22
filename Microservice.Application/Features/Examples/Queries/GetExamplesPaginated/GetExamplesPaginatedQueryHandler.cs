using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs;
using Microservice.Application.Models;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.Examples.Queries.GetExamplesPaginated
{
    /// <summary>
    /// Use Case: Retrieve Example records with pagination support for large datasets.
    /// 
    /// When to use:
    /// - When displaying data in a paginated UI (tables, grids, lists)
    /// - When handling large datasets that shouldn't be loaded entirely into memory
    /// - When AI agents need to process data in batches
    /// - For implementing infinite scroll or "load more" functionality
    /// 
    /// Responsibilities:
    /// - Request specific page of results from the repository
    /// - Handle page navigation parameters (CurrentPage, PageSize)
    /// - Project entities to DTOs
    /// - Return metadata about pagination (total rows, current page, page size)
    /// 
    /// AI Agent Use Cases:
    /// - AI systems can use this to:
    ///   * Process large datasets in manageable chunks
    ///   * Analyze data patterns across multiple pages
    ///   * Generate reports with paginated result sets
    ///   * Implement streaming analysis of large collections
    /// 
    /// Performance Benefits:
    /// - Reduces memory footprint for large datasets
    /// - Improves response times by limiting result size
    /// - Enables efficient client-side pagination
    /// </summary>
    public class GetExamplesPaginatedQueryHandler(
        IReadRepository<Example> readRepository,
        IMapper mapper) : IRequestHandler<GetExamplesPaginatedQuery, Result<PagedResult<GetExamplesPaginatedDto>>>
    {
        public async Task<Result<PagedResult<GetExamplesPaginatedDto>>> Handle(GetExamplesPaginatedQuery request, CancellationToken cancellationToken)
        {
            var pagedResult = await readRepository.GetListPaginatedAsync(
                request.CurrentPage,
                request.PageSize,
                cancellationToken: cancellationToken);

            var mappedResults = mapper.Map<IEnumerable<GetExamplesPaginatedDto>>(pagedResult.Results);

            var result = new PagedResult<GetExamplesPaginatedDto>(
                mappedResults,
                pagedResult.RowsCount,
                pagedResult.CurrentPage,
                pagedResult.PageSize);

            return Result<PagedResult<GetExamplesPaginatedDto>>.Success(result);
        }
    }
}
