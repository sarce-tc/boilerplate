using FluentValidation;

namespace Microservice.Application.Features.InvoicesEF.Commands.IssueInvoice;
// Valida la emisión del comprobante antes del handler.
public sealed class IssueInvoiceCommandValidator : AbstractValidator<IssueInvoiceCommand>
{
    public IssueInvoiceCommandValidator()
    {
        RuleFor(x => x.SalePublicId)
            .NotEmpty()
            .WithMessage("SalePublicId is required");

        RuleFor(x => x.PointOfSale)
            .GreaterThan(0)
            .WithMessage("PointOfSale must be greater than zero");
    }
}
