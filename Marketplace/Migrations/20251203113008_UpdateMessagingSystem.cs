using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marketplace.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMessagingSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Lida",
                table: "Mensagens",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "RemetenteId",
                table: "Mensagens",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UltimaMensagemData",
                table: "Conversas",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Mensagens_RemetenteId",
                table: "Mensagens",
                column: "RemetenteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Mensagens_AspNetUsers_RemetenteId",
                table: "Mensagens",
                column: "RemetenteId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mensagens_AspNetUsers_RemetenteId",
                table: "Mensagens");

            migrationBuilder.DropIndex(
                name: "IX_Mensagens_RemetenteId",
                table: "Mensagens");

            migrationBuilder.DropColumn(
                name: "Lida",
                table: "Mensagens");

            migrationBuilder.DropColumn(
                name: "RemetenteId",
                table: "Mensagens");

            migrationBuilder.DropColumn(
                name: "UltimaMensagemData",
                table: "Conversas");
        }
    }
}
