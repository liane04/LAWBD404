using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marketplace.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarIndicesPerformance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reservas_CompradorId",
                table: "Reservas");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_CompradorId_Estado",
                table: "Reservas",
                columns: new[] { "CompradorId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_Anuncios_Destacado_DestaqueAte",
                table: "Anuncios",
                columns: new[] { "Destacado", "DestaqueAte" });

            migrationBuilder.CreateIndex(
                name: "IX_Anuncios_Estado",
                table: "Anuncios",
                column: "Estado");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reservas_CompradorId_Estado",
                table: "Reservas");

            migrationBuilder.DropIndex(
                name: "IX_Anuncios_Destacado_DestaqueAte",
                table: "Anuncios");

            migrationBuilder.DropIndex(
                name: "IX_Anuncios_Estado",
                table: "Anuncios");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_CompradorId",
                table: "Reservas",
                column: "CompradorId");
        }
    }
}
