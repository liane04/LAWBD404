# Estrutura do Projeto Marketplace - ASP.NET Core MVC

## Visao Geral
Projeto ASP.NET Core 8.0 MVC em C# com Entity Framework Core 9.0.10 e SQL Server. Marketplace de automoveis com suporte para vendedores, compradores e administradores.

## 1. ESTRUTURA DE PASTAS

### Controllers (6 controladores, 486 linhas no total)
- AdministradorController.cs (15 linhas)
- AnunciosController.cs (195 linhas) - CRUD de anuncios
- FaqController.cs (15 linhas)
- HomeController.cs (39 linhas) - Homepage, Privacy, Error handling
- MensagensController.cs (15 linhas)
- UtilizadoresController.cs (207 linhas) - Gestao de utilizadores

### Models (33+ entidades)
**Hierarquia Utilizador (TPH):**
- Utilizador (base abstrata) - Id, Username, Email, Nome, PasswordHash, Estado, Tipo, ImagemPerfil, MoradaId
  - Administrador - NivelAcesso, HistoricoAcoes, DenunciasGeridas
  - Vendedor - DadosFaturacao, Nif, Contactos, Anuncios, Visitas, Conversas
  - Comprador - Preferencias, Contactos, Reservas, Compras, Pesquisas, Notificacoes, Favoritos, Conversas, Denuncias

**Entidades Principais:**
- Anuncio - Preco(10,2), Ano, Cor, Descricao, Quilometragem, Titulo, Caixa, Localizacao, Portas, Lugares, Potencia, Cilindrada, ValorSinal(10,2), NVisualizacoes
  - FK: VendedorId, MarcaId, ModeloId, CategoriaId, CombustivelId, TipoId
  - Colecoes: Imagens, Reservas, Compras, Conversas, Denuncias, AcoesAnuncio, AnuncioExtras

**Metadados de Veiculos:**
- Marca, Modelo (com MarcaId), Categoria, Combustivel, Tipo

**Sistema de Imagens:**
- Imagem - Caminho, AnuncioId
- AnuncioExtra - Relacao N:N com Extra (Cascade delete)
- Extra - Descricao

**Sistema de Reservas e Compras:**
- Reserva - CompradorId, AnuncioId, ValorSinal
- Visita - ReservaId, DataVisita (para agendamento de visitas)
- Compra - CompradorId, AnuncioId, DataCompra

**Sistema de Comunicacao:**
- Conversa - VendedorId, CompradorId, AnuncioId, Tipo (A comprar|A anunciar)
- Mensagens - ConversaId, RemeteId, DataEnvio, Conteudo

**Sistema de Favoritos:**
- AnuncioFav - CompradorId, AnuncioId
- MarcasFav - CompradorId, MarcaId
- FiltrosFav - CompradorId, com criterios

**Sistema de Denuncias (TPH):**
- Denuncia (base) - CompradorId, AdministradorId, Motivo, DataDenuncia
  - DenunciaAnuncio - AnuncioId
  - DenunciaUser - UtilizadorAlvoId

**Historico e Auditoria (TPH):**
- HistoricoAcao (base) - AdministradorId, DataAcao, Descricao
  - AcaoAnuncio - AnuncioId
  - AcaoUser - UtilizadorId

**Notificacoes e Pesquisas:**
- Notificacoes - CompradorId, Mensagem, DataNotificacao, Lida
- PesquisasPassadas - CompradorId, Criterios, DataPesquisa

**Contactos e Moradas:**
- Contactos - VendedorId, Email, Telefone
- ContactosComprador - CompradorId, Email, Telefone
- Morada - Rua, Numero, CodigoPostal, Cidade, Pais

### Data (Contextos EF Core)
- ApplicationDbContext.cs (principal com 44 DbSets)
  - Configuracao TPH para 3 hierarquias (Utilizador, HistoricoAcao, Denuncia)
  - DeleteBehavior.Restrict em maioria (protecao integridade)
  - Precisao decimal(10,2) para Preco e ValorSinal
- MarketplaceContext.cs (contexto auxiliar com 4 DbSets basicos)

### Views (Razor)
- administrador/ - Index.cshtml
- Anuncios/ - Index, Details, Create, Edit, Delete, Compare
- Faq/ - Index.cshtml
- Home/ - Index.cshtml, Privacy.cshtml
- Mensagens/ - Index.cshtml
- Utilizadores/ - Index, Details, Edit, Delete, Perfil
- Shared/ - _Layout.cshtml, _ChatWidget.cshtml, StatusCode.cshtml, Error.cshtml, _ValidationScriptsPartial.cshtml

### wwwroot (Recursos Estaticos)
- css/ - Estilos CSS customizados
- js/ - JavaScript customizado
- imagens/ - Imagens (upload de anuncios)
- lib/ - Bibliotecas externas (Bootstrap, jQuery)
- favicon.ico

### Migrations
- 20251023165525_InitialCreate.cs/Designer.cs
- ApplicationDbContextModelSnapshot.cs

## 2. CONFIGURACAO PRINCIPAL (Program.cs)

**DbContexts:**
- MarketplaceContext - contexto auxiliar
- ApplicationDbContext - contexto principal com logica completa

**Connection Strings (appsettings.json):**
- DefaultConnection: Server=(localdb)\mssqllocaldb;Database=MarketplaceDb;
- MarketplaceContext: Segunda BD (MarketplaceContext-a21a5d6d-...)

**Middleware Configurado:**
- UseStatusCodePagesWithReExecute("/Home/StatusCode/{0}") - paginas de erro personalizadas
- UseHttpsRedirection
- UseStaticFiles
- MapControllerRoute - routing padrao {controller=Home}/{action=Index}/{id?}

## 3. TECNOLOGIAS E DEPENDENCIAS

**Framework:** ASP.NET Core 8.0 (net8.0)
**Linguagem:** C# com Nullable=enable, ImplicitUsings=enable
**ORM:** Entity Framework Core 9.0.10
**BD:** SQL Server (LocalDB)
**Packages:**
- Microsoft.EntityFrameworkCore.Design
- Microsoft.EntityFrameworkCore.SqlServer
- Microsoft.EntityFrameworkCore.Tools
- Microsoft.VisualStudio.Web.CodeGeneration.Design (8.0.7)

## 4. PADROES DE DADOS

**TPH (Table Per Hierarchy):**
- Utilizador - Discriminador=Discriminator (Administrador|Vendedor|Comprador)
- HistoricoAcao - Discriminador=TipoAcao (AcaoAnuncio|AcaoUser)
- Denuncia - Discriminador=TipoDenuncia (DenunciaAnuncio|DenunciaUser)

**Cascade Delete:**
- Primarily DeleteBehavior.Restrict (maioria dos relacionamentos)
- Excecoes: AnuncioExtra->Extra (Cascade), Visita->Reserva (SetNull)

**Precisao:**
- Decimal(10,2) para valores: Preco, ValorSinal

**Relacionamentos N:N:**
- AnuncioExtra como tabela de juncao explcita entre Anuncio e Extra

## 5. FLUXOS DE NEGOCIO

**Workflow de Venda:**
1. Vendedor cria Anuncio com Imagens
2. Comprador visualiza anuncio (NVisualizacoes++)
3. Comprador faz Reserva (com ValorSinal)
4. Sistema registra Conversa entre Vendedor e Comprador
5. Comprador agenda Visita
6. Transacao finaliza com Compra

**Sistema de Favoritos:**
- Comprador salva AnuncioFav, MarcasFav, FiltrosFav
- Sistema mantem PesquisasPassadas e oferece Notificacoes

**Moderacao:**
- Usuarios podem fazer Denuncias (Anuncio ou User)
- Administrador gere DenunciasGeridas
- HistoricoAcao registra todas as acoes administrativas

## 6. ESTADO ATUAL DO PROJETO

**Observacoes:**
- Muitos controladores com codigo comentado (em desenvolvimento)
- Views estaticas mencionadas em comentarios
- Estrutura bem definida, implementacao em progresso
- Dual DbContext (ApplicationDbContext principal, MarketplaceContext auxiliar)

**Documentacao Adicional:**
- MELHORIAS_UI.md - documentacao de melhorias de UI
- notas.txt - anotacoes diversas

## 7. RESUMO TECNICO

| Aspecto | Valor |
|--------|-------|
| Framework | ASP.NET Core 8.0 MVC |
| ORM | EF Core 9.0.10 |
| Banco de Dados | SQL Server (LocalDB) |
| Linguagem | C# (Nullable enabled) |
| Entidades | 33+ modelos |
| Controladores | 6 (486 linhas total) |
| DbSets Mapeados | 44 entidades |
| Hierarquias TPH | 3 (Utilizador, HistoricoAcao, Denuncia) |
| Migracoes | 1 inicial (InitialCreate) |
| Views | ~20 ficheiros .cshtml |
| Padroes | CRUD, MVC, EF Core Fluent API |
| DeleteBehavior | Restrict (maioria), Cascade/SetNull (excecoes) |

Documento gerado em 2025-11-05
