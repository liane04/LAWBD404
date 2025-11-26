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

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
