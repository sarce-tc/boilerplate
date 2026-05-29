namespace Microservice.Domain.ValueObjects;
// ═══════════════════════════════════════════════════════════════════════
// AGENT — Base class for all domain entities.
//
// Id        (int)  — internal PK, never exposed in API responses
// PublicId  (Guid) — stable external identifier, used in all API routes
// CreatedAt / UpdatedAt — set by the entity factory or EF; never null
// ═══════════════════════════════════════════════════════════════════════
public class BaseDomainModel
{
    public int Id { get; set; }
    public Guid PublicId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
