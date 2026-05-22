using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Domain.Entities;
using Microservice.Application.Contracts.Persistence.EF;

namespace Microservice.Application.Features.Examples.Queries.CountExamples
{
    /// <summary>
    /// Use Case: Get total count of all Example records in the database.
    /// 
    /// When to use:
    /// - Displaying total record count in UI or dashboards
    /// - Calculating statistics and metrics
    /// - Monitoring data volume growth over time
    /// - AI agents analyzing dataset characteristics
    /// - Pagination calculations (total pages = count / pageSize)
    /// 
    /// Responsibilities:
    /// - Query database for total entity count
    /// - Return count efficiently
    /// 
    /// Performance:
    /// - Efficient COUNT(*) query on server side
    /// - No data transfer, only integer result
    /// - Single database round trip
    /// 
    /// AI Agent Use Cases:
    /// - Analyze dataset size before processing
    /// - Monitor growth rate of data
    /// - Make decisions based on volume thresholds
    /// - Generate statistics about data characteristics
    /// 
    /// Use in Combination:
    /// - Often paired with pagination queries to determine total pages
    /// - Combined with filtered counts for analytics
    /// </summary>
    public class CountExamplesQueryHandler(
                IReadRepository<Example> readRepository
        ) : IRequestHandler<CountExamplesQuery, Result<int>>
    {
        public async Task<Result<int>> Handle(CountExamplesQuery request, CancellationToken cancellationToken)
        {
            var count = await readRepository.CountAsync(cancellationToken);
            return Result<int>.Success(count);
        }
    }
}
