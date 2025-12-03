using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marketplace.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVisitaModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Visitas_Utilizador_VendedorId",
                table: "Visitas");

            migrationBuilder.AlterColumn<string>(
                name: "Estado",
                table: "Visitas",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AnuncioId",
                table: "Visitas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CompradorId",
                table: "Visitas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataAtualizacao",
                table: "Visitas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataCriacao",
                table: "Visitas",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Observacoes",
                table: "Visitas",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Visitas_AnuncioId",
                table: "Visitas",
                column: "AnuncioId");

            migrationBuilder.CreateIndex(
                name: "IX_Visitas_CompradorId",
                table: "Visitas",
                column: "CompradorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Visitas_Anuncios_AnuncioId",
                table: "Visitas",
                column: "AnuncioId",
                principalTable: "Anuncios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Visitas_Utilizador_CompradorId",
                table: "Visitas",
                column: "CompradorId",
                principalTable: "Utilizador",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Visitas_Utilizador_VendedorId",
                table: "Visitas",
                column: "VendedorId",
                principalTable: "Utilizador",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Visitas_Anuncios_AnuncioId",
                table: "Visitas");

            migrationBuilder.DropForeignKey(
                name: "FK_Visitas_Utilizador_CompradorId",
                table: "Visitas");

            migrationBuilder.DropForeignKey(
                name: "FK_Visitas_Utilizador_VendedorId",
                table: "Visitas");

            migrationBuilder.DropIndex(
                name: "IX_Visitas_AnuncioId",
                table: "Visitas");

            migrationBuilder.DropIndex(
                name: "IX_Visitas_CompradorId",
                table: "Visitas");

            migrationBuilder.DropColumn(
                name: "AnuncioId",
                table: "Visitas");

            migrationBuilder.DropColumn(
                name: "CompradorId",
                table: "Visitas");

            migrationBuilder.DropColumn(
                name: "DataAtualizacao",
                table: "Visitas");

            migrationBuilder.DropColumn(
                name: "DataCriacao",
                table: "Visitas");

            migrationBuilder.DropColumn(
                name: "Observacoes",
                table: "Visitas");

            migrationBuilder.AlterColumn<string>(
                name: "Estado",
                table: "Visitas",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AddForeignKey(
                name: "FK_Visitas_Utilizador_VendedorId",
                table: "Visitas",
                column: "VendedorId",
                principalTable: "Utilizador",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
