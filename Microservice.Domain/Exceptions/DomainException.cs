namespace Microservice.Domain.Exceptions;

/// <summary>
/// Thrown when a domain operation violates a business invariant.
///
/// Examples:
/// <list type="bullet">
///   <item>Cancelling an order that is already <c>Completed</c>.</item>
///   <item>Completing an order that is already <c>Cancelled</c>.</item>
/// </list>
///
/// <b>Pipeline contract:</b> <c>GlobalExceptionHandler</c> maps this exception
/// to HTTP 409 Conflict and logs it at <c>Warning</c> level — it is an expected,
/// known business-rule rejection, not a system error.
/// </summary>
public sealed class DomainException(string message) : Exception(message);
