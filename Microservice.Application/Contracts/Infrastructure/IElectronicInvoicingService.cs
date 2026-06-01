using Microservice.Domain.Entities;

namespace Microservice.Application.Contracts.Infrastructure;

/// <summary>
/// Puerto hacia el servicio de facturación electrónica AFIP/ARCA (WSAA + WSFEv1).
/// <para>
/// Abstracción de infraestructura: la implementación real firmaría el TRA con el certificado X.509,
/// obtendría el token WSAA y solicitaría el CAE vía WSFEv1. La implementación incluida es un STUB
/// (autoriza con un CAE simulado) para no acoplar el boilerplate a certificados/SOAP todavía.
/// </para>
/// </summary>
public interface IElectronicInvoicingService
{
    /// <summary>Solicita la autorización (CAE) de un comprobante al organismo fiscal.</summary>
    Task<ElectronicInvoiceResult> RequestAuthorizationAsync(
        ElectronicInvoiceRequest request,
        CancellationToken cancellationToken = default);
}

/// <summary>Datos del comprobante a autorizar.</summary>
public record ElectronicInvoiceRequest(
    InvoiceType InvoiceType,
    int PointOfSale,
    decimal Net,
    decimal Tax,
    decimal Total,
    DocumentType? CustomerDocType,
    string? CustomerDocNumber);

/// <summary>Resultado de la solicitud de autorización.</summary>
public record ElectronicInvoiceResult(
    bool Authorized,
    string? Cae,
    DateTimeOffset? CaeExpiration,
    long? InvoiceNumber,
    string? RejectionReason)
{
    public static ElectronicInvoiceResult Approved(string cae, DateTimeOffset caeExpiration, long invoiceNumber) =>
        new(true, cae, caeExpiration, invoiceNumber, null);

    public static ElectronicInvoiceResult Denied(string reason) =>
        new(false, null, null, null, reason);
}
