using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace broodjeszaak.Migrations
{
    public partial class AddIdentity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_producten_ProductId",
                table: "OrderDetails");

            migrationBuilder.DropIndex(
                name: "IX_OrderDetails_ProductId",
                table: "OrderDetails");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_ProductId",
                table: "OrderDetails",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_producten_ProductId",
                table: "OrderDetails",
                column: "ProductId",
                principalTable: "producten",
                principalColumn: "ProductID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
