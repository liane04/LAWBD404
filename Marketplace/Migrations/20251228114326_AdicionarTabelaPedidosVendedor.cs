using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marketplace.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarTabelaPedidosVendedor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PedidosVendedor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompradorId = table.Column<int>(type: "int", nullable: false),
                    Nif = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: false),
                    DadosFaturacao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Motivacao = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DataPedido = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataResposta = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AdminRespondeuId = table.Column<int>(type: "int", nullable: true),
                    MotivoRejeicao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PedidosVendedor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PedidosVendedor_Utilizador_CompradorId",
                        column: x => x.CompradorId,
                        principalTable: "Utilizador",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PedidosVendedor_CompradorId",
                table: "PedidosVendedor",
                column: "CompradorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PedidosVendedor");
        }
    }
}
