# Guia Completo: Configurar Git no Servidor

## Passo 1: Verificar Repositório GitHub

**Na tua máquina (Windows):**

```bash
# Ver URL do repositório GitHub
cd C:\Users\bruno\Desktop\utad\a_1_semestre_3_ano\Laboratotio_web_bd\app
git remote -v
```

Vai aparecer algo como:
```
origin  https://github.com/SEU_USUARIO/404ride.git (fetch)
origin  https://github.com/SEU_USUARIO/404ride.git (push)
```

**✏️ ANOTA O URL DO TEU REPOSITÓRIO AQUI:**
```
URL: _________________________________
```

---

## Passo 2: Fazer Push de Todas as Alterações

**Antes de configurar o servidor, garante que o GitHub tem tudo:**

```bash
# Na tua máquina
cd C:\Users\bruno\Desktop\utad\a_1_semestre_3_ano\Laboratotio_web_bd\app

# Ver status
git status

# Adicionar todas as alterações
git add .

# Commit
git commit -m "Adicionar suporte PostgreSQL e corrigir versões EF Core 8.0.10"

# Push para GitHub
git push origin main
```

---

## Passo 3: Configurar Git no Servidor

**Conectar ao servidor via SSH:**

```bash
ssh bruno@hpserver
# ou
ssh bruno@SEU_IP
```

**Instalar Git (se ainda não tiver):**

```bash
# Verificar se git está instalado
git --version

# Se não estiver, instalar
sudo apt update
sudo apt install git -y

# Verificar versão
git --version
```

---

## Passo 4: Fazer Backup do Código Atual

```bash
# Ver o que existe atualmente
ls -la /var/www/404ride

# Fazer backup (por segurança)
sudo cp -r /var/www/404ride /var/www/404ride.backup.$(date +%Y%m%d_%H%M%S)

# Verificar backup
ls -la /var/www/
```

---

## Passo 5: Configurar Estrutura de Diretórios

```bash
# Criar estrutura recomendada
sudo mkdir -p /var/www/404ride/app
sudo mkdir -p /var/www/404ride/publish

# Mudar owner para www-data
sudo chown -R www-data:www-data /var/www/404ride
```

---

## Passo 6: Clonar Repositório GitHub

### Opção A: Repositório Público (HTTPS)

```bash
# Clonar usando HTTPS
cd /var/www/404ride
sudo -u www-data git clone https://github.com/SEU_USUARIO/SEU_REPO.git app

# Exemplo:
# sudo -u www-data git clone https://github.com/bruno/404ride.git app
```

### Opção B: Repositório Privado (recomendado - SSH)

**Se o repositório for privado, precisas configurar SSH key:**

```bash
# 1. Gerar SSH key no servidor (como www-data)
sudo -u www-data ssh-keygen -t ed25519 -C "404ride@hpserver"

# Quando perguntar onde guardar, pressiona Enter (usar localização default)
# Quando perguntar password, pressiona Enter (sem password)

# 2. Ver a chave pública
sudo cat /var/www/.ssh/id_ed25519.pub

# 3. Copiar a chave pública (toda a linha que começa com ssh-ed25519)
```

**No GitHub (browser):**
1. Ir para https://github.com/settings/keys
2. Clicar "New SSH key"
3. Title: `404ride-hpserver`
4. Key: Colar a chave pública copiada
5. Clicar "Add SSH key"

**Voltar ao servidor:**

```bash
# 4. Testar conexão SSH com GitHub
sudo -u www-data ssh -T git@github.com

# Deve aparecer: "Hi USERNAME! You've successfully authenticated..."

# 5. Clonar o repositório usando SSH
cd /var/www/404ride
sudo -u www-data git clone git@github.com:SEU_USUARIO/SEU_REPO.git app

# Exemplo:
# sudo -u www-data git clone git@github.com:bruno/404ride.git app
```

---

## Passo 7: Verificar Clone

```bash
# Entrar no diretório
cd /var/www/404ride/app

# Verificar branch
git branch

# Ver status
git status

# Verificar ficheiros
ls -la

# Ver estrutura
tree -L 2
# ou se não tiver tree:
ls -R | head -50
```

---

## Passo 8: Configurar Git (opcional mas recomendado)

```bash
# Configurar user (para commits futuros se necessário)
cd /var/www/404ride/app
sudo -u www-data git config user.name "404ride Server"
sudo -u www-data git config user.email "404ride@hpserver"

# Ver configuração
sudo -u www-data git config --list
```

---

## Passo 9: Criar appsettings.Production.json

```bash
# Criar ficheiro de configuração para produção
cd /var/www/404ride/app

sudo -u www-data nano appsettings.Production.json
```

**Colar este conteúdo:**

```json
{
  "DatabaseProvider": "PostgreSQL",
  "ConnectionStrings": {
    "DefaultConnection": "Host=127.0.0.1;Port=5432;Database=marketplace_db;Username=bruno;Password=Minipc2025"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
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

**Guardar:** `Ctrl+O` → Enter → `Ctrl+X`

**Adicionar ao .gitignore (para não fazer commit de passwords):**

```bash
cd /var/www/404ride/app

# Verificar se .gitignore existe
cat .gitignore

# Se não existir ou não tiver appsettings.Production.json, adicionar
echo "appsettings.Production.json" | sudo -u www-data tee -a .gitignore
```

---

## Passo 10: Primeiro Deploy

```bash
# 1. Ir para o diretório do projeto
cd /var/www/404ride/app/Marketplace

# 2. Restore packages
sudo -u www-data dotnet restore

# 3. Clean
sudo -u www-data dotnet clean

# 4. Publish
sudo -u www-data dotnet publish -c Release -o /var/www/404ride/publish

# 5. Verificar output
ls -la /var/www/404ride/publish

# 6. Copiar appsettings.Production.json para publish
sudo -u www-data cp /var/www/404ride/app/appsettings.Production.json /var/www/404ride/publish/
```

---

## Passo 11: Atualizar 404ride.service

```bash
# Editar serviço
sudo nano /etc/systemd/system/404ride.service
```

**Conteúdo deve ser:**

```ini
[Unit]
Description=404ride ASP.NET Core App
After=network.target

[Service]
WorkingDirectory=/var/www/404ride/publish
ExecStart=/usr/bin/dotnet /var/www/404ride/publish/Marketplace.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyscallFilter=~@clock @debug @module @mount @obsolete @reboot @setuid @swap
SyslogIdentifier=404ride
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false
Environment=ASPNETCORE_URLS=http://localhost:5000

[Install]
WantedBy=multi-user.target
```

**Guardar:** `Ctrl+O` → Enter → `Ctrl+X`

---

## Passo 12: Aplicar Migrations e Reiniciar

```bash
# Reload systemd
sudo systemctl daemon-reload

# Parar serviço
sudo systemctl stop 404ride.service

# Aplicar migrations (se necessário)
cd /var/www/404ride/publish
export ASPNETCORE_ENVIRONMENT=Production
sudo -u www-data dotnet ef database update --no-build

# Iniciar serviço
sudo systemctl start 404ride.service

# Verificar status
sudo systemctl status 404ride.service --no-pager -l

# Ver logs
sudo journalctl -u 404ride.service -n 50 --no-pager
```

---

## Passo 13: Criar Script de Deploy Automático

```bash
# Criar script
sudo nano /var/www/404ride/deploy.sh
```

**Conteúdo:**

```bash
#!/bin/bash
set -e

echo "🚀 404 RIDE - Deploy Script"
echo "=============================="

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${YELLOW}📥 Pulling latest code from GitHub...${NC}"
cd /var/www/404ride/app
sudo -u www-data git pull origin main

echo -e "${YELLOW}🧹 Cleaning previous build...${NC}"
cd /var/www/404ride/app/Marketplace
sudo -u www-data dotnet clean

echo -e "${YELLOW}📦 Restoring packages...${NC}"
sudo -u www-data dotnet restore

echo -e "${YELLOW}🔨 Building and publishing...${NC}"
sudo -u www-data dotnet publish -c Release -o /var/www/404ride/publish

echo -e "${YELLOW}📝 Copying production settings...${NC}"
sudo -u www-data cp /var/www/404ride/app/appsettings.Production.json /var/www/404ride/publish/

echo -e "${YELLOW}🔄 Restarting service...${NC}"
sudo systemctl restart 404ride.service

echo -e "${YELLOW}⏳ Waiting for service to start...${NC}"
sleep 3

echo -e "${YELLOW}📊 Checking service status...${NC}"
if sudo systemctl is-active --quiet 404ride.service; then
    echo -e "${GREEN}✅ Deploy successful! Service is running.${NC}"
    sudo systemctl status 404ride.service --no-pager -l | head -20
else
    echo -e "${RED}❌ Deploy failed! Service is not running.${NC}"
    echo -e "${RED}Showing last 30 lines of logs:${NC}"
    sudo journalctl -u 404ride.service -n 30 --no-pager
    exit 1
fi

echo ""
echo -e "${GREEN}🎉 Deploy complete!${NC}"
echo "🌐 Visit: https://404ride.b-host.me"
```

**Dar permissões de execução:**

```bash
sudo chmod +x /var/www/404ride/deploy.sh
```

---

## Passo 14: Testar Deploy

```bash
# Executar script de deploy
sudo /var/www/404ride/deploy.sh
```

---

## Workflow Final: Como Fazer Deploy Agora

### Na Tua Máquina (Windows):

```bash
# 1. Fazer alterações ao código
# 2. Commit e push
git add .
git commit -m "Descrição das alterações"
git push origin main
```

### No Servidor:

```bash
# Executar deploy
sudo /var/www/404ride/deploy.sh
```

**OU manualmente:**

```bash
cd /var/www/404ride/app
sudo -u www-data git pull origin main
cd Marketplace
sudo -u www-data dotnet publish -c Release -o /var/www/404ride/publish
sudo -u www-data cp /var/www/404ride/app/appsettings.Production.json /var/www/404ride/publish/
sudo systemctl restart 404ride.service
```

---

## Troubleshooting

### Erro: "Permission denied (publickey)"
**Solução:** Configurar SSH key (ver Passo 6, Opção B)

### Erro: "fatal: could not read Username"
**Solução:** Usar SSH em vez de HTTPS, ou configurar credentials helper

### Erro: "DLL not found"
**Solução:** Fazer `dotnet restore` antes de `dotnet publish`

### Erro: "Database connection failed"
**Solução:** Verificar appsettings.Production.json está em `/var/www/404ride/publish/`

### Ver logs detalhados:
```bash
sudo journalctl -u 404ride.service -f
```

---

## Comandos Úteis

```bash
# Ver logs em tempo real
sudo journalctl -u 404ride.service -f

# Ver status
sudo systemctl status 404ride.service

# Reiniciar serviço
sudo systemctl restart 404ride.service

# Ver última versão do código
cd /var/www/404ride/app
git log -1

# Ver diferenças entre local e remoto
git fetch
git diff origin/main

# Reverter para commit anterior (se algo correr mal)
git reset --hard COMMIT_HASH
sudo /var/www/404ride/deploy.sh
```

---

## ✅ Checklist Final

- [ ] Git instalado no servidor
- [ ] Repositório clonado em `/var/www/404ride/app`
- [ ] appsettings.Production.json criado e configurado
- [ ] 404ride.service atualizado
- [ ] Deploy script criado e testado
- [ ] Primeiro deploy bem sucedido
- [ ] Site acessível em https://404ride.b-host.me

---

**Pronto! Agora tens um workflow profissional de deploy com Git! 🎉**
