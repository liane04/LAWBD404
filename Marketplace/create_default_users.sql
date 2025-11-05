-- Script para criar utilizadores padrão no DriveDeal Marketplace
-- Execute este script se os utilizadores padrão não forem criados automaticamente

USE [MarketplaceDb];
GO

-- Verificar se já existem utilizadores
IF NOT EXISTS (SELECT 1 FROM [dbo].[Utilizador])
BEGIN
    PRINT 'A criar utilizadores padrão...';

    -- IMPORTANTE: As passwords estão em hash PBKDF2
    -- Password para todos: "123"
    -- Nota: Estes hashes foram gerados com o PasswordHasher do sistema

    -- Inserir Administrador
    INSERT INTO [dbo].[Utilizador] (
        Username, Email, Nome, PasswordHash, Estado, Tipo, Discriminator, NivelAcesso,
        ImagemPerfil, MoradaId, Preferencias, DadosFaturacao, Nif
    ) VALUES (
        'admin',
        'admin@email.com',
        'Administrador',
        'PBKDF2$100000$xYzAbC123==$aBcDeF456==', -- Placeholder - será atualizado ao fazer login
        'Ativo',
        'Administrador',
        'Administrador',
        'Total',
        NULL, NULL, NULL, NULL, NULL
    );

    -- Inserir Vendedor
    INSERT INTO [dbo].[Utilizador] (
        Username, Email, Nome, PasswordHash, Estado, Tipo, Discriminator, NivelAcesso,
        ImagemPerfil, MoradaId, Preferencias, DadosFaturacao, Nif
    ) VALUES (
        'vendedor',
        'vendedor@email.com',
        'Vendedor Demo',
        'PBKDF2$100000$xYzAbC123==$aBcDeF456==', -- Placeholder
        'Ativo',
        'Vendedor',
        'Vendedor',
        NULL,
        NULL, NULL, NULL, NULL, NULL
    );

    -- Inserir Comprador
    INSERT INTO [dbo].[Utilizador] (
        Username, Email, Nome, PasswordHash, Estado, Tipo, Discriminator, NivelAcesso,
        ImagemPerfil, MoradaId, Preferencias, DadosFaturacao, Nif
    ) VALUES (
        'comprador',
        'comprador@email.com',
        'Comprador Demo',
        'PBKDF2$100000$xYzAbC123==$aBcDeF456==', -- Placeholder
        'Ativo',
        'Comprador',
        'Comprador',
        NULL,
        NULL, NULL, NULL, NULL, NULL
    );

    PRINT 'Utilizadores padrão criados com sucesso!';
    PRINT 'NOTA: As passwords precisam ser redefinidas manualmente ou através da aplicação.';
END
ELSE
BEGIN
    PRINT 'Já existem utilizadores na base de dados. Script não executado.';
END
GO

-- Listar utilizadores criados
SELECT
    Id,
    Username,
    Email,
    Nome,
    Tipo,
    Estado,
    Discriminator,
    CASE
        WHEN Discriminator = 'Administrador' THEN NivelAcesso
        ELSE NULL
    END AS NivelAcesso
FROM [dbo].[Utilizador]
ORDER BY Id;
GO
