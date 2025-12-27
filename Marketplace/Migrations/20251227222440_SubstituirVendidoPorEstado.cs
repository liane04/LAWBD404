using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marketplace.Migrations
{
    /// <inheritdoc />
    public partial class SubstituirVendidoPorEstado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Adicionar nova coluna Estado com valor padrão "Ativo"
            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "Anuncios",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Ativo");

            // 2. Migrar dados: Vendido = 1 → Estado = "Vendido", Vendido = 0 → Estado = "Ativo"
            migrationBuilder.Sql(@"
                UPDATE Anuncios
                SET Estado = CASE
                    WHEN Vendido = 1 THEN 'Vendido'
                    ELSE 'Ativo'
                END
            ");

            // 3. Remover coluna antiga Vendido
            migrationBuilder.DropColumn(
                name: "Vendido",
                table: "Anuncios");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Anuncios");

            migrationBuilder.AddColumn<bool>(
                name: "Vendido",
                table: "Anuncios",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
