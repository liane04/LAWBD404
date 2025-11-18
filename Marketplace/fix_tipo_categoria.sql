-- ===================================
-- CORRIGIR TIPOS E CATEGORIAS
-- ===================================

USE MarketplaceDB;
GO

-- Limpar dados
DELETE FROM Modelos;
DELETE FROM Marcas;
DELETE FROM Tipos;
DELETE FROM Categorias;
GO

-- NOTA: Modelo.TipoId aponta para Tipos (FK constraint)
-- Por isso mantemos categorias em Tipos e tipos de veículos em Categorias

-- ===================================
-- 1. TIPOS (categorias de veículos - SUV, Ligeiro, etc.)
-- ===================================
SET IDENTITY_INSERT Tipos ON;

INSERT INTO Tipos (Id, Nome) VALUES
(1, 'Ligeiro'),
(2, 'SUV'),
(3, 'Carrinha'),
(4, 'Monovolume'),
(5, 'Desportivo'),
(6, 'Citadino'),
(7, 'Coupé'),
(8, 'Cabriolet'),
(9, 'Pick-up'),
(10, 'Comercial');

SET IDENTITY_INSERT Tipos OFF;
GO

-- ===================================
-- 2. CATEGORIAS (tipo de veículo - Carro, Moto, etc.)
-- ===================================
SET IDENTITY_INSERT Categorias ON;

INSERT INTO Categorias (Id, Nome) VALUES
(1, 'Carro'),
(2, 'Moto'),
(3, 'Caravana'),
(4, 'Autocaravana'),
(5, 'Comercial'),
(6, 'Clássico');

SET IDENTITY_INSERT Categorias OFF;
GO

-- ===================================
-- 3. MARCAS
-- ===================================
SET IDENTITY_INSERT Marcas ON;

INSERT INTO Marcas (Id, Nome) VALUES
(1, 'Audi'),
(2, 'BMW'),
(3, 'Mercedes-Benz'),
(4, 'Volkswagen'),
(5, 'Renault'),
(6, 'Peugeot'),
(7, 'Citroën'),
(8, 'Ford'),
(9, 'Opel'),
(10, 'Toyota'),
(11, 'Nissan'),
(12, 'Hyundai'),
(13, 'Kia'),
(14, 'Seat'),
(15, 'Skoda'),
(16, 'Volvo'),
(17, 'Fiat'),
(18, 'Honda'),
(19, 'Mazda'),
(20, 'Tesla');

SET IDENTITY_INSERT Marcas OFF;
GO

-- ===================================
-- 4. MODELOS (com TipoId = CategoriaId correta)
-- TipoId no modelo representa a CATEGORIA (SUV, Ligeiro, etc.)
-- ===================================
SET IDENTITY_INSERT Modelos ON;

-- AUDI
INSERT INTO Modelos (Id, Nome, MarcaId, TipoId) VALUES
(1, 'A1', 1, 6),        -- Citadino
(2, 'A3', 1, 1),        -- Ligeiro
(3, 'A4', 1, 1),        -- Ligeiro
(4, 'A5', 1, 7),        -- Coupé
(5, 'A6', 1, 1),        -- Ligeiro
(6, 'Q2', 1, 2),        -- SUV
(7, 'Q3', 1, 2),        -- SUV
(8, 'Q5', 1, 2),        -- SUV
(9, 'Q7', 1, 2),        -- SUV
(10, 'Q8', 1, 2),       -- SUV

-- BMW
(11, 'Série 1', 2, 6),  -- Citadino
(12, 'Série 2', 2, 1),  -- Ligeiro
(13, 'Série 3', 2, 1),  -- Ligeiro
(14, 'Série 4', 2, 7),  -- Coupé
(15, 'Série 5', 2, 1),  -- Ligeiro
(16, 'X1', 2, 2),       -- SUV
(17, 'X3', 2, 2),       -- SUV
(18, 'X5', 2, 2),       -- SUV
(19, 'Z4', 2, 8),       -- Cabriolet

-- MERCEDES-BENZ
(20, 'Classe A', 3, 6), -- Citadino
(21, 'Classe B', 3, 4), -- Monovolume
(22, 'Classe C', 3, 1), -- Ligeiro
(23, 'Classe E', 3, 1), -- Ligeiro
(24, 'GLA', 3, 2),      -- SUV
(25, 'GLC', 3, 2),      -- SUV
(26, 'GLE', 3, 2),      -- SUV

-- VOLKSWAGEN
(27, 'Up!', 4, 6),      -- Citadino
(28, 'Polo', 4, 6),     -- Citadino
(29, 'Golf', 4, 1),     -- Ligeiro
(30, 'Passat', 4, 1),   -- Ligeiro
(31, 'T-Cross', 4, 2),  -- SUV
(32, 'T-Roc', 4, 2),    -- SUV
(33, 'Tiguan', 4, 2),   -- SUV
(34, 'Sharan', 4, 4),   -- Monovolume

-- RENAULT
(35, 'Twingo', 5, 6),   -- Citadino
(36, 'Clio', 5, 6),     -- Citadino
(37, 'Captur', 5, 2),   -- SUV
(38, 'Mégane', 5, 1),   -- Ligeiro
(39, 'Kadjar', 5, 2),   -- SUV
(40, 'Scénic', 5, 4),   -- Monovolume
(41, 'Zoe', 5, 6),      -- Citadino

-- PEUGEOT
(42, '108', 6, 6),      -- Citadino
(43, '208', 6, 6),      -- Citadino
(44, '2008', 6, 2),     -- SUV
(45, '308', 6, 1),      -- Ligeiro
(46, '3008', 6, 2),     -- SUV
(47, '508', 6, 1),      -- Ligeiro
(48, '5008', 6, 2),     -- SUV

-- CITROËN
(49, 'C1', 7, 6),       -- Citadino
(50, 'C3', 7, 6),       -- Citadino
(51, 'C3 Aircross', 7, 2), -- SUV
(52, 'C4', 7, 1),       -- Ligeiro
(53, 'C5 Aircross', 7, 2), -- SUV
(54, 'Berlingo', 7, 4), -- Monovolume

-- FORD
(55, 'Fiesta', 8, 6),   -- Citadino
(56, 'Focus', 8, 1),    -- Ligeiro
(57, 'Puma', 8, 2),     -- SUV
(58, 'Kuga', 8, 2),     -- SUV
(59, 'Mustang', 8, 5),  -- Desportivo

-- OPEL
(60, 'Corsa', 9, 6),    -- Citadino
(61, 'Astra', 9, 1),    -- Ligeiro
(62, 'Crossland', 9, 2),-- SUV
(63, 'Grandland', 9, 2),-- SUV
(64, 'Mokka', 9, 2),    -- SUV

-- TOYOTA
(65, 'Aygo', 10, 6),    -- Citadino
(66, 'Yaris', 10, 6),   -- Citadino
(67, 'Corolla', 10, 1), -- Ligeiro
(68, 'C-HR', 10, 2),    -- SUV
(69, 'RAV4', 10, 2),    -- SUV
(70, 'Prius', 10, 1),   -- Ligeiro

-- NISSAN
(71, 'Micra', 11, 6),   -- Citadino
(72, 'Juke', 11, 2),    -- SUV
(73, 'Qashqai', 11, 2), -- SUV
(74, 'X-Trail', 11, 2), -- SUV
(75, 'Leaf', 11, 6),    -- Citadino

-- HYUNDAI
(76, 'i10', 12, 6),     -- Citadino
(77, 'i20', 12, 6),     -- Citadino
(78, 'i30', 12, 1),     -- Ligeiro
(79, 'Kauai', 12, 2),   -- SUV
(80, 'Tucson', 12, 2),  -- SUV

-- KIA
(81, 'Picanto', 13, 6), -- Citadino
(82, 'Rio', 13, 6),     -- Citadino
(83, 'Ceed', 13, 1),    -- Ligeiro
(84, 'Sportage', 13, 2),-- SUV
(85, 'Sorento', 13, 2), -- SUV

-- SEAT
(86, 'Mii', 14, 6),     -- Citadino
(87, 'Ibiza', 14, 6),   -- Citadino
(88, 'Leon', 14, 1),    -- Ligeiro
(89, 'Arona', 14, 2),   -- SUV
(90, 'Ateca', 14, 2),   -- SUV

-- SKODA
(91, 'Citigo', 15, 6),  -- Citadino
(92, 'Fabia', 15, 6),   -- Citadino
(93, 'Octavia', 15, 1), -- Ligeiro
(94, 'Kamiq', 15, 2),   -- SUV
(95, 'Karoq', 15, 2),   -- SUV

-- VOLVO
(96, 'V40', 16, 1),     -- Ligeiro
(97, 'V60', 16, 3),     -- Carrinha
(98, 'XC40', 16, 2),    -- SUV
(99, 'XC60', 16, 2),    -- SUV

-- FIAT
(100, '500', 17, 6),    -- Citadino
(101, 'Panda', 17, 6),  -- Citadino
(102, 'Tipo', 17, 1),   -- Ligeiro
(103, '500X', 17, 2),   -- SUV

-- HONDA
(104, 'Jazz', 18, 6),   -- Citadino
(105, 'Civic', 18, 1),  -- Ligeiro
(106, 'CR-V', 18, 2),   -- SUV

-- MAZDA
(107, 'Mazda2', 19, 6), -- Citadino
(108, 'Mazda3', 19, 1), -- Ligeiro
(109, 'CX-3', 19, 2),   -- SUV
(110, 'CX-5', 19, 2),   -- SUV
(111, 'MX-5', 19, 8),   -- Cabriolet

-- TESLA
(112, 'Model 3', 20, 1),-- Ligeiro
(113, 'Model S', 20, 1),-- Ligeiro
(114, 'Model X', 20, 2),-- SUV
(115, 'Model Y', 20, 2);-- SUV

SET IDENTITY_INSERT Modelos OFF;
GO

-- ===================================
-- ADICIONAR COMBUSTÍVEIS
-- ===================================
SET IDENTITY_INSERT Combustiveis ON;

INSERT INTO Combustiveis (Id, Tipo) VALUES
(1, 'Gasolina'),
(2, 'Diesel'),
(3, 'Elétrico'),
(4, 'Híbrido Gasolina'),
(5, 'Híbrido Diesel'),
(6, 'Plug-in Híbrido'),
(7, 'GPL'),
(8, 'GNV');

SET IDENTITY_INSERT Combustiveis OFF;
GO

PRINT '✅ Estrutura corrigida!';
PRINT '';
PRINT '⚠️  NOTA: Devido à estrutura do modelo:';
PRINT '- Tabela TIPOS contém: SUV, Ligeiro, Citadino, etc. (categoria do veículo)';
PRINT '- Tabela CATEGORIAS contém: Carro, Moto, Caravana, etc. (tipo de veículo)';
PRINT '';
PRINT '📋 No formulário:';
PRINT '- Campo CATEGORIA (topo): Seleciona Carro/Moto/etc.';
PRINT '- Campo TIPO: Preenchido automaticamente (SUV/Ligeiro/etc.)';
PRINT '';
PRINT 'RESUMO:';
PRINT '- Tipos (categorias veículo): 10 registos';
PRINT '- Categorias (tipo veículo): 6 registos';
PRINT '- Combustíveis: 8 registos';
PRINT '- Marcas: 20 registos';
PRINT '- Modelos: 115 registos';
GO
