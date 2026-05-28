using FluentValidation;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.ExamplesEF.Commands.UpdateExample;
public sealed class UpdateExampleCommandValidator : AbstractValidator<UpdateExampleCommand>
{
    public UpdateExampleCommandValidator()
    {
        RuleFor(x => x.PublicId)
            .NotEmpty()
            .WithMessage("PublicId is required")
            .WithErrorCode("PublicIdInvalid")
            .WithSeverity(Severity.Error);

        RuleFor(x => x.Name)
            .MaximumLength(Example.NameMaxLength)
            .WithMessage($"Name must not exceed {Example.NameMaxLength} characters")
            .When(x => x.Name is not null);

        RuleFor(x => x.Description)
            .MaximumLength(Example.DescriptionMaxLength)
            .WithMessage($"Description must not exceed {Example.DescriptionMaxLength} characters")
            .When(x => x.Description is not null);

        RuleForEach(x => x.AddItems)
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
            .When(x => x.AddItems is { Count: > 0 });

        RuleForEach(x => x.RemoveItemIds)
            .NotEmpty()
            .WithMessage("RemoveItemIds must not contain empty GUIDs")
            .When(x => x.RemoveItemIds is { Count: > 0 });

        RuleForEach(x => x.CompleteItemIds)
            .NotEmpty()
            .WithMessage("CompleteItemIds must not contain empty GUIDs")
            .When(x => x.CompleteItemIds is { Count: > 0 });
    }
}
