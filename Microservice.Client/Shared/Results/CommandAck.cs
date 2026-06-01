namespace Microservice.Client.Shared.Results;

/// <summary>
/// Acknowledgement of a mutation submitted through a gateway. Distinguishes a command that
/// was <b>applied online</b> (server confirmed, <see cref="ResourceId"/> known) from one that
/// was <b>queued</b> for later sync (offline / network race). The UI uses this to show
/// "guardado" vs "se sincronizará al reconectar" without inspecting transport details.
/// </summary>
public readonly record struct CommandAck(bool Queued, Guid? ResourceId = null)
{
    public bool Applied => !Queued;
}
