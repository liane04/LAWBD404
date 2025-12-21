using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marketplace.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFiltrosFavoritosTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AnoMax",
                table: "FiltrosFavoritos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AnoMin",
                table: "FiltrosFavoritos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Ativo",
                table: "FiltrosFavoritos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Caixa",
                table: "FiltrosFavoritos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CombustivelId",
                table: "FiltrosFavoritos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "FiltrosFavoritos",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "KmMax",
                table: "FiltrosFavoritos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastCheckedAt",
                table: "FiltrosFavoritos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Localizacao",
                table: "FiltrosFavoritos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MarcaId",
                table: "FiltrosFavoritos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxAnuncioIdNotificado",
                table: "FiltrosFavoritos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ModeloId",
                table: "FiltrosFavoritos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nome",
                table: "FiltrosFavoritos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecoMax",
                table: "FiltrosFavoritos",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TipoId",
                table: "FiltrosFavoritos",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnoMax",
                table: "FiltrosFavoritos");

            migrationBuilder.DropColumn(
                name: "AnoMin",
                table: "FiltrosFavoritos");

            migrationBuilder.DropColumn(
                name: "Ativo",
                table: "FiltrosFavoritos");

            migrationBuilder.DropColumn(
                name: "Caixa",
                table: "FiltrosFavoritos");

            migrationBuilder.DropColumn(
                name: "CombustivelId",
                table: "FiltrosFavoritos");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "FiltrosFavoritos");

            migrationBuilder.DropColumn(
                name: "KmMax",
                table: "FiltrosFavoritos");

            migrationBuilder.DropColumn(
                name: "LastCheckedAt",
                table: "FiltrosFavoritos");

            migrationBuilder.DropColumn(
                name: "Localizacao",
                table: "FiltrosFavoritos");

            migrationBuilder.DropColumn(
                name: "MarcaId",
                table: "FiltrosFavoritos");

            migrationBuilder.DropColumn(
                name: "MaxAnuncioIdNotificado",
                table: "FiltrosFavoritos");

            migrationBuilder.DropColumn(
                name: "ModeloId",
                table: "FiltrosFavoritos");

            migrationBuilder.DropColumn(
                name: "Nome",
                table: "FiltrosFavoritos");

            migrationBuilder.DropColumn(
                name: "PrecoMax",
                table: "FiltrosFavoritos");

            migrationBuilder.DropColumn(
                name: "TipoId",
                table: "FiltrosFavoritos");
        }
    }
}
