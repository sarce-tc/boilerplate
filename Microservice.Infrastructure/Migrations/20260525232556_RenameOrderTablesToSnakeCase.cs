using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microservice.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameOrderTablesToSnakeCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Orders_OrderId",
                table: "OrderItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Orders",
                table: "Orders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderItems",
                table: "OrderItems");

            migrationBuilder.RenameTable(
                name: "Orders",
                newName: "orders");

            migrationBuilder.RenameTable(
                name: "OrderItems",
                newName: "order_items");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "orders",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "orders",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "orders",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "TotalAmount",
                table: "orders",
                newName: "total_amount");

            migrationBuilder.RenameColumn(
                name: "PublicId",
                table: "orders",
                newName: "public_id");

            migrationBuilder.RenameColumn(
                name: "CustomerName",
                table: "orders",
                newName: "customer_name");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "orders",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_PublicId",
                table: "orders",
                newName: "IX_orders_public_id");

            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "order_items",
                newName: "quantity");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "order_items",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "order_items",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "UnitPrice",
                table: "order_items",
                newName: "unit_price");

            migrationBuilder.RenameColumn(
                name: "PublicId",
                table: "order_items",
                newName: "public_id");

            migrationBuilder.RenameColumn(
                name: "ProductName",
                table: "order_items",
                newName: "product_name");

            migrationBuilder.RenameColumn(
                name: "OrderId",
                table: "order_items",
                newName: "order_id");

            migrationBuilder.RenameColumn(
                name: "LineTotal",
                table: "order_items",
                newName: "line_total");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "order_items",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItems_PublicId",
                table: "order_items",
                newName: "IX_order_items_public_id");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItems_OrderId",
                table: "order_items",
                newName: "IX_order_items_order_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_orders",
                table: "orders",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_order_items",
                table: "order_items",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_order_items_orders_order_id",
                table: "order_items",
                column: "order_id",
                principalTable: "orders",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_order_items_orders_order_id",
                table: "order_items");

            migrationBuilder.DropPrimaryKey(
                name: "PK_orders",
                table: "orders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_order_items",
                table: "order_items");

            migrationBuilder.RenameTable(
                name: "orders",
                newName: "Orders");

            migrationBuilder.RenameTable(
                name: "order_items",
                newName: "OrderItems");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "Orders",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Orders",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "Orders",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "total_amount",
                table: "Orders",
                newName: "TotalAmount");

            migrationBuilder.RenameColumn(
                name: "public_id",
                table: "Orders",
                newName: "PublicId");

            migrationBuilder.RenameColumn(
                name: "customer_name",
                table: "Orders",
                newName: "CustomerName");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Orders",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_orders_public_id",
                table: "Orders",
                newName: "IX_Orders_PublicId");

            migrationBuilder.RenameColumn(
                name: "quantity",
                table: "OrderItems",
                newName: "Quantity");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "OrderItems",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "OrderItems",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "unit_price",
                table: "OrderItems",
                newName: "UnitPrice");

            migrationBuilder.RenameColumn(
                name: "public_id",
                table: "OrderItems",
                newName: "PublicId");

            migrationBuilder.RenameColumn(
                name: "product_name",
                table: "OrderItems",
                newName: "ProductName");

            migrationBuilder.RenameColumn(
                name: "order_id",
                table: "OrderItems",
                newName: "OrderId");

            migrationBuilder.RenameColumn(
                name: "line_total",
                table: "OrderItems",
                newName: "LineTotal");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "OrderItems",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_order_items_public_id",
                table: "OrderItems",
                newName: "IX_OrderItems_PublicId");

            migrationBuilder.RenameIndex(
                name: "IX_order_items_order_id",
                table: "OrderItems",
                newName: "IX_OrderItems_OrderId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Orders",
                table: "Orders",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderItems",
                table: "OrderItems",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Orders_OrderId",
                table: "OrderItems",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
