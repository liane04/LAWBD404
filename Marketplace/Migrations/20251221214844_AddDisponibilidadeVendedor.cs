using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marketplace.Migrations
{
    /// <inheritdoc />
    public partial class AddDisponibilidadeVendedor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DisponibilidadesVendedor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VendedorId = table.Column<int>(type: "int", nullable: false),
                    DiaSemana = table.Column<int>(type: "int", nullable: false),
                    HoraInicio = table.Column<TimeSpan>(type: "time", nullable: false),
                    HoraFim = table.Column<TimeSpan>(type: "time", nullable: false),
                    IntervaloMinutos = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DisponibilidadesVendedor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DisponibilidadesVendedor_Utilizador_VendedorId",
                        column: x => x.VendedorId,
                        principalTable: "Utilizador",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DisponibilidadesVendedor_VendedorId",
                table: "DisponibilidadesVendedor",
                column: "VendedorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DisponibilidadesVendedor");
        }
    }
}
