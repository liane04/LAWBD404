using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Marketplace.Migrations
{
    /// <inheritdoc />
    public partial class RefDataSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Tipos",
                columns: new[] { "Id", "Nome" },
                values: new object[,]
                {
                    { 1, "Carro" },
                    { 2, "Mota" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tipos_Nome",
                table: "Tipos",
                column: "Nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Modelos_Nome_MarcaId",
                table: "Modelos",
                columns: new[] { "Nome", "MarcaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Marcas_Nome",
                table: "Marcas",
                column: "Nome",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tipos_Nome",
                table: "Tipos");

            migrationBuilder.DropIndex(
                name: "IX_Modelos_Nome_MarcaId",
                table: "Modelos");

            migrationBuilder.DropIndex(
                name: "IX_Marcas_Nome",
                table: "Marcas");

            migrationBuilder.DeleteData(
                table: "Tipos",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Tipos",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
