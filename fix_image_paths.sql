-- Script para corrigir os caminhos das imagens de /images/ para /imagens/
-- Este script atualiza os registos na tabela Imagens que têm o caminho errado

USE MarketplaceDb;
GO

-- Verificar quantos registos têm o caminho errado
SELECT COUNT(*) as 'Registos com caminho errado'
FROM Imagens
WHERE ImagemCaminho LIKE '/images/%';
GO

-- Mostrar alguns exemplos antes da correção
SELECT TOP 5 Id, ImagemCaminho, AnuncioId
FROM Imagens
WHERE ImagemCaminho LIKE '/images/%';
GO

-- Atualizar os caminhos
UPDATE Imagens
SET ImagemCaminho = REPLACE(ImagemCaminho, '/images/', '/imagens/')
WHERE ImagemCaminho LIKE '/images/%';
GO

-- Verificar após a correção
SELECT COUNT(*) as 'Registos corrigidos'
FROM Imagens
WHERE ImagemCaminho LIKE '/imagens/%';
GO

-- Mostrar alguns exemplos após a correção
SELECT TOP 5 Id, ImagemCaminho, AnuncioId
FROM Imagens
WHERE ImagemCaminho LIKE '/imagens/%';
GO

PRINT 'Caminhos das imagens corrigidos com sucesso!';
