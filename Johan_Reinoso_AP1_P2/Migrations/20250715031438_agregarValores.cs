using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Johan_Reinoso_AP1_P2.Migrations
{
    /// <inheritdoc />
    public partial class agregarValores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "productos",
                keyColumn: "ProductoId",
                keyValue: 1,
                column: "Peso",
                value: 5m);

            migrationBuilder.UpdateData(
                table: "productos",
                keyColumn: "ProductoId",
                keyValue: 2,
                column: "Peso",
                value: 10m);

            migrationBuilder.UpdateData(
                table: "productos",
                keyColumn: "ProductoId",
                keyValue: 3,
                column: "Peso",
                value: 8m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "productos",
                keyColumn: "ProductoId",
                keyValue: 1,
                column: "Peso",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "productos",
                keyColumn: "ProductoId",
                keyValue: 2,
                column: "Peso",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "productos",
                keyColumn: "ProductoId",
                keyValue: 3,
                column: "Peso",
                value: 0m);
        }
    }
}
