// Dapper reference entity — demonstrates the Dapper path for a simple aggregate
// (no child collections). Factory Create() validates Name and Email;
// throws ArgumentException → GlobalExceptionHandler → 400.
using Microservice.Domain.Common;

namespace Microservice.Domain.Entities;

public sealed class Customer : BaseDomainModel
{
    public string  Name  { get; private set; } = string.Empty;
    public string  Email { get; private set; } = string.Empty;
    public string? Phone { get; private set; }

    private Customer() { }

    // ── Factory ──────────────────────────────────────────────────────────────

    public static Customer Create(string name, string email, string? phone = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name,  nameof(name));
        ArgumentException.ThrowIfNullOrWhiteSpace(email, nameof(email));

        return new Customer
        {
            PublicId = Guid.NewGuid(),
            Name     = name.Trim(),
            Email    = email.Trim(),
            Phone    = phone?.Trim()
        };
    }

    // ── Domain behaviour ────────────────────────────────────────────────────

    public void Update(string name, string email, string? phone)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name,  nameof(name));
        ArgumentException.ThrowIfNullOrWhiteSpace(email, nameof(email));
        Name  = name.Trim();
        Email = email.Trim();
        Phone = phone?.Trim();
    }
}
