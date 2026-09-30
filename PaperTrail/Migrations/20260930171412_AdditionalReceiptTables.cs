using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaperTrail.Migrations
{
    /// <inheritdoc />
    public partial class AdditionalReceiptTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptItem_Product_ProductId",
                table: "ReceiptItem");

            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptItem_Receipts_ReceiptId",
                table: "ReceiptItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReceiptItem",
                table: "ReceiptItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Product",
                table: "Product");

            migrationBuilder.RenameTable(
                name: "ReceiptItem",
                newName: "ReceiptItems");

            migrationBuilder.RenameTable(
                name: "Product",
                newName: "Products");

            migrationBuilder.RenameIndex(
                name: "IX_ReceiptItem_ReceiptId",
                table: "ReceiptItems",
                newName: "IX_ReceiptItems_ReceiptId");

            migrationBuilder.RenameIndex(
                name: "IX_ReceiptItem_ProductId",
                table: "ReceiptItems",
                newName: "IX_ReceiptItems_ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReceiptItems",
                table: "ReceiptItems",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Products",
                table: "Products",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReceiptItems_Products_ProductId",
                table: "ReceiptItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReceiptItems_Receipts_ReceiptId",
                table: "ReceiptItems",
                column: "ReceiptId",
                principalTable: "Receipts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptItems_Products_ProductId",
                table: "ReceiptItems");

            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptItems_Receipts_ReceiptId",
                table: "ReceiptItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReceiptItems",
                table: "ReceiptItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Products",
                table: "Products");

            migrationBuilder.RenameTable(
                name: "ReceiptItems",
                newName: "ReceiptItem");

            migrationBuilder.RenameTable(
                name: "Products",
                newName: "Product");

            migrationBuilder.RenameIndex(
                name: "IX_ReceiptItems_ReceiptId",
                table: "ReceiptItem",
                newName: "IX_ReceiptItem_ReceiptId");

            migrationBuilder.RenameIndex(
                name: "IX_ReceiptItems_ProductId",
                table: "ReceiptItem",
                newName: "IX_ReceiptItem_ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReceiptItem",
                table: "ReceiptItem",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Product",
                table: "Product",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReceiptItem_Product_ProductId",
                table: "ReceiptItem",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReceiptItem_Receipts_ReceiptId",
                table: "ReceiptItem",
                column: "ReceiptId",
                principalTable: "Receipts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
