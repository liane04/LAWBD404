-- Corrigir ligações entre Identity e Domínio

-- 1. Comprador
UPDATE Utilizador
SET IdentityUserId = (SELECT Id FROM AspNetUsers WHERE Email = 'comprador@email.com')
WHERE Email = 'comprador@email.com' AND Discriminator = 'Comprador';

-- 2. Vendedor
UPDATE Utilizador
SET IdentityUserId = (SELECT Id FROM AspNetUsers WHERE Email = 'vendedor@email.com')
WHERE Email = 'vendedor@email.com' AND Discriminator = 'Vendedor';

-- 3. Administrador
UPDATE Utilizador
SET IdentityUserId = (SELECT Id FROM AspNetUsers WHERE Email = 'admin@email.com')
WHERE Email = 'admin@email.com' AND Discriminator = 'Administrador';

-- Verificar resultados
SELECT Id, Discriminator, Nome, Email, IdentityUserId
FROM Utilizador
WHERE Email IN ('comprador@email.com', 'vendedor@email.com', 'admin@email.com')
ORDER BY Email;
