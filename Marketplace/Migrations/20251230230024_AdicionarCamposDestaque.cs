using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marketplace.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarCamposDestaque : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataDestaque",
                table: "Anuncios",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Destacado",
                table: "Anuncios",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DestaqueAte",
                table: "Anuncios",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataDestaque",
                table: "Anuncios");

            migrationBuilder.DropColumn(
                name: "Destacado",
                table: "Anuncios");

            migrationBuilder.DropColumn(
                name: "DestaqueAte",
                table: "Anuncios");
        }
    }
}
