using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marketplace.Migrations.Marketplace
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categoria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categoria", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Combustivel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tipo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Combustivel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Marca",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Marca", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Morada",
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
                    table.PrimaryKey("PK_Morada", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tipo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tipo", x => x.Id);
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
                        name: "FK_Utilizador_Morada_MoradaId",
                        column: x => x.MoradaId,
                        principalTable: "Morada",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Modelo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MarcaId = table.Column<int>(type: "int", nullable: false),
                    TipoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modelo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Modelo_Marca_MarcaId",
                        column: x => x.MarcaId,
                        principalTable: "Marca",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Modelo_Tipo_TipoId",
                        column: x => x.TipoId,
                        principalTable: "Tipo",
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
                name: "ContactosComprador",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CompradorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactosComprador", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContactosComprador_Utilizador_CompradorId",
                        column: x => x.CompradorId,
                        principalTable: "Utilizador",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FiltrosFav",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompradorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FiltrosFav", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FiltrosFav_Utilizador_CompradorId",
                        column: x => x.CompradorId,
                        principalTable: "Utilizador",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MarcasFav",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompradorId = table.Column<int>(type: "int", nullable: false),
                    MarcaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarcasFav", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarcasFav_Marca_MarcaId",
                        column: x => x.MarcaId,
                        principalTable: "Marca",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MarcasFav_Utilizador_CompradorId",
                        column: x => x.CompradorId,
                        principalTable: "Utilizador",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "Anuncio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Preco = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
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
                    table.PrimaryKey("PK_Anuncio", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Anuncio_Categoria_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categoria",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Anuncio_Combustivel_CombustivelId",
                        column: x => x.CombustivelId,
                        principalTable: "Combustivel",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Anuncio_Marca_MarcaId",
                        column: x => x.MarcaId,
                        principalTable: "Marca",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Anuncio_Modelo_ModeloId",
                        column: x => x.ModeloId,
                        principalTable: "Modelo",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Anuncio_Tipo_TipoId",
                        column: x => x.TipoId,
                        principalTable: "Tipo",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Anuncio_Utilizador_VendedorId",
                        column: x => x.VendedorId,
                        principalTable: "Utilizador",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnuncioFav",
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
                    table.PrimaryKey("PK_AnuncioFav", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnuncioFav_Anuncio_AnuncioId",
                        column: x => x.AnuncioId,
                        principalTable: "Anuncio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnuncioFav_Utilizador_CompradorId",
                        column: x => x.CompradorId,
                        principalTable: "Utilizador",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Compra",
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
                    table.PrimaryKey("PK_Compra", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Compra_Anuncio_AnuncioId",
                        column: x => x.AnuncioId,
                        principalTable: "Anuncio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Compra_Utilizador_CompradorId",
                        column: x => x.CompradorId,
                        principalTable: "Utilizador",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Conversa",
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
                    table.PrimaryKey("PK_Conversa", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Conversa_Anuncio_AnuncioId",
                        column: x => x.AnuncioId,
                        principalTable: "Anuncio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Conversa_Utilizador_CompradorId",
                        column: x => x.CompradorId,
                        principalTable: "Utilizador",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Conversa_Utilizador_VendedorId",
                        column: x => x.VendedorId,
                        principalTable: "Utilizador",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    Discriminator = table.Column<string>(type: "nvarchar(21)", maxLength: 21, nullable: false),
                    AnuncioId = table.Column<int>(type: "int", nullable: true),
                    VendedorId = table.Column<int>(type: "int", nullable: true),
                    UtilizadorAlvoId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Denuncia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Denuncia_Anuncio_AnuncioId",
                        column: x => x.AnuncioId,
                        principalTable: "Anuncio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Denuncia_Utilizador_UtilizadorAlvoId",
                        column: x => x.UtilizadorAlvoId,
                        principalTable: "Utilizador",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    Discriminator = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    AnuncioId = table.Column<int>(type: "int", nullable: true),
                    UtilizadorId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricoAcao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoricoAcao_Anuncio_AnuncioId",
                        column: x => x.AnuncioId,
                        principalTable: "Anuncio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HistoricoAcao_Utilizador_AdministradorId",
                        column: x => x.AdministradorId,
                        principalTable: "Utilizador",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HistoricoAcao_Utilizador_UtilizadorId",
                        column: x => x.UtilizadorId,
                        principalTable: "Utilizador",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Imagem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImagemCaminho = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    AnuncioId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Imagem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Imagem_Anuncio_AnuncioId",
                        column: x => x.AnuncioId,
                        principalTable: "Anuncio",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Reserva",
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
                    table.PrimaryKey("PK_Reserva", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reserva_Anuncio_AnuncioId",
                        column: x => x.AnuncioId,
                        principalTable: "Anuncio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reserva_Utilizador_CompradorId",
                        column: x => x.CompradorId,
                        principalTable: "Utilizador",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                        name: "FK_Notificacoes_AnuncioFav_AnuncioFavId",
                        column: x => x.AnuncioFavId,
                        principalTable: "AnuncioFav",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Notificacoes_FiltrosFav_FiltrosFavId",
                        column: x => x.FiltrosFavId,
                        principalTable: "FiltrosFav",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Notificacoes_MarcasFav_MarcasFavId",
                        column: x => x.MarcasFavId,
                        principalTable: "MarcasFav",
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
                        onDelete: ReferentialAction.Cascade);
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
                        name: "FK_Mensagens_Conversa_ConversaId",
                        column: x => x.ConversaId,
                        principalTable: "Conversa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Visita",
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
                    table.PrimaryKey("PK_Visita", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Visita_Reserva_ReservaId",
                        column: x => x.ReservaId,
                        principalTable: "Reserva",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Visita_Utilizador_VendedorId",
                        column: x => x.VendedorId,
                        principalTable: "Utilizador",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Anuncio_CategoriaId",
                table: "Anuncio",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Anuncio_CombustivelId",
                table: "Anuncio",
                column: "CombustivelId");

            migrationBuilder.CreateIndex(
                name: "IX_Anuncio_MarcaId",
                table: "Anuncio",
                column: "MarcaId");

            migrationBuilder.CreateIndex(
                name: "IX_Anuncio_ModeloId",
                table: "Anuncio",
                column: "ModeloId");

            migrationBuilder.CreateIndex(
                name: "IX_Anuncio_TipoId",
                table: "Anuncio",
                column: "TipoId");

            migrationBuilder.CreateIndex(
                name: "IX_Anuncio_VendedorId",
                table: "Anuncio",
                column: "VendedorId");

            migrationBuilder.CreateIndex(
                name: "IX_AnuncioFav_AnuncioId",
                table: "AnuncioFav",
                column: "AnuncioId");

            migrationBuilder.CreateIndex(
                name: "IX_AnuncioFav_CompradorId",
                table: "AnuncioFav",
                column: "CompradorId");

            migrationBuilder.CreateIndex(
                name: "IX_Compra_AnuncioId",
                table: "Compra",
                column: "AnuncioId");

            migrationBuilder.CreateIndex(
                name: "IX_Compra_CompradorId",
                table: "Compra",
                column: "CompradorId");

            migrationBuilder.CreateIndex(
                name: "IX_Contactos_VendedorId",
                table: "Contactos",
                column: "VendedorId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactosComprador_CompradorId",
                table: "ContactosComprador",
                column: "CompradorId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversa_AnuncioId",
                table: "Conversa",
                column: "AnuncioId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversa_CompradorId",
                table: "Conversa",
                column: "CompradorId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversa_VendedorId",
                table: "Conversa",
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
                name: "IX_FiltrosFav_CompradorId",
                table: "FiltrosFav",
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
                name: "IX_Imagem_AnuncioId",
                table: "Imagem",
                column: "AnuncioId");

            migrationBuilder.CreateIndex(
                name: "IX_MarcasFav_CompradorId",
                table: "MarcasFav",
                column: "CompradorId");

            migrationBuilder.CreateIndex(
                name: "IX_MarcasFav_MarcaId",
                table: "MarcasFav",
                column: "MarcaId");

            migrationBuilder.CreateIndex(
                name: "IX_Mensagens_ConversaId",
                table: "Mensagens",
                column: "ConversaId");

            migrationBuilder.CreateIndex(
                name: "IX_Modelo_MarcaId",
                table: "Modelo",
                column: "MarcaId");

            migrationBuilder.CreateIndex(
                name: "IX_Modelo_TipoId",
                table: "Modelo",
                column: "TipoId");

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
                name: "IX_Reserva_AnuncioId",
                table: "Reserva",
                column: "AnuncioId");

            migrationBuilder.CreateIndex(
                name: "IX_Reserva_CompradorId",
                table: "Reserva",
                column: "CompradorId");

            migrationBuilder.CreateIndex(
                name: "IX_Utilizador_MoradaId",
                table: "Utilizador",
                column: "MoradaId");

            migrationBuilder.CreateIndex(
                name: "IX_Visita_ReservaId",
                table: "Visita",
                column: "ReservaId");

            migrationBuilder.CreateIndex(
                name: "IX_Visita_VendedorId",
                table: "Visita",
                column: "VendedorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Compra");

            migrationBuilder.DropTable(
                name: "Contactos");

            migrationBuilder.DropTable(
                name: "ContactosComprador");

            migrationBuilder.DropTable(
                name: "Denuncia");

            migrationBuilder.DropTable(
                name: "HistoricoAcao");

            migrationBuilder.DropTable(
                name: "Imagem");

            migrationBuilder.DropTable(
                name: "Mensagens");

            migrationBuilder.DropTable(
                name: "Notificacoes");

            migrationBuilder.DropTable(
                name: "Visita");

            migrationBuilder.DropTable(
                name: "Conversa");

            migrationBuilder.DropTable(
                name: "AnuncioFav");

            migrationBuilder.DropTable(
                name: "FiltrosFav");

            migrationBuilder.DropTable(
                name: "MarcasFav");

            migrationBuilder.DropTable(
                name: "PesquisasPassadas");

            migrationBuilder.DropTable(
                name: "Reserva");

            migrationBuilder.DropTable(
                name: "Anuncio");

            migrationBuilder.DropTable(
                name: "Categoria");

            migrationBuilder.DropTable(
                name: "Combustivel");

            migrationBuilder.DropTable(
                name: "Modelo");

            migrationBuilder.DropTable(
                name: "Utilizador");

            migrationBuilder.DropTable(
                name: "Marca");

            migrationBuilder.DropTable(
                name: "Tipo");

            migrationBuilder.DropTable(
                name: "Morada");
        }
    }
}
