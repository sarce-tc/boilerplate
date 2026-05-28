using FluentValidation;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.ExamplesEF.Commands.CreateExample;
// Valida CreateExampleCommand antes de que llegue al handler mediante el pipeline de MediatR.
// ── Parámetros ────────────────────────────────────────────────────────────
//   · readRepository — IReadRepository<Example> (Application.Contracts.Persistence.EF): ejecuta la
//     comprobación asíncrona de unicidad de nombre (ExistsAsync) sin cargar la entidad completa.
public sealed class CreateExampleCommandValidator : AbstractValidator<CreateExampleCommand>
{
    public CreateExampleCommandValidator(IReadRepository<Example> readRepository)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MaximumLength(Example.NameMaxLength)
            .WithMessage($"Name must not exceed {Example.NameMaxLength} characters")
            .MustAsync(async (name, ct) =>
                !await readRepository.ExistsAsync(e => e.Name.ToLower() == name.ToLower(), ct))
            .WithMessage("An example with this name already exists");

        RuleFor(x => x.Description)
            .MaximumLength(Example.DescriptionMaxLength)
            .WithMessage($"Description must not exceed {Example.DescriptionMaxLength} characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleForEach(x => x.Items)
            .ChildRules(item =>
            {
                item.RuleFor(i => i.Label)
                    .NotEmpty()
                    .WithMessage("Item label is required")
                    .MaximumLength(ExampleItem.LabelMaxLength)
                    .WithMessage($"Item label must not exceed {ExampleItem.LabelMaxLength} characters");

                item.RuleFor(i => i.Quantity)
                    .GreaterThan(0)
                    .WithMessage("Item quantity must be greater than zero");
            })
            .When(x => x.Items is { Count: > 0 });
    }
}
