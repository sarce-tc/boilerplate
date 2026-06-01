using Microservice.Application.Contracts.Infrastructure;

namespace Microservice.Infrastructure.Services;

/// <summary>
/// STUB de <see cref="IElectronicInvoicingService"/>: simula la autorización de AFIP devolviendo
/// un CAE ficticio y un número incremental basado en tiempo. NO se conecta a WSAA/WSFEv1.
/// <para>
/// Reemplazar por la implementación real (firma de TRA con certificado X.509 + cliente SOAP WSFEv1)
/// cuando se disponga de certificados de homologación/producción. La interfaz no cambia.
/// </para>
/// </summary>
public sealed class StubElectronicInvoicingService : IElectronicInvoicingService
{
    public Task<ElectronicInvoiceResult> RequestAuthorizationAsync(
        ElectronicInvoiceRequest request,
        CancellationToken cancellationToken = default)
    {
        // CAE simulado: 14 dígitos. Número de comprobante simulado: derivado del timestamp.
        var cae = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
        var invoiceNumber = DateTimeOffset.UtcNow.ToUnixTimeSeconds() % 100_000_000;
        var caeExpiration = DateTimeOffset.UtcNow.AddDays(10);

        var result = ElectronicInvoiceResult.Approved(cae, caeExpiration, invoiceNumber);
        return Task.FromResult(result);
    }
}
