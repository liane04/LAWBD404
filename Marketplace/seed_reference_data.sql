-- ===================================
-- SEED REFERENCE DATA
-- ===================================

USE MarketplaceDB;
GO

-- Limpar dados existentes (com ordem correta de dependências)
DELETE FROM Modelos;
DELETE FROM Marcas;
DELETE FROM Tipos WHERE Id > 1;
DELETE FROM Combustiveis;
DELETE FROM Categorias;
GO

-- ===================================
-- 1. TIPOS DE VEÍCULOS
-- ===================================
SET IDENTITY_INSERT Tipos ON;

-- Atualizar o existente e adicionar novos
UPDATE Tipos SET Nome = 'Ligeiro' WHERE Id = 1;

INSERT INTO Tipos (Id, Nome) VALUES
(2, 'SUV'),
(3, 'Carrinha'),
(4, 'Monovolume'),
(5, 'Desportivo'),
(6, 'Citadino'),
(7, 'Coupé'),
(8, 'Cabriolet');

SET IDENTITY_INSERT Tipos OFF;
GO

-- ===================================
-- 2. COMBUSTÍVEIS
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

-- ===================================
-- 3. CATEGORIAS
-- ===================================
SET IDENTITY_INSERT Categorias ON;

INSERT INTO Categorias (Id, Nome) VALUES
(1, 'Executivo'),
(2, 'Familiar'),
(3, 'Compacto'),
(4, 'Premium'),
(5, 'Económico'),
(6, 'Utilitário'),
(7, 'Performance');

SET IDENTITY_INSERT Categorias OFF;
GO

-- ===================================
-- 4. MARCAS
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
-- 5. MODELOS (com TipoId correspondente)
-- ===================================
SET IDENTITY_INSERT Modelos ON;

-- AUDI (MarcaId = 1)
INSERT INTO Modelos (Id, Nome, MarcaId, TipoId) VALUES
(1, 'A1', 1, 6),        -- Citadino
(2, 'A3', 1, 3),        -- Compacto/Ligeiro
(3, 'A4', 1, 1),        -- Ligeiro
(4, 'A5', 1, 7),        -- Coupé
(5, 'A6', 1, 1),        -- Ligeiro
(6, 'Q2', 1, 2),        -- SUV
(7, 'Q3', 1, 2),        -- SUV
(8, 'Q5', 1, 2),        -- SUV
(9, 'Q7', 1, 2),        -- SUV
(10, 'Q8', 1, 2),       -- SUV

-- BMW (MarcaId = 2)
(11, 'Série 1', 2, 6),  -- Citadino
(12, 'Série 2', 2, 1),  -- Ligeiro
(13, 'Série 3', 2, 1),  -- Ligeiro
(14, 'Série 4', 2, 7),  -- Coupé
(15, 'Série 5', 2, 1),  -- Ligeiro
(16, 'Série 7', 2, 1),  -- Ligeiro
(17, 'X1', 2, 2),       -- SUV
(18, 'X3', 2, 2),       -- SUV
(19, 'X5', 2, 2),       -- SUV
(20, 'Z4', 2, 8),       -- Cabriolet

-- MERCEDES-BENZ (MarcaId = 3)
(21, 'Classe A', 3, 6), -- Citadino
(22, 'Classe B', 3, 4), -- Monovolume
(23, 'Classe C', 3, 1), -- Ligeiro
(24, 'Classe E', 3, 1), -- Ligeiro
(25, 'Classe S', 3, 1), -- Ligeiro
(26, 'GLA', 3, 2),      -- SUV
(27, 'GLC', 3, 2),      -- SUV
(28, 'GLE', 3, 2),      -- SUV
(29, 'GLS', 3, 2),      -- SUV
(30, 'CLA', 3, 7),      -- Coupé

-- VOLKSWAGEN (MarcaId = 4)
(31, 'Up!', 4, 6),      -- Citadino
(32, 'Polo', 4, 6),     -- Citadino
(33, 'Golf', 4, 3),     -- Compacto
(34, 'Passat', 4, 1),   -- Ligeiro
(35, 'Arteon', 4, 1),   -- Ligeiro
(36, 'T-Cross', 4, 2),  -- SUV
(37, 'T-Roc', 4, 2),    -- SUV
(38, 'Tiguan', 4, 2),   -- SUV
(39, 'Touareg', 4, 2),  -- SUV
(40, 'Sharan', 4, 4),   -- Monovolume

-- RENAULT (MarcaId = 5)
(41, 'Twingo', 5, 6),   -- Citadino
(42, 'Clio', 5, 6),     -- Citadino
(43, 'Captur', 5, 2),   -- SUV
(44, 'Mégane', 5, 3),   -- Compacto
(45, 'Kadjar', 5, 2),   -- SUV
(46, 'Koleos', 5, 2),   -- SUV
(47, 'Talisman', 5, 1), -- Ligeiro
(48, 'Scénic', 5, 4),   -- Monovolume
(49, 'Espace', 5, 4),   -- Monovolume
(50, 'Zoe', 5, 6),      -- Citadino (Elétrico)

-- PEUGEOT (MarcaId = 6)
(51, '108', 6, 6),      -- Citadino
(52, '208', 6, 6),      -- Citadino
(53, '2008', 6, 2),     -- SUV
(54, '308', 6, 3),      -- Compacto
(55, '3008', 6, 2),     -- SUV
(56, '508', 6, 1),      -- Ligeiro
(57, '5008', 6, 2),     -- SUV
(58, 'Rifter', 6, 4),   -- Monovolume
(59, 'e-208', 6, 6),    -- Citadino (Elétrico)
(60, 'e-2008', 6, 2),   -- SUV (Elétrico)

-- CITROËN (MarcaId = 7)
(61, 'C1', 7, 6),       -- Citadino
(62, 'C3', 7, 6),       -- Citadino
(63, 'C3 Aircross', 7, 2), -- SUV
(64, 'C4', 7, 3),       -- Compacto
(65, 'C5 Aircross', 7, 2), -- SUV
(66, 'Berlingo', 7, 4), -- Monovolume
(67, 'SpaceTourer', 7, 4), -- Monovolume

-- FORD (MarcaId = 8)
(68, 'Fiesta', 8, 6),   -- Citadino
(69, 'Focus', 8, 3),    -- Compacto
(70, 'Puma', 8, 2),     -- SUV
(71, 'Kuga', 8, 2),     -- SUV
(72, 'Mondeo', 8, 1),   -- Ligeiro
(73, 'Mustang', 8, 5),  -- Desportivo
(74, 'Explorer', 8, 2), -- SUV
(75, 'Transit Custom', 8, 6), -- Utilitário

-- OPEL (MarcaId = 9)
(76, 'Corsa', 9, 6),    -- Citadino
(77, 'Astra', 9, 3),    -- Compacto
(78, 'Crossland', 9, 2),-- SUV
(79, 'Grandland', 9, 2),-- SUV
(80, 'Insignia', 9, 1), -- Ligeiro
(81, 'Mokka', 9, 2),    -- SUV
(82, 'Combo Life', 9, 4), -- Monovolume

-- TOYOTA (MarcaId = 10)
(83, 'Aygo', 10, 6),    -- Citadino
(84, 'Yaris', 10, 6),   -- Citadino
(85, 'Corolla', 10, 3), -- Compacto
(86, 'C-HR', 10, 2),    -- SUV
(87, 'RAV4', 10, 2),    -- SUV
(88, 'Camry', 10, 1),   -- Ligeiro
(89, 'Highlander', 10, 2), -- SUV
(90, 'Prius', 10, 3),   -- Compacto (Híbrido)

-- NISSAN (MarcaId = 11)
(91, 'Micra', 11, 6),   -- Citadino
(92, 'Juke', 11, 2),    -- SUV
(93, 'Qashqai', 11, 2), -- SUV
(94, 'X-Trail', 11, 2), -- SUV
(95, 'Leaf', 11, 6),    -- Citadino (Elétrico)
(96, 'Ariya', 11, 2),   -- SUV (Elétrico)

-- HYUNDAI (MarcaId = 12)
(97, 'i10', 12, 6),     -- Citadino
(98, 'i20', 12, 6),     -- Citadino
(99, 'i30', 12, 3),     -- Compacto
(100, 'Kauai', 12, 2),  -- SUV
(101, 'Tucson', 12, 2), -- SUV
(102, 'Santa Fe', 12, 2), -- SUV
(103, 'Ioniq 5', 12, 2),  -- SUV (Elétrico)

-- KIA (MarcaId = 13)
(104, 'Picanto', 13, 6),-- Citadino
(105, 'Rio', 13, 6),    -- Citadino
(106, 'Ceed', 13, 3),   -- Compacto
(107, 'XCeed', 13, 2),  -- SUV
(108, 'Sportage', 13, 2),-- SUV
(109, 'Sorento', 13, 2),-- SUV
(110, 'EV6', 13, 2),    -- SUV (Elétrico)

-- SEAT (MarcaId = 14)
(111, 'Mii', 14, 6),    -- Citadino
(112, 'Ibiza', 14, 6),  -- Citadino
(113, 'Leon', 14, 3),   -- Compacto
(114, 'Arona', 14, 2),  -- SUV
(115, 'Ateca', 14, 2),  -- SUV
(116, 'Tarraco', 14, 2),-- SUV

-- SKODA (MarcaId = 15)
(117, 'Citigo', 15, 6), -- Citadino
(118, 'Fabia', 15, 6),  -- Citadino
(119, 'Scala', 15, 3),  -- Compacto
(120, 'Octavia', 15, 3),-- Compacto
(121, 'Kamiq', 15, 2),  -- SUV
(122, 'Karoq', 15, 2),  -- SUV
(123, 'Kodiaq', 15, 2), -- SUV
(124, 'Superb', 15, 1), -- Ligeiro

-- VOLVO (MarcaId = 16)
(125, 'V40', 16, 3),    -- Compacto
(126, 'V60', 16, 3),    -- Carrinha
(127, 'V90', 16, 3),    -- Carrinha
(128, 'XC40', 16, 2),   -- SUV
(129, 'XC60', 16, 2),   -- SUV
(130, 'XC90', 16, 2),   -- SUV

-- FIAT (MarcaId = 17)
(131, '500', 17, 6),    -- Citadino
(132, 'Panda', 17, 6),  -- Citadino
(133, 'Tipo', 17, 3),   -- Compacto
(134, '500X', 17, 2),   -- SUV
(135, 'Ducato', 17, 6), -- Utilitário

-- HONDA (MarcaId = 18)
(136, 'Jazz', 18, 6),   -- Citadino
(137, 'Civic', 18, 3),  -- Compacto
(138, 'CR-V', 18, 2),   -- SUV
(139, 'HR-V', 18, 2),   -- SUV
(140, 'e', 18, 6),      -- Citadino (Elétrico)

-- MAZDA (MarcaId = 19)
(141, 'Mazda2', 19, 6), -- Citadino
(142, 'Mazda3', 19, 3), -- Compacto
(143, 'CX-3', 19, 2),   -- SUV
(144, 'CX-5', 19, 2),   -- SUV
(145, 'CX-30', 19, 2),  -- SUV
(146, 'MX-5', 19, 8),   -- Cabriolet

-- TESLA (MarcaId = 20)
(147, 'Model 3', 20, 1),-- Ligeiro (Elétrico)
(148, 'Model S', 20, 1),-- Ligeiro (Elétrico)
(149, 'Model X', 20, 2),-- SUV (Elétrico)
(150, 'Model Y', 20, 2);-- SUV (Elétrico)

SET IDENTITY_INSERT Modelos OFF;
GO

PRINT '✅ Dados de referência inseridos com sucesso!';
PRINT '';
PRINT 'RESUMO:';
PRINT '- Tipos: 8 registos';
PRINT '- Combustíveis: 8 registos';
PRINT '- Categorias: 7 registos';
PRINT '- Marcas: 20 registos';
PRINT '- Modelos: 150 registos';
GO
