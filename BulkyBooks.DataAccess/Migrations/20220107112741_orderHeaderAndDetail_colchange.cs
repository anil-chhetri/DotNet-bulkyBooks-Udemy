using Microsoft.EntityFrameworkCore.Migrations;

namespace BulkyBooks.Web.Migrations
{
    public partial class orderHeaderAndDetail_colchange : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ShippingOrder",
                table: "orderHeader",
                newName: "ShippingDate");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ShippingDate",
                table: "orderHeader",
                newName: "ShippingOrder");
        }
    }
}
