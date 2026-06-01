using Microservice.Client.Shared.Results;

namespace Microservice.Client.Infrastructure.Auth;

/// <summary>Login/logout orchestration. The only component-facing auth API.</summary>
public interface IAuthService
{
    Task<UiResult> LoginAsync(string username, string password, CancellationToken ct = default);
    Task LogoutAsync();
}
