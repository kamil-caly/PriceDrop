using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PriceDropApi.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMediaExpertColumns_AddMoreleColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MediaExpertPrice",
                table: "Products",
                newName: "MorelePrice");

            migrationBuilder.RenameColumn(
                name: "MediaExpertLink",
                table: "Products",
                newName: "MoreleLink");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MorelePrice",
                table: "Products",
                newName: "MediaExpertPrice");

            migrationBuilder.RenameColumn(
                name: "MoreleLink",
                table: "Products",
                newName: "MediaExpertLink");
        }
    }
}
