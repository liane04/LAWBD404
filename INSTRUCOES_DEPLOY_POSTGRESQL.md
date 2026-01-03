# Instruções para Deploy com PostgreSQL no Servidor

## O que foi feito no projeto

✅ **1. Instalado pacote PostgreSQL**
```bash
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 8.0.10
```

✅ **2. Modificado `Program.cs`**
- Adicionada lógica para detectar o provider de base de dados através da configuração `DatabaseProvider`
- Suporta SQL Server (desenvolvimento local) e PostgreSQL (produção)
- SQL direto de emergency migration só executa em SQL Server

✅ **3. Criado `appsettings.Production.json`**
- Configurado com `DatabaseProvider: "PostgreSQL"`
- Connection string apontando para PostgreSQL local no servidor

---

## Como o projeto funciona agora

### **Desenvolvimento Local (Windows)**
- Usa `appsettings.json` → SQL Server LocalDB
- DatabaseProvider não definido → assume "SqlServer" por padrão
- Funciona como sempre funcionou

### **Produção (Servidor Linux)**
- Usa `appsettings.Production.json` → PostgreSQL
- DatabaseProvider = "PostgreSQL"
- Conecta ao PostgreSQL na porta 5432

---

## Passos para fazer Deploy no Servidor

### 1. Fazer upload do projeto atualizado
```bash
# Na tua máquina, fazer commit e push das alterações
git add .
git commit -m "Adicionar suporte PostgreSQL para produção"
git push origin main

# No servidor, fazer pull
cd /var/www/404ride
sudo -u www-data git pull origin main
```

### 2. Garantir que o appsettings.Production.json está presente
```bash
# Verificar se o ficheiro existe
ls -la /var/www/404ride/appsettings.Production.json

# Se não existir, criar com as credenciais corretas
sudo nano /var/www/404ride/appsettings.Production.json
```

Conteúdo do ficheiro:
```json
{
  "DatabaseProvider": "PostgreSQL",
  "ConnectionStrings": {
    "DefaultConnection": "Host=127.0.0.1;Port=5432;Database=marketplace_db;Username=bruno;Password=Minipc2025"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Smtp": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "EnableSsl": true,
    "User": "404ride@gmail.com",
    "Pass": "yvay wxoq jyih abgl",
    "From": "404 RIDE <404ride@gmail.com>"
  },
  "Stripe": {
    "PublishableKey": "pk_test_51Scp8537eBCb733C08BQk6vcDN13jOcilUuLPY1TYxlbEXRQ10I1VMzZkXdiy5YWcj4oQNNJ8ABlew5p8Z8vhQPg00ppR0E6KH",
    "SecretKey": "sk_test_51Scp8537eBCb733CTqJuaOZ3eOMXAzFuUEcBRF4Iy1qh4fmgW5G3jkSQcRK1cU971fD3tWGvUluJXnJeQhpBZj7t00CFwlR6zC",
    "Currency": "eur",
    "ReservaValorPercentagem": 10
  }
}
```

### 3. Instalar .NET EF Tools (se ainda não tiver)
```bash
dotnet tool install --global dotnet-ef
# ou atualizar
dotnet tool update --global dotnet-ef
```

### 4. Aplicar migrations ao PostgreSQL
```bash
cd /var/www/404ride

# Garantir que o ambiente é Production
export ASPNETCORE_ENVIRONMENT=Production

# Aplicar migrations
dotnet ef database update --project Marketplace.csproj

# OU se preferires usar o próprio runtime da app
# (a app faz db.Database.Migrate() no startup em Development)
# Mas em Production, é melhor fazer manualmente
```

**IMPORTANTE**: Se deres erro de permissões, usar:
```bash
sudo -u www-data dotnet ef database update --project Marketplace.csproj
```

### 5. Verificar se a base de dados foi criada
```bash
# Entrar no PostgreSQL
sudo -u postgres psql

# Conectar à base de dados
\c marketplace_db

# Listar tabelas (deve aparecer AspNetUsers, Anuncios, etc.)
\dt

# Ver detalhes de uma tabela
\d "AspNetUsers"

# Sair
\q
```

### 6. Compilar a aplicação
```bash
cd /var/www/404ride
sudo -u www-data dotnet publish -c Release -o /var/www/404ride/publish
```

### 7. Configurar permissões
```bash
sudo chown -R www-data:www-data /var/www/404ride
sudo chmod -R 755 /var/www/404ride
```

### 8. Reiniciar o serviço
```bash
sudo systemctl restart 404ride.service

# Verificar status
sudo systemctl status 404ride.service

# Ver logs em tempo real
sudo journalctl -fu 404ride.service
```

---

## Troubleshooting

### Erro: "Npgsql not found"
```bash
cd /var/www/404ride
dotnet restore
```

### Erro: "Password authentication failed for user bruno"
Verificar a password no appsettings.Production.json e no PostgreSQL:
```bash
sudo -u postgres psql
ALTER USER bruno WITH PASSWORD 'Minipc2025';
\q
```

### Erro: "Database does not exist"
Criar manualmente:
```bash
sudo -u postgres psql -c "CREATE DATABASE marketplace_db OWNER bruno;"
```

### Erro: "Could not connect to server"
Verificar se o PostgreSQL está a correr:
```bash
sudo systemctl status postgresql
sudo systemctl start postgresql
```

### Ver logs da aplicação
```bash
# Logs do systemd
sudo journalctl -fu 404ride.service

# Logs do Nginx (se tiver)
sudo tail -f /var/log/nginx/error.log
sudo tail -f /var/log/nginx/access.log
```

---

## Verificação Final

1. Aceder ao site: https://404ride.b-host.me/
2. Verificar que não aparece o erro 500
3. Tentar fazer login com um utilizador de teste
4. Verificar se os anúncios aparecem

Se tudo funcionar, o deployment está completo! 🎉

---

## Notas Importantes

- **Não executar seeding em Production**: O seeding só funciona em Development (ver Program.cs linha 137)
- **Migrations são automáticas**: O código faz `db.Database.Migrate()` ao iniciar
- **Dados de teste**: Vais precisar criar dados manualmente ou fazer seeding uma vez em Development no servidor
- **Backup**: Sempre fazer backup da BD antes de aplicar migrations em produção

```bash
# Backup PostgreSQL
sudo -u postgres pg_dump marketplace_db > backup_$(date +%Y%m%d_%H%M%S).sql

# Restore
sudo -u postgres psql marketplace_db < backup_YYYYMMDD_HHMMSS.sql
```
