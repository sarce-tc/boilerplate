using MediatR;
using Microservice.Application.Common;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs.Customers;

namespace Microservice.Application.Features.Customers.Queries.GetCustomers;

public record GetCustomersQuery(int Page, int PageSize) : IRequest<Result<PagedResult<CustomerSummaryDto>>>;
