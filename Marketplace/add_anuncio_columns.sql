-- Adicionar colunas em falta na tabela Anuncios
-- Execução: sqlcmd -S (localdb)\MSSQLLocalDB -d MarketplaceDb -i add_anuncio_columns.sql

-- Cilindrada (cm³)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Anuncios]') AND name = 'Cilindrada')
BEGIN
    ALTER TABLE [dbo].[Anuncios] ADD [Cilindrada] INT NULL;
    PRINT 'Coluna Cilindrada adicionada';
END

-- Localizacao
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Anuncios]') AND name = 'Localizacao')
BEGIN
    ALTER TABLE [dbo].[Anuncios] ADD [Localizacao] NVARCHAR(100) NULL;
    PRINT 'Coluna Localizacao adicionada';
END

-- Lugares
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Anuncios]') AND name = 'Lugares')
BEGIN
    ALTER TABLE [dbo].[Anuncios] ADD [Lugares] INT NULL;
    PRINT 'Coluna Lugares adicionada';
END

-- n_visualizacoes
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Anuncios]') AND name = 'n_visualizacoes')
BEGIN
    ALTER TABLE [dbo].[Anuncios] ADD [n_visualizacoes] INT NOT NULL DEFAULT 0;
    PRINT 'Coluna n_visualizacoes adicionada';
END

-- Portas
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Anuncios]') AND name = 'Portas')
BEGIN
    ALTER TABLE [dbo].[Anuncios] ADD [Portas] INT NULL;
    PRINT 'Coluna Portas adicionada';
END

-- Potencia (cv)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Anuncios]') AND name = 'Potencia')
BEGIN
    ALTER TABLE [dbo].[Anuncios] ADD [Potencia] INT NULL;
    PRINT 'Coluna Potencia adicionada';
END

-- Valor_sinal
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Anuncios]') AND name = 'Valor_sinal')
BEGIN
    ALTER TABLE [dbo].[Anuncios] ADD [Valor_sinal] DECIMAL(10,2) NOT NULL DEFAULT 0;
    PRINT 'Coluna Valor_sinal adicionada';
END

-- TipoId (Foreign Key)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Anuncios]') AND name = 'TipoId')
BEGIN
    ALTER TABLE [dbo].[Anuncios] ADD [TipoId] INT NULL;
    PRINT 'Coluna TipoId adicionada';

    -- Adicionar Foreign Key se a tabela Tipos existir
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Tipos')
    BEGIN
        ALTER TABLE [dbo].[Anuncios]
        ADD CONSTRAINT FK_Anuncios_Tipos_TipoId
        FOREIGN KEY (TipoId) REFERENCES Tipos(Id)
        ON DELETE NO ACTION;
        PRINT 'Foreign Key FK_Anuncios_Tipos_TipoId adicionada';
    END
END

PRINT 'Script executado com sucesso!';
GO
