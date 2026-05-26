using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs.Customers;

namespace Microservice.Application.Features.Customers.Queries.GetCustomerByEmail;

public record GetCustomerByEmailQuery(string Email) : IRequest<Result<CustomerDto>>;
