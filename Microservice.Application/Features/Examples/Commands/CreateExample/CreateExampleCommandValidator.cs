using FluentValidation;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.Examples.Commands.CreateExample
{
    // Invoked by ValidationBehaviour before the handler; failures → Result.Failure (not exception)
    // Name: required, unique (async DB check), max 200 chars. Description: optional, max 1000 chars.
    public class CreateExampleCommandValidator : AbstractValidator<CreateExampleCommand>
    {
        private readonly IReadRepository<Example> _readRepository;
        public CreateExampleCommandValidator(IReadRepository<Example> readRepository)
        {
            _readRepository = readRepository;

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Name is required")
                .MaximumLength(200)
                .WithMessage("Name must not exceed 200 characters")
                // Async uniqueness check — runs after the sync rules above
                .MustAsync(async (name, cancellationToken) =>
                !await _readRepository.ExistsAsync(
                    e => e.Name.ToLower() == name.ToLower(),
                    cancellationToken))
                .WithMessage("An example with this name already exists");

            RuleFor(x => x.Description)
                .MaximumLength(1000)
                .WithMessage("Description must not exceed 1000 characters")
                .When(x => !string.IsNullOrEmpty(x.Description));


        }
    }
}
