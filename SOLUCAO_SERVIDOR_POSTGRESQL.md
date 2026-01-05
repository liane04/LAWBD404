# Solução para Erro no Servidor - PostgreSQL Timestamp

## Problema Identificado

```
System.NotSupportedException: Cannot apply binary operation on types
'timestamp with time zone' and 'timestamp without time zone'
```

**Causa:** PostgreSQL trata DateTime de forma diferente do SQL Server. Por padrão, o Npgsql 8.0+ usa `timestamp with time zone`, mas o código usa `DateTime` sem timezone.

## Solução: Adicionar Configuração de Timestamp

### Passo 1: Modificar Program.cs

Adicionar esta linha **ANTES** de configurar o DbContext:

```csharp
// ADICIONAR NO INÍCIO DO Program.cs (antes de var builder = ...)
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);
// resto do código...
```

Isto força o Npgsql a usar `timestamp` (sem timezone) em vez de `timestamptz`.

---

## Passos Completos no Servidor

### 1. Parar o serviço
```bash
sudo systemctl stop 404ride.service
```

### 2. Fazer pull das alterações
```bash
cd /var/www/404ride/app
sudo -u www-data git pull origin main
```

### 3. Restore packages
```bash
cd /var/www/404ride/app
sudo -u www-data dotnet restore
```

### 4. Limpar build anterior
```bash
sudo -u www-data dotnet clean
```

### 5. Publish para produção
```bash
sudo -u www-data dotnet publish -c Release -o /var/www/404ride/publish
```

**IMPORTANTE:** Isto copia TODAS as DLLs necessárias para `/var/www/404ride/publish`

### 6. Verificar appsettings.Production.json
```bash
cat /var/www/404ride/publish/appsettings.Production.json
```

Deve conter:
```json
{
  "DatabaseProvider": "PostgreSQL",
  "ConnectionStrings": {
    "DefaultConnection": "Host=127.0.0.1;Port=5432;Database=marketplace_db;Username=bruno;Password=Minipc2025"
  }
}
```

### 7. Atualizar 404ride.service

Verificar que o serviço aponta para `/var/www/404ride/publish`:

```bash
sudo nano /etc/systemd/system/404ride.service
```

Deve ter:
```ini
[Service]
WorkingDirectory=/var/www/404ride/publish
ExecStart=/usr/bin/dotnet /var/www/404ride/publish/Marketplace.dll
```

Se mudaste, fazer reload:
```bash
sudo systemctl daemon-reload
```

### 8. Definir ambiente Production
```bash
sudo nano /etc/systemd/system/404ride.service
```

Adicionar/verificar:
```ini
[Service]
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false
```

### 9. Recarregar e iniciar
```bash
sudo systemctl daemon-reload
sudo systemctl start 404ride.service
```

### 10. Verificar status
```bash
sudo systemctl status 404ride.service --no-pager -l
sudo journalctl -u 404ride.service -n 50 --no-pager
```

---

## Se Ainda Houver Erros de Timestamp

Se o erro `timestamp with time zone` persistir, há uma alternativa:

### Opção 2: Configurar no ApplicationDbContext

Editar `Data/ApplicationDbContext.cs` e adicionar:

```csharp
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    base.OnConfiguring(optionsBuilder);

    // Força uso de timestamp sem timezone
    if (optionsBuilder.IsConfigured)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }
}
```

---

## Verificações Finais

### 1. Verificar logs em tempo real
```bash
sudo journalctl -u 404ride.service -f
```

### 2. Verificar conexão PostgreSQL
```bash
sudo -u postgres psql -d marketplace_db -c "SELECT current_database(), current_user;"
```

### 3. Testar site
```bash
curl http://localhost:5000
```

ou

```bash
curl https://404ride.b-host.me
```

---

## Resumo dos Comandos (Copy-Paste)

```bash
# Parar serviço
sudo systemctl stop 404ride.service

# Pull código
cd /var/www/404ride/app
sudo -u www-data git pull origin main

# Clean e Restore
sudo -u www-data dotnet clean
sudo -u www-data dotnet restore

# Publish
sudo -u www-data dotnet publish -c Release -o /var/www/404ride/publish

# Aplicar migrations (se necessário)
cd /var/www/404ride/publish
export ASPNETCORE_ENVIRONMENT=Production
sudo -u www-data dotnet ef database update --no-build

# Verificar serviço
cat /etc/systemd/system/404ride.service

# Reload e start
sudo systemctl daemon-reload
sudo systemctl start 404ride.service

# Ver logs
sudo journalctl -u 404ride.service -n 100 --no-pager
```

---

## Troubleshooting

### Erro: "DLL not found"
**Solução:** Fazer `dotnet publish` em vez de `dotnet build`

### Erro: "timestamp with time zone"
**Solução:** Adicionar `AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);` no Program.cs

### Erro: "Cannot connect to PostgreSQL"
**Solução:** Verificar connection string em `appsettings.Production.json`

### Erro: "Permission denied"
**Solução:**
```bash
sudo chown -R www-data:www-data /var/www/404ride
sudo chmod -R 755 /var/www/404ride
```

### Site mostra erro 500
**Solução:** Ver logs detalhados:
```bash
sudo journalctl -u 404ride.service -n 200 --no-pager -o cat | grep -A 20 "fail:"
```
