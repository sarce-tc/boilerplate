using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs.Customers;

namespace Microservice.Application.Features.Customers.Queries.GetCustomerById;

public record GetCustomerByIdQuery(Guid PublicId) : IRequest<Result<CustomerDto>>;
