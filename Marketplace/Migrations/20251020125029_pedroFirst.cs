using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marketplace.Migrations
{
    /// <inheritdoc />
    public partial class pedroFirst : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Modelos_Marcas_MarcaId",
                table: "Modelos");

            migrationBuilder.AlterColumn<int>(
                name: "MarcaId",
                table: "Modelos",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TipoId",
                table: "Modelos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Modelos_TipoId",
                table: "Modelos",
                column: "TipoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Modelos_Marcas_MarcaId",
                table: "Modelos",
                column: "MarcaId",
                principalTable: "Marcas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Modelos_Tipos_TipoId",
                table: "Modelos",
                column: "TipoId",
                principalTable: "Tipos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Modelos_Marcas_MarcaId",
                table: "Modelos");

            migrationBuilder.DropForeignKey(
                name: "FK_Modelos_Tipos_TipoId",
                table: "Modelos");

            migrationBuilder.DropIndex(
                name: "IX_Modelos_TipoId",
                table: "Modelos");

            migrationBuilder.DropColumn(
                name: "TipoId",
                table: "Modelos");

            migrationBuilder.AlterColumn<int>(
                name: "MarcaId",
                table: "Modelos",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Modelos_Marcas_MarcaId",
                table: "Modelos",
                column: "MarcaId",
                principalTable: "Marcas",
                principalColumn: "Id");
        }
    }
}
