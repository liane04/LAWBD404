-- ===================================
-- CORRIGIR ENCODING DE CARACTERES
-- ===================================

USE MarketplaceDB;
GO

-- Corrigir BMW Série
UPDATE Modelos SET Nome = 'Série 1' WHERE Id = 11;
UPDATE Modelos SET Nome = 'Série 2' WHERE Id = 12;
UPDATE Modelos SET Nome = 'Série 3' WHERE Id = 13;
UPDATE Modelos SET Nome = 'Série 4' WHERE Id = 14;
UPDATE Modelos SET Nome = 'Série 5' WHERE Id = 15;
UPDATE Modelos SET Nome = 'Série 7' WHERE Id = 16;

-- Corrigir Renault Mégane e Scénic
UPDATE Modelos SET Nome = 'Mégane' WHERE Id = 38;
UPDATE Modelos SET Nome = 'Scénic' WHERE Id = 40;

-- Corrigir Citroën
UPDATE Modelos SET Nome = 'C3 Aircross' WHERE Id = 51;
UPDATE Modelos SET Nome = 'C5 Aircross' WHERE Id = 53;

-- Corrigir Tipos (Coupé)
UPDATE Tipos SET Nome = 'Coupé' WHERE Id = 7;

PRINT '✅ Encoding corrigido com sucesso!';
GO
