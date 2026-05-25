using Microservice.Domain.Common;

namespace Microservice.Domain.Entities
{
    /// <summary>
    /// Line item belonging to an Order.
    /// LineTotal is a domain-computed value; it is also stored in the DB column
    /// so Dapper can read it directly without recalculating.
    /// </summary>
    public sealed class OrderItem : BaseDomainModel
    {
        public int     OrderId     { get; private set; }
        public string  ProductName { get; private set; } = string.Empty;
        public int     Quantity    { get; private set; }
        public decimal UnitPrice   { get; private set; }
        public decimal LineTotal   { get; private set; }

        // EF navigation — not used by Dapper repositories
        public Order? Order { get; private set; }

        private OrderItem() { }

        // ── Factory ──────────────────────────────────────────────────────────

        /// <summary>
        /// Creates a new OrderItem.
        /// <paramref name="orderId"/> is 0 when the parent Order has not been persisted yet;
        /// Dapper sets it explicitly after INSERT … RETURNING id on the Order row.
        /// </summary>
        internal static OrderItem Create(int orderId, string productName, int quantity, decimal unitPrice)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(productName, nameof(productName));
            if (quantity  <= 0) throw new ArgumentException("Quantity must be positive.",  nameof(quantity));
            if (unitPrice <= 0) throw new ArgumentException("UnitPrice must be positive.", nameof(unitPrice));

            var lineTotal = quantity * unitPrice;

            return new OrderItem
            {
                PublicId    = Guid.NewGuid(),
                OrderId     = orderId,
                ProductName = productName.Trim(),
                Quantity    = quantity,
                UnitPrice   = unitPrice,
                LineTotal   = lineTotal
            };
        }

        /// <summary>
        /// Public overload used by Dapper handlers that already know the orderId.
        /// </summary>
        public static OrderItem CreateForOrder(int orderId, string productName, int quantity, decimal unitPrice)
            => Create(orderId, productName, quantity, unitPrice);
    }
}
