using MediatR;
using Microservice.Application.Common.Results;

namespace Microservice.Application.Features.ExamplesEF.Commands.ActivateExample;

/// <summary>
/// Transitions an <c>Example</c> from <c>Inactive</c> to <c>Active</c> state.
/// Returns the <see cref="Guid"/> of the reactivated aggregate.
/// </summary>
public record ActivateExampleCommand(Guid PublicId) : IRequest<Result<Guid>>;
