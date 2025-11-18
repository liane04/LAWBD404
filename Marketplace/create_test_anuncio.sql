-- ===================================
-- CRIAR ANÚNCIO DE TESTE
-- ===================================

USE MarketplaceDB;
GO

-- Obter o VendedorId do vendedor de teste
DECLARE @VendedorId INT;
SELECT @VendedorId = Id FROM Utilizador WHERE Email = 'vendedor@email.com' AND Discriminator = 'Vendedor';

IF @VendedorId IS NOT NULL
BEGIN
    -- Inserir anúncio de teste
    INSERT INTO Anuncios (
        Titulo, Preco, Ano, Cor, Descricao, Quilometragem, Caixa,
        MarcaId, ModeloId, CategoriaId, CombustivelId, TipoId,
        Localizacao, Portas, Lugares, Potencia, Cilindrada,
        VendedorId, n_visualizacoes
    )
    VALUES (
        'BMW Série 3 320d Pack M',  -- Titulo
        28500.00,                    -- Preco
        2019,                        -- Ano
        'Preto',                     -- Cor
        'Excelente estado. Revisões em dia. Único dono. Pack M completo.',  -- Descricao
        95000,                       -- Quilometragem
        'Automática',                -- Caixa
        2,                           -- MarcaId (BMW)
        13,                          -- ModeloId (Série 3)
        1,                           -- CategoriaId (Carro)
        2,                           -- CombustivelId (Diesel)
        1,                           -- TipoId (Ligeiro)
        'Lisboa',                    -- Localizacao
        4,                           -- Portas
        5,                           -- Lugares
        190,                         -- Potencia
        1995,                        -- Cilindrada
        @VendedorId,                 -- VendedorId
        0                            -- NVisualizacoes
    );

    PRINT '✅ Anúncio de teste criado com sucesso!';
    PRINT 'VendedorId: ' + CAST(@VendedorId AS VARCHAR);
END
ELSE
BEGIN
    PRINT '❌ Vendedor não encontrado!';
END
GO
