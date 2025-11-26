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
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
    CREATE TABLE [Categorias] (
        [Id] int NOT NULL IDENTITY,
        [Nome] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_Categorias] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
    CREATE TABLE [Combustiveis] (
        [Id] int NOT NULL IDENTITY,
        [Tipo] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_Combustiveis] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
    CREATE TABLE [Marcas] (
        [Id] int NOT NULL IDENTITY,
        [Nome] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_Marcas] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
    CREATE TABLE [Moradas] (
        [Id] int NOT NULL IDENTITY,
        [CodigoPostal] nvarchar(20) NOT NULL,
        [Localidade] nvarchar(100) NOT NULL,
        [Rua] nvarchar(200) NOT NULL,
        CONSTRAINT [PK_Moradas] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
    CREATE TABLE [Tipos] (
        [Id] int NOT NULL IDENTITY,
        [Nome] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_Tipos] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
    CREATE TABLE [Modelos] (
        [Id] int NOT NULL IDENTITY,
        [Nome] nvarchar(100) NOT NULL,
        [MarcaId] int NOT NULL,
        [TipoId] int NOT NULL,
        CONSTRAINT [PK_Modelos] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Modelos_Marcas_MarcaId] FOREIGN KEY ([MarcaId]) REFERENCES [Marcas] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Modelos_Tipos_TipoId] FOREIGN KEY ([TipoId]) REFERENCES [Tipos] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
    CREATE TABLE [Contactos] (
        [Id] int NOT NULL IDENTITY,
        [Nome] nvarchar(100) NOT NULL,
        [VendedorId] int NOT NULL,
        CONSTRAINT [PK_Contactos] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Contactos_Utilizador_VendedorId] FOREIGN KEY ([VendedorId]) REFERENCES [Utilizador] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
    CREATE TABLE [ContactosCompradores] (
        [Id] int NOT NULL IDENTITY,
        [Nome] nvarchar(100) NOT NULL,
        [CompradorId] int NOT NULL,
        CONSTRAINT [PK_ContactosCompradores] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ContactosCompradores_Utilizador_CompradorId] FOREIGN KEY ([CompradorId]) REFERENCES [Utilizador] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
    CREATE TABLE [FiltrosFavoritos] (
        [Id] int NOT NULL IDENTITY,
        [CompradorId] int NOT NULL,
        CONSTRAINT [PK_FiltrosFavoritos] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_FiltrosFavoritos_Utilizador_CompradorId] FOREIGN KEY ([CompradorId]) REFERENCES [Utilizador] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
    CREATE TABLE [MarcasFavoritas] (
        [Id] int NOT NULL IDENTITY,
        [CompradorId] int NOT NULL,
        [MarcaId] int NOT NULL,
        CONSTRAINT [PK_MarcasFavoritas] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MarcasFavoritas_Marcas_MarcaId] FOREIGN KEY ([MarcaId]) REFERENCES [Marcas] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_MarcasFavoritas_Utilizador_CompradorId] FOREIGN KEY ([CompradorId]) REFERENCES [Utilizador] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
    CREATE TABLE [PesquisasPassadas] (
        [Id] int NOT NULL IDENTITY,
        [Data] datetime2 NOT NULL,
        [Count] int NOT NULL,
        [CompradorId] int NOT NULL,
        CONSTRAINT [PK_PesquisasPassadas] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PesquisasPassadas_Utilizador_CompradorId] FOREIGN KEY ([CompradorId]) REFERENCES [Utilizador] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
    CREATE TABLE [AnunciosFavoritos] (
        [Id] int NOT NULL IDENTITY,
        [CompradorId] int NOT NULL,
        [AnuncioId] int NOT NULL,
        [Campo] nvarchar(100) NULL,
        CONSTRAINT [PK_AnunciosFavoritos] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AnunciosFavoritos_Anuncios_AnuncioId] FOREIGN KEY ([AnuncioId]) REFERENCES [Anuncios] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AnunciosFavoritos_Utilizador_CompradorId] FOREIGN KEY ([CompradorId]) REFERENCES [Utilizador] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
    CREATE TABLE [Imagens] (
        [Id] int NOT NULL IDENTITY,
        [ImagemCaminho] nvarchar(500) NOT NULL,
        [AnuncioId] int NULL,
        CONSTRAINT [PK_Imagens] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Imagens_Anuncios_AnuncioId] FOREIGN KEY ([AnuncioId]) REFERENCES [Anuncios] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
    CREATE TABLE [Mensagens] (
        [Id] int NOT NULL IDENTITY,
        [Conteudo] nvarchar(2000) NOT NULL,
        [Estado] nvarchar(30) NULL,
        [DataEnvio] datetime2 NOT NULL,
        [ConversaId] int NOT NULL,
        CONSTRAINT [PK_Mensagens] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Mensagens_Conversas_ConversaId] FOREIGN KEY ([ConversaId]) REFERENCES [Conversas] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Anuncios_CategoriaId] ON [Anuncios] ([CategoriaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Anuncios_CombustivelId] ON [Anuncios] ([CombustivelId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Anuncios_MarcaId] ON [Anuncios] ([MarcaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Anuncios_ModeloId] ON [Anuncios] ([ModeloId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Anuncios_TipoId] ON [Anuncios] ([TipoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Anuncios_VendedorId] ON [Anuncios] ([VendedorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AnunciosFavoritos_AnuncioId] ON [AnunciosFavoritos] ([AnuncioId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AnunciosFavoritos_CompradorId] ON [AnunciosFavoritos] ([CompradorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Compras_AnuncioId] ON [Compras] ([AnuncioId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Compras_CompradorId] ON [Compras] ([CompradorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Contactos_VendedorId] ON [Contactos] ([VendedorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ContactosCompradores_CompradorId] ON [ContactosCompradores] ([CompradorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Conversas_AnuncioId] ON [Conversas] ([AnuncioId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Conversas_CompradorId] ON [Conversas] ([CompradorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Conversas_VendedorId] ON [Conversas] ([VendedorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Denuncia_AdministradorId] ON [Denuncia] ([AdministradorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Denuncia_AnuncioId] ON [Denuncia] ([AnuncioId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Denuncia_CompradorId] ON [Denuncia] ([CompradorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Denuncia_UtilizadorAlvoId] ON [Denuncia] ([UtilizadorAlvoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Denuncia_VendedorId] ON [Denuncia] ([VendedorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_FiltrosFavoritos_CompradorId] ON [FiltrosFavoritos] ([CompradorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_HistoricoAcao_AdministradorId] ON [HistoricoAcao] ([AdministradorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_HistoricoAcao_AnuncioId] ON [HistoricoAcao] ([AnuncioId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_HistoricoAcao_UtilizadorId] ON [HistoricoAcao] ([UtilizadorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Imagens_AnuncioId] ON [Imagens] ([AnuncioId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_MarcasFavoritas_CompradorId] ON [MarcasFavoritas] ([CompradorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_MarcasFavoritas_MarcaId] ON [MarcasFavoritas] ([MarcaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Mensagens_ConversaId] ON [Mensagens] ([ConversaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Modelos_MarcaId] ON [Modelos] ([MarcaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Modelos_TipoId] ON [Modelos] ([TipoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Notificacoes_AnuncioFavId] ON [Notificacoes] ([AnuncioFavId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Notificacoes_CompradorId] ON [Notificacoes] ([CompradorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Notificacoes_FiltrosFavId] ON [Notificacoes] ([FiltrosFavId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Notificacoes_MarcasFavId] ON [Notificacoes] ([MarcasFavId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Notificacoes_PesquisasPassadasId] ON [Notificacoes] ([PesquisasPassadasId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PesquisasPassadas_CompradorId] ON [PesquisasPassadas] ([CompradorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Reservas_AnuncioId] ON [Reservas] ([AnuncioId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Reservas_CompradorId] ON [Reservas] ([CompradorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Utilizador_MoradaId] ON [Utilizador] ([MoradaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Visitas_ReservaId] ON [Visitas] ([ReservaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Visitas_VendedorId] ON [Visitas] ([VendedorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023165525_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251023165525_InitialCreate', N'9.0.10');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025153031_Fase2_Update'
)
BEGIN
    CREATE TABLE [Extras] (
        [Id] int NOT NULL IDENTITY,
        [Descricao] nvarchar(100) NOT NULL,
        [Tipo] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_Extras] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025153031_Fase2_Update'
)
BEGIN
    CREATE TABLE [AnuncioExtras] (
        [Id] int NOT NULL IDENTITY,
        [AnuncioId] int NOT NULL,
        [ExtraId] int NOT NULL,
        CONSTRAINT [PK_AnuncioExtras] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AnuncioExtras_Anuncios_AnuncioId] FOREIGN KEY ([AnuncioId]) REFERENCES [Anuncios] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AnuncioExtras_Extras_ExtraId] FOREIGN KEY ([ExtraId]) REFERENCES [Extras] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025153031_Fase2_Update'
)
BEGIN
    CREATE INDEX [IX_AnuncioExtras_AnuncioId] ON [AnuncioExtras] ([AnuncioId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025153031_Fase2_Update'
)
BEGIN
    CREATE INDEX [IX_AnuncioExtras_ExtraId] ON [AnuncioExtras] ([ExtraId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251025153031_Fase2_Update'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251025153031_Fase2_Update', N'9.0.10');
END;

COMMIT;
GO

