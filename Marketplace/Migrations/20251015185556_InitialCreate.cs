using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marketplace.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Combustiveis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tipo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Combustiveis", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Marcas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Marcas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Moradas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodigoPostal = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Localidade = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Rua = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Moradas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tipos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tipos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Modelos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MarcaId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modelos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Modelos_Marcas_MarcaId",
                        column: x => x.MarcaId,
                        principalTable: "Marcas",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Utilizador",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Tipo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MoradaId = table.Column<int>(type: "int", nullable: true),
                    Discriminator = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    NivelAcesso = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Preferencias = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DadosFaturacao = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Nif = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilizador", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Utilizador_Moradas_MoradaId",
                        column: x => x.MoradaId,
                        principalTable: "Moradas",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Anuncios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Preco = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Ano = table.Column<int>(type: "int", nullable: true),
                    Cor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Descricao = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Quilometragem = table.Column<int>(type: "int", nullable: true),
                    Titulo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Caixa = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    VendedorId = table.Column<int>(type: "int", nullable: false),
                    MarcaId = table.Column<int>(type: "int", nullable: true),
                    ModeloId = table.Column<int>(type: "int", nullable: true),
                    CategoriaId = table.Column<int>(type: "int", nullable: true),
                    CombustivelId = table.Column<int>(type: "int", nullable: true),
                    TipoId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Anuncios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Anuncios_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Anuncios_Combustiveis_CombustivelId",
                        column: x => x.CombustivelId,
                        principalTable: "Combustiveis",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Anuncios_Marcas_MarcaId",
                        column: x => x.MarcaId,
                        principalTable: "Marcas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Anuncios_Modelos_ModeloId",
                        column: x => x.ModeloId,
                        principalTable: "Modelos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Anuncios_Tipos_TipoId",
                        column: x => x.TipoId,
                        principalTable: "Tipos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Anuncios_Utilizador_VendedorId",
                        column: x => x.VendedorId,
                        principalTable: "Utilizador",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Contactos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VendedorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contactos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contactos_Utilizador_VendedorId",
                        column: x => x.VendedorId,
                        principalTable: "Utilizador",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContactosCompradores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CompradorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactosCompradores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContactosCompradores_Utilizador_CompradorId",
                        column: x => x.CompradorId,
                        principalTable: "Utilizador",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FiltrosFavoritos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompradorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FiltrosFavoritos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FiltrosFavoritos_Utilizador_CompradorId",
                        column: x => x.CompradorId,
                        principalTable: "Utilizador",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MarcasFavoritas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompradorId = table.Column<int>(type: "int", nullable: false),
                    MarcaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarcasFavoritas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarcasFavoritas_Marcas_MarcaId",
                        column: x => x.MarcaId,
                        principalTable: "Marcas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MarcasFavoritas_Utilizador_CompradorId",
                        column: x => x.CompradorId,
                        principalTable: "Utilizador",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PesquisasPassadas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Data = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false),
                    CompradorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PesquisasPassadas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PesquisasPassadas_Utilizador_CompradorId",
                        column: x => x.CompradorId,
                        principalTable: "Utilizador",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnunciosFavoritos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompradorId = table.Column<int>(type: "int", nullable: false),
                    AnuncioId = table.Column<int>(type: "int", nullable: false),
                    Campo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnunciosFavoritos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnunciosFavoritos_Anuncios_AnuncioId",
                        column: x => x.AnuncioId,
                        principalTable: "Anuncios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnunciosFavoritos_Utilizador_CompradorId",
                        column: x => x.CompradorId,
                        principalTable: "Utilizador",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Compras",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnuncioId = table.Column<int>(type: "int", nullable: false),
                    CompradorId = table.Column<int>(type: "int", nullable: false),
                    Data = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EstadoPagamento = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Compras", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Compras_Anuncios_AnuncioId",
                        column: x => x.AnuncioId,
                        principalTable: "Anuncios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Compras_Utilizador_CompradorId",
                        column: x => x.CompradorId,
                        principalTable: "Utilizador",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Conversas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tipo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    VendedorId = table.Column<int>(type: "int", nullable: false),
                    CompradorId = table.Column<int>(type: "int", nullable: false),
                    AnuncioId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conversas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Conversas_Anuncios_AnuncioId",
                        column: x => x.AnuncioId,
                        principalTable: "Anuncios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Conversas_Utilizador_CompradorId",
                        column: x => x.CompradorId,
                        principalTable: "Utilizador",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Conversas_Utilizador_VendedorId",
                        column: x => x.VendedorId,
                        principalTable: "Utilizador",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Denuncia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descricao = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    DataDeDenuncia = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataEncerramento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompradorId = table.Column<int>(type: "int", nullable: false),
                    AdministradorId = table.Column<int>(type: "int", nullable: true),
                    TipoDenuncia = table.Column<string>(type: "nvarchar(21)", maxLength: 21, nullable: false),
                    AnuncioId = table.Column<int>(type: "int", nullable: true),
                    VendedorId = table.Column<int>(type: "int", nullable: true),
                    UtilizadorAlvoId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Denuncia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Denuncia_Anuncios_AnuncioId",
                        column: x => x.AnuncioId,
                        principalTable: "Anuncios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Denuncia_Utilizador_AdministradorId",
                        column: x => x.AdministradorId,
                        principalTable: "Utilizador",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Denuncia_Utilizador_CompradorId",
                        column: x => x.CompradorId,
                        principalTable: "Utilizador",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Denuncia_Utilizador_UtilizadorAlvoId",
                        column: x => x.UtilizadorAlvoId,
                        principalTable: "Utilizador",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Denuncia_Utilizador_VendedorId",
                        column: x => x.VendedorId,
                        principalTable: "Utilizador",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "HistoricoAcao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Data = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TipoAcao = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AdministradorId = table.Column<int>(type: "int", nullable: false),
                    AnuncioId = table.Column<int>(type: "int", nullable: true),
                    UtilizadorId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricoAcao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoricoAcao_Anuncios_AnuncioId",
                        column: x => x.AnuncioId,
                        principalTable: "Anuncios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistoricoAcao_Utilizador_AdministradorId",
                        column: x => x.AdministradorId,
                        principalTable: "Utilizador",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistoricoAcao_Utilizador_UtilizadorId",
                        column: x => x.UtilizadorId,
                        principalTable: "Utilizador",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Imagens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImagemCaminho = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    AnuncioId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Imagens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Imagens_Anuncios_AnuncioId",
                        column: x => x.AnuncioId,
                        principalTable: "Anuncios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Reservas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Data = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    DataExpiracao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompradorId = table.Column<int>(type: "int", nullable: false),
                    AnuncioId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reservas_Anuncios_AnuncioId",
                        column: x => x.AnuncioId,
                        principalTable: "Anuncios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reservas_Utilizador_CompradorId",
                        column: x => x.CompradorId,
                        principalTable: "Utilizador",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notificacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Conteudo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Data = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PesquisasPassadasId = table.Column<int>(type: "int", nullable: true),
                    FiltrosFavId = table.Column<int>(type: "int", nullable: true),
                    AnuncioFavId = table.Column<int>(type: "int", nullable: true),
                    MarcasFavId = table.Column<int>(type: "int", nullable: true),
                    CompradorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notificacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notificacoes_AnunciosFavoritos_AnuncioFavId",
                        column: x => x.AnuncioFavId,
                        principalTable: "AnunciosFavoritos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Notificacoes_FiltrosFavoritos_FiltrosFavId",
                        column: x => x.FiltrosFavId,
                        principalTable: "FiltrosFavoritos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Notificacoes_MarcasFavoritas_MarcasFavId",
                        column: x => x.MarcasFavId,
                        principalTable: "MarcasFavoritas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Notificacoes_PesquisasPassadas_PesquisasPassadasId",
                        column: x => x.PesquisasPassadasId,
                        principalTable: "PesquisasPassadas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Notificacoes_Utilizador_CompradorId",
                        column: x => x.CompradorId,
                        principalTable: "Utilizador",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Mensagens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Conteudo = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    DataEnvio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConversaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mensagens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Mensagens_Conversas_ConversaId",
                        column: x => x.ConversaId,
                        principalTable: "Conversas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Visitas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Data = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ReservaId = table.Column<int>(type: "int", nullable: true),
                    VendedorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Visitas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Visitas_Reservas_ReservaId",
                        column: x => x.ReservaId,
                        principalTable: "Reservas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Visitas_Utilizador_VendedorId",
                        column: x => x.VendedorId,
                        principalTable: "Utilizador",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Anuncios_CategoriaId",
                table: "Anuncios",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Anuncios_CombustivelId",
                table: "Anuncios",
                column: "CombustivelId");

            migrationBuilder.CreateIndex(
                name: "IX_Anuncios_MarcaId",
                table: "Anuncios",
                column: "MarcaId");

            migrationBuilder.CreateIndex(
                name: "IX_Anuncios_ModeloId",
                table: "Anuncios",
                column: "ModeloId");

            migrationBuilder.CreateIndex(
                name: "IX_Anuncios_TipoId",
                table: "Anuncios",
                column: "TipoId");

            migrationBuilder.CreateIndex(
                name: "IX_Anuncios_VendedorId",
                table: "Anuncios",
                column: "VendedorId");

            migrationBuilder.CreateIndex(
                name: "IX_AnunciosFavoritos_AnuncioId",
                table: "AnunciosFavoritos",
                column: "AnuncioId");

            migrationBuilder.CreateIndex(
                name: "IX_AnunciosFavoritos_CompradorId",
                table: "AnunciosFavoritos",
                column: "CompradorId");

            migrationBuilder.CreateIndex(
                name: "IX_Compras_AnuncioId",
                table: "Compras",
                column: "AnuncioId");

            migrationBuilder.CreateIndex(
                name: "IX_Compras_CompradorId",
                table: "Compras",
                column: "CompradorId");

            migrationBuilder.CreateIndex(
                name: "IX_Contactos_VendedorId",
                table: "Contactos",
                column: "VendedorId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactosCompradores_CompradorId",
                table: "ContactosCompradores",
                column: "CompradorId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversas_AnuncioId",
                table: "Conversas",
                column: "AnuncioId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversas_CompradorId",
                table: "Conversas",
                column: "CompradorId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversas_VendedorId",
                table: "Conversas",
                column: "VendedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Denuncia_AdministradorId",
                table: "Denuncia",
                column: "AdministradorId");

            migrationBuilder.CreateIndex(
                name: "IX_Denuncia_AnuncioId",
                table: "Denuncia",
                column: "AnuncioId");

            migrationBuilder.CreateIndex(
                name: "IX_Denuncia_CompradorId",
                table: "Denuncia",
                column: "CompradorId");

            migrationBuilder.CreateIndex(
                name: "IX_Denuncia_UtilizadorAlvoId",
                table: "Denuncia",
                column: "UtilizadorAlvoId");

            migrationBuilder.CreateIndex(
                name: "IX_Denuncia_VendedorId",
                table: "Denuncia",
                column: "VendedorId");

            migrationBuilder.CreateIndex(
                name: "IX_FiltrosFavoritos_CompradorId",
                table: "FiltrosFavoritos",
                column: "CompradorId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoricoAcao_AdministradorId",
                table: "HistoricoAcao",
                column: "AdministradorId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoricoAcao_AnuncioId",
                table: "HistoricoAcao",
                column: "AnuncioId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoricoAcao_UtilizadorId",
                table: "HistoricoAcao",
                column: "UtilizadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Imagens_AnuncioId",
                table: "Imagens",
                column: "AnuncioId");

            migrationBuilder.CreateIndex(
                name: "IX_MarcasFavoritas_CompradorId",
                table: "MarcasFavoritas",
                column: "CompradorId");

            migrationBuilder.CreateIndex(
                name: "IX_MarcasFavoritas_MarcaId",
                table: "MarcasFavoritas",
                column: "MarcaId");

            migrationBuilder.CreateIndex(
                name: "IX_Mensagens_ConversaId",
                table: "Mensagens",
                column: "ConversaId");

            migrationBuilder.CreateIndex(
                name: "IX_Modelos_MarcaId",
                table: "Modelos",
                column: "MarcaId");

            migrationBuilder.CreateIndex(
                name: "IX_Notificacoes_AnuncioFavId",
                table: "Notificacoes",
                column: "AnuncioFavId");

            migrationBuilder.CreateIndex(
                name: "IX_Notificacoes_CompradorId",
                table: "Notificacoes",
                column: "CompradorId");

            migrationBuilder.CreateIndex(
                name: "IX_Notificacoes_FiltrosFavId",
                table: "Notificacoes",
                column: "FiltrosFavId");

            migrationBuilder.CreateIndex(
                name: "IX_Notificacoes_MarcasFavId",
                table: "Notificacoes",
                column: "MarcasFavId");

            migrationBuilder.CreateIndex(
                name: "IX_Notificacoes_PesquisasPassadasId",
                table: "Notificacoes",
                column: "PesquisasPassadasId");

            migrationBuilder.CreateIndex(
                name: "IX_PesquisasPassadas_CompradorId",
                table: "PesquisasPassadas",
                column: "CompradorId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_AnuncioId",
                table: "Reservas",
                column: "AnuncioId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_CompradorId",
                table: "Reservas",
                column: "CompradorId");

            migrationBuilder.CreateIndex(
                name: "IX_Utilizador_MoradaId",
                table: "Utilizador",
                column: "MoradaId");

            migrationBuilder.CreateIndex(
                name: "IX_Visitas_ReservaId",
                table: "Visitas",
                column: "ReservaId");

            migrationBuilder.CreateIndex(
                name: "IX_Visitas_VendedorId",
                table: "Visitas",
                column: "VendedorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Compras");

            migrationBuilder.DropTable(
                name: "Contactos");

            migrationBuilder.DropTable(
                name: "ContactosCompradores");

            migrationBuilder.DropTable(
                name: "Denuncia");

            migrationBuilder.DropTable(
                name: "HistoricoAcao");

            migrationBuilder.DropTable(
                name: "Imagens");

            migrationBuilder.DropTable(
                name: "Mensagens");

            migrationBuilder.DropTable(
                name: "Notificacoes");

            migrationBuilder.DropTable(
                name: "Visitas");

            migrationBuilder.DropTable(
                name: "Conversas");

            migrationBuilder.DropTable(
                name: "AnunciosFavoritos");

            migrationBuilder.DropTable(
                name: "FiltrosFavoritos");

            migrationBuilder.DropTable(
                name: "MarcasFavoritas");

            migrationBuilder.DropTable(
                name: "PesquisasPassadas");

            migrationBuilder.DropTable(
                name: "Reservas");

            migrationBuilder.DropTable(
                name: "Anuncios");

            migrationBuilder.DropTable(
                name: "Categorias");

            migrationBuilder.DropTable(
                name: "Combustiveis");

            migrationBuilder.DropTable(
                name: "Modelos");

            migrationBuilder.DropTable(
                name: "Tipos");

            migrationBuilder.DropTable(
                name: "Utilizador");

            migrationBuilder.DropTable(
                name: "Marcas");

            migrationBuilder.DropTable(
                name: "Moradas");
        }
    }
}
