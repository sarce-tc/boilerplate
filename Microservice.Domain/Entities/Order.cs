using Microservice.Domain.Common;

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
                throw new InvalidOperationException("Cannot cancel a completed order.");
            Status = OrderStatus.Cancelled;
        }

        public void Complete()
        {
            if (Status == OrderStatus.Cancelled)
                throw new InvalidOperationException("Cannot complete a cancelled order.");
            Status = OrderStatus.Completed;
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
