using MediatR;
using Microservice.Application.Common.Results;

namespace Microservice.Application.Features.ExamplesEF.Commands.ExecuteSql;
public record ExecuteSqlCommand(
    FormattableString Sql
) : IRequest<Result<int>>;
