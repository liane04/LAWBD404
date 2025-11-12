-- Script para criar utilizadores padrão
-- Password: 123 para todos

-- Hash gerado com PasswordHasher.HashPassword("123")
-- Password: 123 para todos os utilizadores

DECLARE @hash NVARCHAR(500);
SET @hash = 'PBKDF2$100000$rElYCqPw0yRSNzWXzEfwsw==$1pN+dNUPKXQRrmXOLXeQSOuy8eWiV6IaFLl0toCVkyk=';

-- Inserir Administrador
IF NOT EXISTS (SELECT 1 FROM Utilizador WHERE Email = 'admin@email.com')
BEGIN
    INSERT INTO Utilizador (Username, Email, Nome, PasswordHash, Estado, Tipo, Discriminator, NivelAcesso)
    VALUES ('admin', 'admin@email.com', 'Administrador', @hash, 'Ativo', 'Administrador', 'Administrador', 'Total');
    PRINT 'Administrador criado com sucesso';
END
ELSE
BEGIN
    PRINT 'Administrador já existe';
END

-- Inserir Vendedor
IF NOT EXISTS (SELECT 1 FROM Utilizador WHERE Email = 'vendedor@email.com')
BEGIN
    INSERT INTO Utilizador (Username, Email, Nome, PasswordHash, Estado, Tipo, Discriminator)
    VALUES ('vendedor', 'vendedor@email.com', 'Vendedor Demo', @hash, 'Ativo', 'Vendedor', 'Vendedor');
    PRINT 'Vendedor criado com sucesso';
END
ELSE
BEGIN
    PRINT 'Vendedor já existe';
END

-- Inserir Comprador
IF NOT EXISTS (SELECT 1 FROM Utilizador WHERE Email = 'comprador@email.com')
BEGIN
    INSERT INTO Utilizador (Username, Email, Nome, PasswordHash, Estado, Tipo, Discriminator)
    VALUES ('comprador', 'comprador@email.com', 'Comprador Demo', @hash, 'Ativo', 'Comprador', 'Comprador');
    PRINT 'Comprador criado com sucesso';
END
ELSE
BEGIN
    PRINT 'Comprador já existe';
END

PRINT '';
PRINT 'Script concluído! Utilizadores padrão:';
PRINT 'Admin: admin@email.com / 123';
PRINT 'Vendedor: vendedor@email.com / 123';
PRINT 'Comprador: comprador@email.com / 123';
