// ═══════════════════════════════════════════════════════════════════════════
// AGENT ENTRY POINT — Domain aggregate root for Orders
// REFERENCE IMPLEMENTATION — Este aggregate es la plantilla de patrones del proyecto.
// No degradar su claridad al refactorizar: otros aggregates aprenden de él.
//
// All business rules live HERE, not in handlers.
// Key methods:
//   Order.Create(customerName)                 → factory, sets Status = "Pending"
//   order.Cancel()                             → DomainException if Completed
//   order.Complete()                           → DomainException if Cancelled
//   order.EnsureModifiable()                   → DomainException if not Pending
//   order.UpdateCustomerName(name)             → calls EnsureModifiable
//   order.AddItemForDapper(name,qty,price)     → calls EnsureModifiable, returns OrderItem
//   order.RemoveItemForDapper(itemId, items)   → calls EnsureModifiable, returns item for DELETE
//   order.RecalculateTotal(items)              → used after Dapper loads (no _items navigation)
//
// DomainException propagates to GlobalExceptionHandler → HTTP 409 Conflict.
// Always call domain methods BEFORE BeginTransactionAsync (no rollback needed on rule failure).
//
// EF path: uses _items navigation + AddItem(). Dapper path: uses AddItemForDapper().
// ═══════════════════════════════════════════════════════════════════════════
using Microservice.Domain.Common;
using Microservice.Domain.Exceptions;

namespace Microservice.Domain.Entities
{
    /// <summary>
    /// Aggregate root that represents a customer order.
    /// Designed to work with both EF Core (migrations) and Dapper (UoW transaction examples).
    /// </summary>
    public sealed class Order : BaseDomainModel
    {
        public string CustomerName { get; private set; } = string.Empty;
        public string Status      { get; private set; } = OrderStatus.Pending;
        public decimal TotalAmount { get; private set; }

        // EF navigation — not used by Dapper repositories
        private readonly List<OrderItem> _items = [];
        public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

        private Order() { }

        // ── Factory ──────────────────────────────────────────────────────────

        public static Order Create(string customerName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(customerName, nameof(customerName));

            return new Order
            {
                PublicId     = Guid.NewGuid(),
                CustomerName = customerName.Trim(),
                Status       = OrderStatus.Pending,
                TotalAmount  = 0m
            };
        }

        // ── Domain behaviour ─────────────────────────────────────────────────

        /// <summary>Adds an item and recalculates TotalAmount (domain-level, EF path).</summary>
        public OrderItem AddItem(string productName, int quantity, decimal unitPrice)
        {
            var item = OrderItem.Create(Id, productName, quantity, unitPrice);
            _items.Add(item);
            TotalAmount += item.LineTotal;
            return item;
        }

        /// <summary>Recalculates TotalAmount from persisted items (used after Dapper loads).</summary>
        public void RecalculateTotal(IEnumerable<OrderItem> items)
        {
            TotalAmount = items.Sum(i => i.LineTotal);
        }

        public void Cancel()
        {
            if (Status == OrderStatus.Completed)
                throw new DomainException("Cannot cancel a completed order.");
            Status = OrderStatus.Cancelled;
        }

        public void Complete()
        {
            if (Status == OrderStatus.Cancelled)
                throw new DomainException("Cannot complete a cancelled order.");
            Status = OrderStatus.Completed;
        }

        // ── Dapper write-path helpers ────────────────────────────────────────
        // These methods are called BEFORE BeginTransactionAsync so a DomainException
        // propagates to GlobalExceptionHandler without needing a rollback.

        /// <summary>
        /// Throws <see cref="DomainException"/> if the order is not in <c>Pending</c> status.
        /// Call before any mutation that requires an open order.
        /// </summary>
        public void EnsureModifiable()
        {
            if (Status != OrderStatus.Pending)
                throw new DomainException($"Cannot modify an order in '{Status}' status.");
        }

        /// <summary>
        /// Changes the customer name. Order must be Pending.
        /// </summary>
        public void UpdateCustomerName(string customerName)
        {
            EnsureModifiable();
            ArgumentException.ThrowIfNullOrWhiteSpace(customerName, nameof(customerName));
            CustomerName = customerName.Trim();
        }

        /// <summary>
        /// Dapper write path: creates an <see cref="OrderItem"/>, adds its LineTotal to
        /// <see cref="TotalAmount"/>, and returns the item for persistence.
        /// Order must be Pending.
        /// </summary>
        public OrderItem AddItemForDapper(string productName, int quantity, decimal unitPrice)
        {
            EnsureModifiable();
            var item = OrderItem.CreateForOrder(Id, productName, quantity, unitPrice);
            TotalAmount += item.LineTotal;
            return item;
        }

        /// <summary>
        /// Dapper write path: finds the item in <paramref name="currentItems"/>, subtracts
        /// its LineTotal from <see cref="TotalAmount"/>, and returns it so the handler can
        /// delete the DB row. Order must be Pending.
        /// </summary>
        public OrderItem RemoveItemForDapper(Guid itemPublicId, IReadOnlyList<OrderItem> currentItems)
        {
            EnsureModifiable();
            var item = currentItems.FirstOrDefault(i => i.PublicId == itemPublicId)
                ?? throw new DomainException($"Item '{itemPublicId}' does not belong to this order.");
            TotalAmount -= item.LineTotal;
            return item;
        }
    }

    /// <summary>Allowed values for Order.Status.</summary>
    public static class OrderStatus
    {
        public const string Pending   = "Pending";
        public const string Completed = "Completed";
        public const string Cancelled = "Cancelled";
    }
}
