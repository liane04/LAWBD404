IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Categorias] (
    [Id] int NOT NULL IDENTITY,
    [Nome] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_Categorias] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Combustiveis] (
    [Id] int NOT NULL IDENTITY,
    [Tipo] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_Combustiveis] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Marcas] (
    [Id] int NOT NULL IDENTITY,
    [Nome] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_Marcas] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Moradas] (
    [Id] int NOT NULL IDENTITY,
    [CodigoPostal] nvarchar(20) NOT NULL,
    [Localidade] nvarchar(100) NOT NULL,
    [Rua] nvarchar(200) NOT NULL,
    CONSTRAINT [PK_Moradas] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Tipos] (
    [Id] int NOT NULL IDENTITY,
    [Nome] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_Tipos] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Utilizador] (
    [Id] int NOT NULL IDENTITY,
    [Username] nvarchar(60) NOT NULL,
    [Email] nvarchar(254) NOT NULL,
    [Nome] nvarchar(120) NOT NULL,
    [PasswordHash] nvarchar(255) NOT NULL,
    [Estado] nvarchar(30) NULL,
    [Tipo] nvarchar(50) NULL,
    [MoradaId] int NULL,
    [Discriminator] nvarchar(13) NOT NULL,
    [NivelAcesso] nvarchar(50) NULL,
    [Preferencias] nvarchar(500) NULL,
    [DadosFaturacao] nvarchar(200) NULL,
    [Nif] nvarchar(20) NULL,
    CONSTRAINT [PK_Utilizador] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Utilizador_Moradas_MoradaId] FOREIGN KEY ([MoradaId]) REFERENCES [Moradas] ([Id])
);
GO

CREATE TABLE [Modelos] (
    [Id] int NOT NULL IDENTITY,
    [Nome] nvarchar(100) NOT NULL,
    [MarcaId] int NOT NULL,
    [TipoId] int NOT NULL,
    CONSTRAINT [PK_Modelos] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Modelos_Marcas_MarcaId] FOREIGN KEY ([MarcaId]) REFERENCES [Marcas] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Modelos_Tipos_TipoId] FOREIGN KEY ([TipoId]) REFERENCES [Tipos] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Contactos] (
    [Id] int NOT NULL IDENTITY,
    [Nome] nvarchar(100) NOT NULL,
    [VendedorId] int NOT NULL,
    CONSTRAINT [PK_Contactos] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Contactos_Utilizador_VendedorId] FOREIGN KEY ([VendedorId]) REFERENCES [Utilizador] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [ContactosCompradores] (
    [Id] int NOT NULL IDENTITY,
    [Nome] nvarchar(100) NOT NULL,
    [CompradorId] int NOT NULL,
    CONSTRAINT [PK_ContactosCompradores] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ContactosCompradores_Utilizador_CompradorId] FOREIGN KEY ([CompradorId]) REFERENCES [Utilizador] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [FiltrosFavoritos] (
    [Id] int NOT NULL IDENTITY,
    [CompradorId] int NOT NULL,
    CONSTRAINT [PK_FiltrosFavoritos] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_FiltrosFavoritos_Utilizador_CompradorId] FOREIGN KEY ([CompradorId]) REFERENCES [Utilizador] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [MarcasFavoritas] (
    [Id] int NOT NULL IDENTITY,
    [CompradorId] int NOT NULL,
    [MarcaId] int NOT NULL,
    CONSTRAINT [PK_MarcasFavoritas] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_MarcasFavoritas_Marcas_MarcaId] FOREIGN KEY ([MarcaId]) REFERENCES [Marcas] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_MarcasFavoritas_Utilizador_CompradorId] FOREIGN KEY ([CompradorId]) REFERENCES [Utilizador] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [PesquisasPassadas] (
    [Id] int NOT NULL IDENTITY,
    [Data] datetime2 NOT NULL,
    [Count] int NOT NULL,
    [CompradorId] int NOT NULL,
    CONSTRAINT [PK_PesquisasPassadas] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PesquisasPassadas_Utilizador_CompradorId] FOREIGN KEY ([CompradorId]) REFERENCES [Utilizador] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Anuncios] (
    [Id] int NOT NULL IDENTITY,
    [Preco] decimal(10,2) NOT NULL,
    [Ano] int NULL,
    [Cor] nvarchar(50) NULL,
    [Descricao] nvarchar(2000) NULL,
    [Quilometragem] int NULL,
    [Titulo] nvarchar(200) NOT NULL,
    [Caixa] nvarchar(50) NULL,
    [VendedorId] int NOT NULL,
    [MarcaId] int NULL,
    [ModeloId] int NULL,
    [CategoriaId] int NULL,
    [CombustivelId] int NULL,
    [TipoId] int NULL,
    CONSTRAINT [PK_Anuncios] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Anuncios_Categorias_CategoriaId] FOREIGN KEY ([CategoriaId]) REFERENCES [Categorias] ([Id]),
    CONSTRAINT [FK_Anuncios_Combustiveis_CombustivelId] FOREIGN KEY ([CombustivelId]) REFERENCES [Combustiveis] ([Id]),
    CONSTRAINT [FK_Anuncios_Marcas_MarcaId] FOREIGN KEY ([MarcaId]) REFERENCES [Marcas] ([Id]),
    CONSTRAINT [FK_Anuncios_Modelos_ModeloId] FOREIGN KEY ([ModeloId]) REFERENCES [Modelos] ([Id]),
    CONSTRAINT [FK_Anuncios_Tipos_TipoId] FOREIGN KEY ([TipoId]) REFERENCES [Tipos] ([Id]),
    CONSTRAINT [FK_Anuncios_Utilizador_VendedorId] FOREIGN KEY ([VendedorId]) REFERENCES [Utilizador] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AnunciosFavoritos] (
    [Id] int NOT NULL IDENTITY,
    [CompradorId] int NOT NULL,
    [AnuncioId] int NOT NULL,
    [Campo] nvarchar(100) NULL,
    CONSTRAINT [PK_AnunciosFavoritos] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AnunciosFavoritos_Anuncios_AnuncioId] FOREIGN KEY ([AnuncioId]) REFERENCES [Anuncios] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AnunciosFavoritos_Utilizador_CompradorId] FOREIGN KEY ([CompradorId]) REFERENCES [Utilizador] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [Compras] (
    [Id] int NOT NULL IDENTITY,
    [AnuncioId] int NOT NULL,
    [CompradorId] int NOT NULL,
    [Data] datetime2 NOT NULL,
    [EstadoPagamento] nvarchar(30) NULL,
    CONSTRAINT [PK_Compras] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Compras_Anuncios_AnuncioId] FOREIGN KEY ([AnuncioId]) REFERENCES [Anuncios] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Compras_Utilizador_CompradorId] FOREIGN KEY ([CompradorId]) REFERENCES [Utilizador] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [Conversas] (
    [Id] int NOT NULL IDENTITY,
    [Tipo] nvarchar(50) NOT NULL,
    [VendedorId] int NOT NULL,
    [CompradorId] int NOT NULL,
    [AnuncioId] int NOT NULL,
    CONSTRAINT [PK_Conversas] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Conversas_Anuncios_AnuncioId] FOREIGN KEY ([AnuncioId]) REFERENCES [Anuncios] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Conversas_Utilizador_CompradorId] FOREIGN KEY ([CompradorId]) REFERENCES [Utilizador] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Conversas_Utilizador_VendedorId] FOREIGN KEY ([VendedorId]) REFERENCES [Utilizador] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [Denuncia] (
    [Id] int NOT NULL IDENTITY,
    [Descricao] nvarchar(2000) NOT NULL,
    [Estado] nvarchar(30) NULL,
    [DataDeDenuncia] datetime2 NOT NULL,
    [DataEncerramento] datetime2 NULL,
    [CompradorId] int NOT NULL,
    [AdministradorId] int NULL,
    [TipoDenuncia] nvarchar(21) NOT NULL,
    [AnuncioId] int NULL,
    [VendedorId] int NULL,
    [UtilizadorAlvoId] int NULL,
    CONSTRAINT [PK_Denuncia] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Denuncia_Anuncios_AnuncioId] FOREIGN KEY ([AnuncioId]) REFERENCES [Anuncios] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Denuncia_Utilizador_AdministradorId] FOREIGN KEY ([AdministradorId]) REFERENCES [Utilizador] ([Id]),
    CONSTRAINT [FK_Denuncia_Utilizador_CompradorId] FOREIGN KEY ([CompradorId]) REFERENCES [Utilizador] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Denuncia_Utilizador_UtilizadorAlvoId] FOREIGN KEY ([UtilizadorAlvoId]) REFERENCES [Utilizador] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Denuncia_Utilizador_VendedorId] FOREIGN KEY ([VendedorId]) REFERENCES [Utilizador] ([Id])
);
GO

CREATE TABLE [HistoricoAcao] (
    [Id] int NOT NULL IDENTITY,
    [Data] datetime2 NOT NULL,
    [Motivo] nvarchar(500) NULL,
    [TipoAcao] nvarchar(100) NOT NULL,
    [AdministradorId] int NOT NULL,
    [AnuncioId] int NULL,
    [UtilizadorId] int NULL,
    CONSTRAINT [PK_HistoricoAcao] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_HistoricoAcao_Anuncios_AnuncioId] FOREIGN KEY ([AnuncioId]) REFERENCES [Anuncios] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_HistoricoAcao_Utilizador_AdministradorId] FOREIGN KEY ([AdministradorId]) REFERENCES [Utilizador] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_HistoricoAcao_Utilizador_UtilizadorId] FOREIGN KEY ([UtilizadorId]) REFERENCES [Utilizador] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [Imagens] (
    [Id] int NOT NULL IDENTITY,
    [ImagemCaminho] nvarchar(500) NOT NULL,
    [AnuncioId] int NULL,
    CONSTRAINT [PK_Imagens] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Imagens_Anuncios_AnuncioId] FOREIGN KEY ([AnuncioId]) REFERENCES [Anuncios] ([Id])
);
GO

CREATE TABLE [Reservas] (
    [Id] int NOT NULL IDENTITY,
    [Data] datetime2 NOT NULL,
    [Estado] nvarchar(30) NULL,
    [DataExpiracao] datetime2 NULL,
    [CompradorId] int NOT NULL,
    [AnuncioId] int NOT NULL,
    CONSTRAINT [PK_Reservas] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Reservas_Anuncios_AnuncioId] FOREIGN KEY ([AnuncioId]) REFERENCES [Anuncios] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Reservas_Utilizador_CompradorId] FOREIGN KEY ([CompradorId]) REFERENCES [Utilizador] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [Notificacoes] (
    [Id] int NOT NULL IDENTITY,
    [Conteudo] nvarchar(500) NOT NULL,
    [Data] datetime2 NOT NULL,
    [PesquisasPassadasId] int NULL,
    [FiltrosFavId] int NULL,
    [AnuncioFavId] int NULL,
    [MarcasFavId] int NULL,
    [CompradorId] int NOT NULL,
    CONSTRAINT [PK_Notificacoes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Notificacoes_AnunciosFavoritos_AnuncioFavId] FOREIGN KEY ([AnuncioFavId]) REFERENCES [AnunciosFavoritos] ([Id]),
    CONSTRAINT [FK_Notificacoes_FiltrosFavoritos_FiltrosFavId] FOREIGN KEY ([FiltrosFavId]) REFERENCES [FiltrosFavoritos] ([Id]),
    CONSTRAINT [FK_Notificacoes_MarcasFavoritas_MarcasFavId] FOREIGN KEY ([MarcasFavId]) REFERENCES [MarcasFavoritas] ([Id]),
    CONSTRAINT [FK_Notificacoes_PesquisasPassadas_PesquisasPassadasId] FOREIGN KEY ([PesquisasPassadasId]) REFERENCES [PesquisasPassadas] ([Id]),
    CONSTRAINT [FK_Notificacoes_Utilizador_CompradorId] FOREIGN KEY ([CompradorId]) REFERENCES [Utilizador] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [Mensagens] (
    [Id] int NOT NULL IDENTITY,
    [Conteudo] nvarchar(2000) NOT NULL,
    [Estado] nvarchar(30) NULL,
    [DataEnvio] datetime2 NOT NULL,
    [ConversaId] int NOT NULL,
    CONSTRAINT [PK_Mensagens] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Mensagens_Conversas_ConversaId] FOREIGN KEY ([ConversaId]) REFERENCES [Conversas] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Visitas] (
    [Id] int NOT NULL IDENTITY,
    [Data] datetime2 NOT NULL,
    [Estado] nvarchar(30) NULL,
    [ReservaId] int NULL,
    [VendedorId] int NOT NULL,
    CONSTRAINT [PK_Visitas] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Visitas_Reservas_ReservaId] FOREIGN KEY ([ReservaId]) REFERENCES [Reservas] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_Visitas_Utilizador_VendedorId] FOREIGN KEY ([VendedorId]) REFERENCES [Utilizador] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_Anuncios_CategoriaId] ON [Anuncios] ([CategoriaId]);
GO

CREATE INDEX [IX_Anuncios_CombustivelId] ON [Anuncios] ([CombustivelId]);
GO

CREATE INDEX [IX_Anuncios_MarcaId] ON [Anuncios] ([MarcaId]);
GO

CREATE INDEX [IX_Anuncios_ModeloId] ON [Anuncios] ([ModeloId]);
GO

CREATE INDEX [IX_Anuncios_TipoId] ON [Anuncios] ([TipoId]);
GO

CREATE INDEX [IX_Anuncios_VendedorId] ON [Anuncios] ([VendedorId]);
GO

CREATE INDEX [IX_AnunciosFavoritos_AnuncioId] ON [AnunciosFavoritos] ([AnuncioId]);
GO

CREATE INDEX [IX_AnunciosFavoritos_CompradorId] ON [AnunciosFavoritos] ([CompradorId]);
GO

CREATE INDEX [IX_Compras_AnuncioId] ON [Compras] ([AnuncioId]);
GO

CREATE INDEX [IX_Compras_CompradorId] ON [Compras] ([CompradorId]);
GO

CREATE INDEX [IX_Contactos_VendedorId] ON [Contactos] ([VendedorId]);
GO

CREATE INDEX [IX_ContactosCompradores_CompradorId] ON [ContactosCompradores] ([CompradorId]);
GO

CREATE INDEX [IX_Conversas_AnuncioId] ON [Conversas] ([AnuncioId]);
GO

CREATE INDEX [IX_Conversas_CompradorId] ON [Conversas] ([CompradorId]);
GO

CREATE INDEX [IX_Conversas_VendedorId] ON [Conversas] ([VendedorId]);
GO

CREATE INDEX [IX_Denuncia_AdministradorId] ON [Denuncia] ([AdministradorId]);
GO

CREATE INDEX [IX_Denuncia_AnuncioId] ON [Denuncia] ([AnuncioId]);
GO

CREATE INDEX [IX_Denuncia_CompradorId] ON [Denuncia] ([CompradorId]);
GO

CREATE INDEX [IX_Denuncia_UtilizadorAlvoId] ON [Denuncia] ([UtilizadorAlvoId]);
GO

CREATE INDEX [IX_Denuncia_VendedorId] ON [Denuncia] ([VendedorId]);
GO

CREATE INDEX [IX_FiltrosFavoritos_CompradorId] ON [FiltrosFavoritos] ([CompradorId]);
GO

CREATE INDEX [IX_HistoricoAcao_AdministradorId] ON [HistoricoAcao] ([AdministradorId]);
GO

CREATE INDEX [IX_HistoricoAcao_AnuncioId] ON [HistoricoAcao] ([AnuncioId]);
GO

CREATE INDEX [IX_HistoricoAcao_UtilizadorId] ON [HistoricoAcao] ([UtilizadorId]);
GO

CREATE INDEX [IX_Imagens_AnuncioId] ON [Imagens] ([AnuncioId]);
GO

CREATE INDEX [IX_MarcasFavoritas_CompradorId] ON [MarcasFavoritas] ([CompradorId]);
GO

CREATE INDEX [IX_MarcasFavoritas_MarcaId] ON [MarcasFavoritas] ([MarcaId]);
GO

CREATE INDEX [IX_Mensagens_ConversaId] ON [Mensagens] ([ConversaId]);
GO

CREATE INDEX [IX_Modelos_MarcaId] ON [Modelos] ([MarcaId]);
GO

CREATE INDEX [IX_Modelos_TipoId] ON [Modelos] ([TipoId]);
GO

CREATE INDEX [IX_Notificacoes_AnuncioFavId] ON [Notificacoes] ([AnuncioFavId]);
GO

CREATE INDEX [IX_Notificacoes_CompradorId] ON [Notificacoes] ([CompradorId]);
GO

CREATE INDEX [IX_Notificacoes_FiltrosFavId] ON [Notificacoes] ([FiltrosFavId]);
GO

CREATE INDEX [IX_Notificacoes_MarcasFavId] ON [Notificacoes] ([MarcasFavId]);
GO

CREATE INDEX [IX_Notificacoes_PesquisasPassadasId] ON [Notificacoes] ([PesquisasPassadasId]);
GO

CREATE INDEX [IX_PesquisasPassadas_CompradorId] ON [PesquisasPassadas] ([CompradorId]);
GO

CREATE INDEX [IX_Reservas_AnuncioId] ON [Reservas] ([AnuncioId]);
GO

CREATE INDEX [IX_Reservas_CompradorId] ON [Reservas] ([CompradorId]);
GO

CREATE INDEX [IX_Utilizador_MoradaId] ON [Utilizador] ([MoradaId]);
GO

CREATE INDEX [IX_Visitas_ReservaId] ON [Visitas] ([ReservaId]);
GO

CREATE INDEX [IX_Visitas_VendedorId] ON [Visitas] ([VendedorId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251023165525_InitialCreate', N'8.0.10');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Extras] (
    [Id] int NOT NULL IDENTITY,
    [Descricao] nvarchar(100) NOT NULL,
    [Tipo] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_Extras] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AnuncioExtras] (
    [Id] int NOT NULL IDENTITY,
    [AnuncioId] int NOT NULL,
    [ExtraId] int NOT NULL,
    CONSTRAINT [PK_AnuncioExtras] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AnuncioExtras_Anuncios_AnuncioId] FOREIGN KEY ([AnuncioId]) REFERENCES [Anuncios] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AnuncioExtras_Extras_ExtraId] FOREIGN KEY ([ExtraId]) REFERENCES [Extras] ([Id]) ON DELETE NO ACTION
);
GO

CREATE INDEX [IX_AnuncioExtras_AnuncioId] ON [AnuncioExtras] ([AnuncioId]);
GO

CREATE INDEX [IX_AnuncioExtras_ExtraId] ON [AnuncioExtras] ([ExtraId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251025153031_Fase2_Update', N'8.0.10');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Utilizador] ADD [ImagemPerfil] nvarchar(500) NULL;
GO

ALTER TABLE [Anuncios] ADD [Cilindrada] int NULL;
GO

ALTER TABLE [Anuncios] ADD [Localizacao] nvarchar(100) NULL;
GO

ALTER TABLE [Anuncios] ADD [Lugares] int NULL;
GO

ALTER TABLE [Anuncios] ADD [Portas] int NULL;
GO

ALTER TABLE [Anuncios] ADD [Potencia] int NULL;
GO

ALTER TABLE [Anuncios] ADD [Valor_sinal] decimal(10,2) NOT NULL DEFAULT 0.0;
GO

ALTER TABLE [Anuncios] ADD [n_visualizacoes] int NOT NULL DEFAULT 0;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251105114921_AddImagemPerfilColumn', N'8.0.10');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Nome') AND [object_id] = OBJECT_ID(N'[Tipos]'))
    SET IDENTITY_INSERT [Tipos] ON;
INSERT INTO [Tipos] ([Id], [Nome])
VALUES (1, N'Carro'),
(2, N'Mota');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Nome') AND [object_id] = OBJECT_ID(N'[Tipos]'))
    SET IDENTITY_INSERT [Tipos] OFF;
GO

CREATE UNIQUE INDEX [IX_Tipos_Nome] ON [Tipos] ([Nome]);
GO

CREATE UNIQUE INDEX [IX_Modelos_Nome_MarcaId] ON [Modelos] ([Nome], [MarcaId]);
GO

CREATE UNIQUE INDEX [IX_Marcas_Nome] ON [Marcas] ([Nome]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251105123136_RefDataSeed', N'8.0.10');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Utilizador] ADD [IdentityUserId] int NOT NULL DEFAULT 0;
GO

CREATE TABLE [AspNetRoles] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AspNetUsers] (
    [Id] int NOT NULL IDENTITY,
    [FullName] nvarchar(max) NULL,
    [ImagemPerfil] nvarchar(max) NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] int NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] int NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserRoles] (
    [UserId] int NOT NULL,
    [RoleId] int NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserTokens] (
    [UserId] int NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE UNIQUE INDEX [IX_Utilizador_IdentityUserId] ON [Utilizador] ([IdentityUserId]);
GO

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
GO

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
GO

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
GO

CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
GO

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO

ALTER TABLE [Utilizador] ADD CONSTRAINT [FK_Utilizador_AspNetUsers_IdentityUserId] FOREIGN KEY ([IdentityUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251112121445_AddIdentityIntegration', N'8.0.10');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251124092542_AddIdentityTables', N'8.0.10');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Visitas] DROP CONSTRAINT [FK_Visitas_Utilizador_VendedorId];
GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Visitas]') AND [c].[name] = N'Estado');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Visitas] DROP CONSTRAINT [' + @var0 + '];');
UPDATE [Visitas] SET [Estado] = N'' WHERE [Estado] IS NULL;
ALTER TABLE [Visitas] ALTER COLUMN [Estado] nvarchar(30) NOT NULL;
ALTER TABLE [Visitas] ADD DEFAULT N'' FOR [Estado];
GO

ALTER TABLE [Visitas] ADD [AnuncioId] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [Visitas] ADD [CompradorId] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [Visitas] ADD [DataAtualizacao] datetime2 NULL;
GO

ALTER TABLE [Visitas] ADD [DataCriacao] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
GO

ALTER TABLE [Visitas] ADD [Observacoes] nvarchar(500) NULL;
GO

CREATE INDEX [IX_Visitas_AnuncioId] ON [Visitas] ([AnuncioId]);
GO

CREATE INDEX [IX_Visitas_CompradorId] ON [Visitas] ([CompradorId]);
GO

ALTER TABLE [Visitas] ADD CONSTRAINT [FK_Visitas_Anuncios_AnuncioId] FOREIGN KEY ([AnuncioId]) REFERENCES [Anuncios] ([Id]) ON DELETE NO ACTION;
GO

ALTER TABLE [Visitas] ADD CONSTRAINT [FK_Visitas_Utilizador_CompradorId] FOREIGN KEY ([CompradorId]) REFERENCES [Utilizador] ([Id]) ON DELETE NO ACTION;
GO

ALTER TABLE [Visitas] ADD CONSTRAINT [FK_Visitas_Utilizador_VendedorId] FOREIGN KEY ([VendedorId]) REFERENCES [Utilizador] ([Id]) ON DELETE NO ACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251126115008_UpdateVisitaModel', N'8.0.10');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Mensagens] ADD [Lida] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Mensagens] ADD [RemetenteId] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [Conversas] ADD [UltimaMensagemData] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
GO

CREATE INDEX [IX_Mensagens_RemetenteId] ON [Mensagens] ([RemetenteId]);
GO

ALTER TABLE [Mensagens] ADD CONSTRAINT [FK_Mensagens_AspNetUsers_RemetenteId] FOREIGN KEY ([RemetenteId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251203113008_UpdateMessagingSystem', N'8.0.10');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [FiltrosFavoritos] ADD [AnoMax] int NULL;
GO

ALTER TABLE [FiltrosFavoritos] ADD [AnoMin] int NULL;
GO

ALTER TABLE [FiltrosFavoritos] ADD [Ativo] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [FiltrosFavoritos] ADD [Caixa] nvarchar(50) NULL;
GO

ALTER TABLE [FiltrosFavoritos] ADD [CombustivelId] int NULL;
GO

ALTER TABLE [FiltrosFavoritos] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
GO

ALTER TABLE [FiltrosFavoritos] ADD [KmMax] int NULL;
GO

ALTER TABLE [FiltrosFavoritos] ADD [LastCheckedAt] datetime2 NULL;
GO

ALTER TABLE [FiltrosFavoritos] ADD [Localizacao] nvarchar(100) NULL;
GO

ALTER TABLE [FiltrosFavoritos] ADD [MarcaId] int NULL;
GO

ALTER TABLE [FiltrosFavoritos] ADD [MaxAnuncioIdNotificado] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [FiltrosFavoritos] ADD [ModeloId] int NULL;
GO

ALTER TABLE [FiltrosFavoritos] ADD [Nome] nvarchar(100) NULL;
GO

ALTER TABLE [FiltrosFavoritos] ADD [PrecoMax] decimal(10,2) NULL;
GO

ALTER TABLE [FiltrosFavoritos] ADD [TipoId] int NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251221213732_UpdateFiltrosFavoritosTable', N'8.0.10');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [DisponibilidadesVendedor] (
    [Id] int NOT NULL IDENTITY,
    [VendedorId] int NOT NULL,
    [DiaSemana] int NOT NULL,
    [HoraInicio] time NOT NULL,
    [HoraFim] time NOT NULL,
    [IntervaloMinutos] int NOT NULL,
    [Ativo] bit NOT NULL,
    [DataCriacao] datetime2 NOT NULL,
    CONSTRAINT [PK_DisponibilidadesVendedor] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_DisponibilidadesVendedor_Utilizador_VendedorId] FOREIGN KEY ([VendedorId]) REFERENCES [Utilizador] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_DisponibilidadesVendedor_VendedorId] ON [DisponibilidadesVendedor] ([VendedorId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251221214844_AddDisponibilidadeVendedor', N'8.0.10');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Anuncios] ADD [Vendido] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251227214812_AdicionarCampoVendidoAnuncio', N'8.0.10');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Anuncios] ADD [Estado] nvarchar(20) NOT NULL DEFAULT N'Ativo';
GO


                UPDATE Anuncios
                SET Estado = CASE
                    WHEN Vendido = 1 THEN 'Vendido'
                    ELSE 'Ativo'
                END
            
GO

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Anuncios]') AND [c].[name] = N'Vendido');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Anuncios] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [Anuncios] DROP COLUMN [Vendido];
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251227222440_SubstituirVendidoPorEstado', N'8.0.10');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [PedidosVendedor] (
    [Id] int NOT NULL IDENTITY,
    [CompradorId] int NOT NULL,
    [Nif] nvarchar(9) NOT NULL,
    [DadosFaturacao] nvarchar(500) NULL,
    [Motivacao] nvarchar(1000) NOT NULL,
    [Estado] nvarchar(20) NOT NULL,
    [DataPedido] datetime2 NOT NULL,
    [DataResposta] datetime2 NULL,
    [AdminRespondeuId] int NULL,
    [MotivoRejeicao] nvarchar(500) NULL,
    CONSTRAINT [PK_PedidosVendedor] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PedidosVendedor_Utilizador_CompradorId] FOREIGN KEY ([CompradorId]) REFERENCES [Utilizador] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_PedidosVendedor_CompradorId] ON [PedidosVendedor] ([CompradorId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251228114326_AdicionarTabelaPedidosVendedor', N'8.0.10');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Utilizador] ADD [Nib] nvarchar(21) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251228184545_AdicionarNibVendedor', N'8.0.10');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [FiltrosFavoritos] ADD [CategoriaId] int NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251230124927_addcategoriasFiltrosFav', N'8.0.10');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [PesquisasPassadas] ADD [Descricao] nvarchar(200) NULL;
GO

ALTER TABLE [PesquisasPassadas] ADD [Parametros] nvarchar(500) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251230152704_historico', N'8.0.10');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Anuncios] ADD [DataDestaque] datetime2 NULL;
GO

ALTER TABLE [Anuncios] ADD [Destacado] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Anuncios] ADD [DestaqueAte] datetime2 NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251230230024_AdicionarCamposDestaque', N'8.0.10');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DROP INDEX [IX_Utilizador_IdentityUserId] ON [Utilizador];
GO

CREATE INDEX [IX_Utilizador_IdentityUserId] ON [Utilizador] ([IdentityUserId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251231151114_RemoverUniqueConstraintIdentityUserId', N'8.0.10');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DROP INDEX [IX_Reservas_CompradorId] ON [Reservas];
GO

CREATE INDEX [IX_Reservas_CompradorId_Estado] ON [Reservas] ([CompradorId], [Estado]);
GO

CREATE INDEX [IX_Anuncios_Destacado_DestaqueAte] ON [Anuncios] ([Destacado], [DestaqueAte]);
GO

CREATE INDEX [IX_Anuncios_Estado] ON [Anuncios] ([Estado]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251231165649_AdicionarIndicesPerformance', N'8.0.10');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [NotificationPreferences] (
    [Id] int NOT NULL IDENTITY,
    [IdentityUserId] nvarchar(450) NOT NULL,
    [EmailNotifications] bit NOT NULL,
    [NewListingsAlerts] bit NOT NULL,
    [PriceDropAlerts] bit NOT NULL,
    [Newsletter] bit NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_NotificationPreferences] PRIMARY KEY ([Id])
);
GO

CREATE UNIQUE INDEX [IX_NotificationPreferences_IdentityUserId] ON [NotificationPreferences] ([IdentityUserId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251231183046_AddNotificationPreferences', N'8.0.10');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Avaliacoes] (
    [Id] int NOT NULL IDENTITY,
    [VendedorId] int NOT NULL,
    [CompradorId] int NOT NULL,
    [Data] datetime2 NOT NULL,
    [Nota] int NOT NULL,
    [Comentario] nvarchar(500) NULL,
    CONSTRAINT [PK_Avaliacoes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Avaliacoes_Utilizador_CompradorId] FOREIGN KEY ([CompradorId]) REFERENCES [Utilizador] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Avaliacoes_Utilizador_VendedorId] FOREIGN KEY ([VendedorId]) REFERENCES [Utilizador] ([Id]) ON DELETE NO ACTION
);
GO

CREATE INDEX [IX_Avaliacoes_CompradorId] ON [Avaliacoes] ([CompradorId]);
GO

CREATE INDEX [IX_Avaliacoes_VendedorId] ON [Avaliacoes] ([VendedorId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251231185515_avaliacoes', N'8.0.10');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251231191022_valicaoVdendeddd', N'8.0.10');
GO

COMMIT;
GO

