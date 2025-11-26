-- Script para corrigir encoding dos tipos de combustível
-- Executa este script no SQL Server Management Studio ou via sqlcmd

USE MarketplaceDb;
GO

-- Atualizar tipos de combustível com encoding correto
UPDATE Combustivel SET Tipo = 'Elétrico' WHERE Tipo LIKE '%l%trico%' OR Tipo = 'ElÃ©trico';
UPDATE Combustivel SET Tipo = 'Gasolina' WHERE Tipo LIKE 'Gasolin%';
UPDATE Combustivel SET Tipo = 'Diesel' WHERE Tipo = 'Diesel';
UPDATE Combustivel SET Tipo = 'Híbrido' WHERE Tipo LIKE '%brido%' OR Tipo = 'HÃ­brido';
UPDATE Combustivel SET Tipo = 'GPL' WHERE Tipo = 'GPL';
UPDATE Combustivel SET Tipo = 'GNV' WHERE Tipo = 'GNV';
UPDATE Combustivel SET Tipo = 'Hidrogénio' WHERE Tipo LIKE '%drog%nio%' OR Tipo LIKE 'Hidrog%nio';

-- Verificar resultados
SELECT Id, Tipo FROM Combustivel ORDER BY Id;
GO
