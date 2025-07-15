using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Johan_Reinoso_AP1_P2.Migrations
{
    /// <inheritdoc />
    public partial class productos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Existencias",
                table: "productos",
                newName: "Existencia");

            migrationBuilder.AlterColumn<decimal>(
                name: "Peso",
                table: "productos",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.InsertData(
                table: "productos",
                columns: new[] { "ProductoId", "Descripcion", "EsCompuesto", "Existencia", "Peso" },
                values: new object[,]
                {
                    { 1, "Maní", false, 100, 0m },
                    { 2, "Pistachos", false, 100, 0m },
                    { 3, "Almendras", false, 100, 0m },
                    { 4, "Frutos Mixtos 200gr", true, 0, 200.00m },
                    { 5, "Frutos Mixtos 400gr", true, 0, 400.00m },
                    { 6, "Frutos Mixtos 600gr", true, 0, 600.00m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "productos",
                keyColumn: "ProductoId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "productos",
                keyColumn: "ProductoId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "productos",
                keyColumn: "ProductoId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "productos",
                keyColumn: "ProductoId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "productos",
                keyColumn: "ProductoId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "productos",
                keyColumn: "ProductoId",
                keyValue: 6);

            migrationBuilder.RenameColumn(
                name: "Existencia",
                table: "productos",
                newName: "Existencias");

            migrationBuilder.AlterColumn<double>(
                name: "Peso",
                table: "productos",
                type: "float",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
        }
    }
}
