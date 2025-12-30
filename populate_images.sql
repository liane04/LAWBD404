-- Script para popular a tabela Imagens com base nos anúncios existentes
-- Adiciona 3 imagens padrão para cada anúncio

USE MarketplaceDb;
GO

-- Limpar imagens existentes (se houver)
DELETE FROM Imagens;
GO

-- Inserir imagens para cada anúncio (assumindo que os anúncios têm IDs de 1 a 21)
DECLARE @AnuncioId INT = 1;
DECLARE @MaxAnuncio INT;

-- Obter o ID máximo de anúncio
SELECT @MaxAnuncio = MAX(Id) FROM Anuncios;

WHILE @AnuncioId <= @MaxAnuncio
BEGIN
    -- Verificar se o anúncio existe
    IF EXISTS (SELECT 1 FROM Anuncios WHERE Id = @AnuncioId)
    BEGIN
        -- Adicionar 3 imagens para cada anúncio
        INSERT INTO Imagens (ImagemCaminho, AnuncioId)
        VALUES
            ('/imagens/anuncios/' + CAST(@AnuncioId AS VARCHAR(10)) + '/foto-01.jpg', @AnuncioId),
            ('/imagens/anuncios/' + CAST(@AnuncioId AS VARCHAR(10)) + '/foto-02.jpg', @AnuncioId),
            ('/imagens/anuncios/' + CAST(@AnuncioId AS VARCHAR(10)) + '/foto-03.jpg', @AnuncioId);
    END

    SET @AnuncioId = @AnuncioId + 1;
END

GO

-- Verificar quantas imagens foram inseridas
SELECT COUNT(*) as 'Total de Imagens Inseridas' FROM Imagens;
GO

-- Mostrar alguns exemplos
SELECT TOP 10 Id, ImagemCaminho, AnuncioId
FROM Imagens
ORDER BY AnuncioId, Id;
GO

PRINT 'Imagens populadas com sucesso!';
