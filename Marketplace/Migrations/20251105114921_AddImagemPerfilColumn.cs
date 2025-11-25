using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marketplace.Migrations
{
    /// <inheritdoc />
    public partial class AddImagemPerfilColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagemPerfil",
                table: "Utilizador",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Cilindrada",
                table: "Anuncios",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Localizacao",
                table: "Anuncios",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Lugares",
                table: "Anuncios",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Portas",
                table: "Anuncios",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Potencia",
                table: "Anuncios",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Valor_sinal",
                table: "Anuncios",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "n_visualizacoes",
                table: "Anuncios",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Extras",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descricao = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Extras", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AnuncioExtras",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnuncioId = table.Column<int>(type: "int", nullable: false),
                    ExtraId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnuncioExtras", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnuncioExtras_Anuncios_AnuncioId",
                        column: x => x.AnuncioId,
                        principalTable: "Anuncios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnuncioExtras_Extras_ExtraId",
                        column: x => x.ExtraId,
                        principalTable: "Extras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnuncioExtras_AnuncioId",
                table: "AnuncioExtras",
                column: "AnuncioId");

            migrationBuilder.CreateIndex(
                name: "IX_AnuncioExtras_ExtraId",
                table: "AnuncioExtras",
                column: "ExtraId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnuncioExtras");

            migrationBuilder.DropTable(
                name: "Extras");

            migrationBuilder.DropColumn(
                name: "ImagemPerfil",
                table: "Utilizador");

            migrationBuilder.DropColumn(
                name: "Cilindrada",
                table: "Anuncios");

            migrationBuilder.DropColumn(
                name: "Localizacao",
                table: "Anuncios");

            migrationBuilder.DropColumn(
                name: "Lugares",
                table: "Anuncios");

            migrationBuilder.DropColumn(
                name: "Portas",
                table: "Anuncios");

            migrationBuilder.DropColumn(
                name: "Potencia",
                table: "Anuncios");

            migrationBuilder.DropColumn(
                name: "Valor_sinal",
                table: "Anuncios");

            migrationBuilder.DropColumn(
                name: "n_visualizacoes",
                table: "Anuncios");
        }
    }
}
