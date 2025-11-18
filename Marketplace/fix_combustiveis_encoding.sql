-- ===================================
-- CORRIGIR ENCODING DOS COMBUSTÍVEIS
-- ===================================

USE MarketplaceDB;
GO

UPDATE Combustiveis SET Tipo = 'Gasolina' WHERE Id = 1;
UPDATE Combustiveis SET Tipo = 'Diesel' WHERE Id = 2;
UPDATE Combustiveis SET Tipo = 'Elétrico' WHERE Id = 3;
UPDATE Combustiveis SET Tipo = 'Híbrido Gasolina' WHERE Id = 4;
UPDATE Combustiveis SET Tipo = 'Híbrido Diesel' WHERE Id = 5;
UPDATE Combustiveis SET Tipo = 'Plug-in Híbrido' WHERE Id = 6;
UPDATE Combustiveis SET Tipo = 'GPL' WHERE Id = 7;
UPDATE Combustiveis SET Tipo = 'GNV' WHERE Id = 8;

PRINT 'Combustiveis corrigidos com sucesso!';
GO
