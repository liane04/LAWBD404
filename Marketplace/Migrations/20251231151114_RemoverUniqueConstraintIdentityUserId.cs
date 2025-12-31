using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marketplace.Migrations
{
    /// <inheritdoc />
    public partial class RemoverUniqueConstraintIdentityUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Utilizador_IdentityUserId",
                table: "Utilizador");

            migrationBuilder.CreateIndex(
                name: "IX_Utilizador_IdentityUserId",
                table: "Utilizador",
                column: "IdentityUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Utilizador_IdentityUserId",
                table: "Utilizador");

            migrationBuilder.CreateIndex(
                name: "IX_Utilizador_IdentityUserId",
                table: "Utilizador",
                column: "IdentityUserId",
                unique: true);
        }
    }
}
