using Microservice.Client.Features.CashRegister.Models;
using Microservice.Client.Features.CashRegister.Services;
using Microservice.Client.Shared.Results;
using Microservice.Client.Shared.State;

namespace Microservice.Client.Features.CashRegister.State;

/// <summary>
/// Observable, app-scoped state for the active cash session. The POS reads <see cref="Active"/>
/// to know whether selling is possible and which session to attach sales to. Critical/auditable
/// state, so it is the single owner of the "current session" and is unit-testable with a mock gateway.
/// </summary>
public sealed class CashSessionState(ICashGateway gateway) : ObservableState
{
    private CashSessionSummaryVm? _active;
    private bool _loading;
    private UiError? _error;

    public CashSessionSummaryVm? Active => _active;
    public bool HasActiveSession => _active is not null;
    public bool IsLoading => _loading;
    public UiError? Error => _error;

    /// <summary>Load the open sessions and adopt the first as active (single-register POS default).</summary>
    public async Task LoadActiveAsync(CancellationToken ct = default)
    {
        Set(ref _loading, true);
        _error = null;

        var result = await gateway.GetOpenSessionsAsync(ct);
        result.Match(
            sessions => { _active = sessions.FirstOrDefault(); return true; },
            err => { _error = err; return false; });

        Set(ref _loading, false);
    }

    public async Task<UiResult<CommandAck>> OpenAsync(OpenCashSessionRequest request, CancellationToken ct = default)
    {
        var result = await gateway.OpenSessionAsync(request, ct);
        if (result.IsSuccess && result.Value.Applied)
            await LoadActiveAsync(ct);
        return result;
    }
}
