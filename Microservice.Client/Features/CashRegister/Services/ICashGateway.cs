using Microservice.Client.Features.CashRegister.Models;
using Microservice.Client.Shared.Contracts;
using Microservice.Client.Shared.Results;

namespace Microservice.Client.Features.CashRegister.Services;

/// <summary>
/// Gateway for cash sessions: open, register movements, close (arqueo) and read.
/// Append-only operations (open, movement) may queue offline; close is a server-orchestrated
/// arqueo and runs online only.
/// </summary>
public interface ICashGateway
{
    Task<UiResult<IReadOnlyList<CashSessionSummaryVm>>> GetOpenSessionsAsync(CancellationToken ct = default);
    Task<UiResult<CashSessionDetailVm>> GetByIdAsync(Guid publicId, CancellationToken ct = default);
    Task<UiResult<PagedResult<CashSessionSummaryVm>>> GetPagedAsync(PageRequest request, CancellationToken ct = default);

    Task<UiResult<CommandAck>> OpenSessionAsync(OpenCashSessionRequest request, CancellationToken ct = default);
    Task<UiResult<CommandAck>> RegisterMovementAsync(Guid sessionPublicId, RegisterCashMovementRequest request, CancellationToken ct = default);

    /// <summary>Close the session and return the arqueo summary (expected vs declared).</summary>
    Task<UiResult<CashCloseResultVm>> CloseSessionAsync(Guid sessionPublicId, CloseCashSessionRequest request, CancellationToken ct = default);
}
