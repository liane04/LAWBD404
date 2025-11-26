# 🚀 Guia de Configuração - DriveDeal (404 Ride)

> **Instruções completas para configurar e executar o projeto após clone**

---

## 📋 Pré-requisitos

Antes de começar, certifica-te que tens instalado:

### Obrigatório:
- ✅ **Visual Studio 2022** (ou superior) com workload "ASP.NET and web development"
  - Download: https://visualstudio.microsoft.com/downloads/
- ✅ **.NET SDK 8.0** ou superior
  - Download: https://dotnet.microsoft.com/download/dotnet/8.0
- ✅ **SQL Server LocalDB** (incluído no Visual Studio)
  - Ou SQL Server Express/Developer Edition

### Opcional (mas recomendado):
- 🔧 **Visual Studio Code** - para edição rápida
- 🔧 **SQL Server Management Studio (SSMS)** - para gestão da BD
- 🔧 **Git** - para controlo de versão

---

## 📥 1. Clonar o Repositório

```bash
# Clone o repositório
git clone [URL_DO_REPOSITORIO]

# Navega para a pasta do projeto
cd app/Marketplace
```

---

## 🔧 2. Restaurar Pacotes NuGet

### Opção A: Visual Studio
1. Abre o ficheiro `Marketplace.sln` no Visual Studio
2. Clica com botão direito na solução → **Restore NuGet Packages**
3. Aguarda a conclusão do download

### Opção B: Linha de Comandos
```bash
dotnet restore
```

---

## 🗄️ 3. Configurar a Base de Dados

### 3.1 Verificar Connection String

Abre o ficheiro `appsettings.json` e verifica a connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MarketplaceDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

**Notas:**
- Por defeito usa **SQL Server LocalDB** (vem com Visual Studio)
- Se usares SQL Server Express, altera para: `Server=localhost\\SQLEXPRESS;Database=MarketplaceDb;...`
- Se usares SQL Server completo, altera para: `Server=localhost;Database=MarketplaceDb;...`

### 3.2 Criar a Base de Dados (Migrações)

#### Opção A: Package Manager Console (Visual Studio)
1. Abre o Visual Studio
2. Menu: **Tools → NuGet Package Manager → Package Manager Console**
3. Executa:

```powershell
Update-Database
```

#### Opção B: CLI (.NET Core)
```bash
dotnet ef database update
```

**✅ Sucesso:** Verás uma mensagem confirmando a criação das tabelas.

---

## 📧 4. Configurar Email (SMTP)

⚠️ **IMPORTANTE:** O sistema de email é necessário para:
- Recuperação de passwords
- Notificações de aprovação de vendedores
- Confirmações de reservas

### Opção 1: Usar Gmail (Recomendado para testes)

1. **Criar App Password no Gmail:**
   - Vai a https://myaccount.google.com/security
   - Ativa **Verificação em 2 passos**
   - Vai a **App passwords**
   - Gera uma password para "Mail" / "Windows Computer"
   - Copia a password gerada (16 caracteres)

2. **Configurar no projeto:**

   Abre `appsettings.json` e atualiza:
   ```json
   {
     "Smtp": {
       "Host": "smtp.gmail.com",
       "Port": 587,
       "EnableSsl": true,
       "User": "teu_email@gmail.com",
       "Pass": "app_password_16_caracteres",
       "From": "404 RIDE <teu_email@gmail.com>"
     }
   }
   ```

### Opção 2: Desativar Email (Desenvolvimento)

Se não quiseres configurar email agora:

1. Comenta o código de envio de emails nos controladores
2. Ou cria um **fake email sender** para testes

---

## 🔐 5. User Secrets (Segurança - Opcional mas Recomendado)

Para **não commitar credenciais** ao Git:

```bash
# Inicializa User Secrets
dotnet user-secrets init

# Adiciona as credenciais SMTP
dotnet user-secrets set "Smtp:Host" "smtp.gmail.com"
dotnet user-secrets set "Smtp:Port" "587"
dotnet user-secrets set "Smtp:User" "teu_email@gmail.com"
dotnet user-secrets set "Smtp:Pass" "tua_app_password"
dotnet user-secrets set "Smtp:From" "404 RIDE <teu_email@gmail.com>"

# Remove do appsettings.json a secção "Smtp" após isto
```

---

## ▶️ 6. Executar a Aplicação

### Opção A: Visual Studio
1. Abre `Marketplace.sln`
2. Define `Marketplace` como **Startup Project** (botão direito na solução)
3. Pressiona **F5** ou clica em **IIS Express** / **Marketplace**
4. O browser abrirá automaticamente em `https://localhost:porta/`

### Opção B: CLI
```bash
dotnet run
```

Abre o browser em: `https://localhost:7xxx/` (verifica a porta no output)

---

## 👥 7. Credenciais de Acesso

O sistema cria automaticamente 3 utilizadores demo no primeiro arranque:

| Perfil | Email | Password |
|--------|-------|----------|
| **Administrador** | admin@email.com | `Admin123` |
| **Vendedor** | vendedor@email.com | `Vende123` |
| **Comprador** | comprador@email.com | `Compr123` |

**Policy de passwords:**
- Mínimo 8 caracteres
- Pelo menos 1 letra maiúscula
- Pelo menos 1 letra minúscula
- Pelo menos 1 dígito

---

## 🧪 8. Testar a Aplicação

### Testes básicos:

1. ✅ **Homepage** - `https://localhost:porta/`
   - Deve carregar sem erros
   - ChatBot no canto inferior direito

2. ✅ **Login** - `/Utilizadores/Login`
   - Tenta login com `admin@email.com` / `Admin123`
   - Deve redirecionar para dashboard de administrador

3. ✅ **Explorar Veículos** - `/Anuncios`
   - Deve mostrar lista de anúncios (pode estar vazia inicialmente)
   - Testa os filtros (marca, modelo, preço)

4. ✅ **Criar Anúncio** (como Vendedor)
   - Login como vendedor
   - Cria um anúncio de teste
   - Verifica se aparece na listagem

5. ✅ **Dashboard Admin** - `/Administrador`
   - Login como admin
   - Verifica estatísticas

---

## 🐛 Resolução de Problemas

### Problema: "Cannot connect to SQL Server"
**Solução:**
```bash
# Verifica se o SQL Server LocalDB está a correr
sqllocaldb info mssqllocaldb

# Se não estiver, inicia:
sqllocaldb start mssqllocaldb
```

### Problema: "A network-related or instance-specific error"
**Solução:**
- Verifica a connection string em `appsettings.json`
- Tenta usar `Server=(localdb)\\mssqllocaldb` ou `Server=localhost\\SQLEXPRESS`

### Problema: "The entity type 'X' requires a primary key"
**Solução:**
```bash
# Remove a base de dados e recria
dotnet ef database drop
dotnet ef database update
```

### Problema: "Unable to send email"
**Solução:**
- Verifica se a App Password do Gmail está correta
- Verifica se SSL está ativado (`EnableSsl: true`)
- Testa com um email real teu

### Problema: "Migration already applied"
**Solução:**
```bash
# Lista migrações aplicadas
dotnet ef migrations list

# Se necessário, remove a última migração
dotnet ef database update [MigracaoAnterior]
```

---

## 📂 Estrutura de Pastas Importante

```
Marketplace/
├── Controllers/         # Lógica de negócio
├── Models/             # Entidades da BD
├── Views/              # Interfaces Razor
├── wwwroot/            # CSS, JS, Imagens
│   ├── css/
│   ├── js/
│   └── imagens/        # Upload de imagens de anúncios
├── Data/               # Contextos EF Core
├── Migrations/         # Migrações da BD
├── Services/           # Serviços (Email, etc)
├── Components/         # View Components
├── appsettings.json    # Configurações
└── Program.cs          # Entry point
```

---

## 🔄 Comandos Úteis

```bash
# Compilar o projeto
dotnet build

# Limpar build
dotnet clean

# Executar testes (se existirem)
dotnet test

# Criar nova migração
dotnet ef migrations add NomeDaMigracao

# Reverter migração
dotnet ef migrations remove

# Ver SQL das migrações
dotnet ef migrations script

# Listar migrações
dotnet ef migrations list

# Limpar base de dados
dotnet ef database drop
```

---

## 🌐 Ambiente de Produção (b-host.me)

Se quiseres fazer deploy para o servidor de testes:

1. **Publicar o projeto:**
   ```bash
   dotnet publish -c Release -o ./publish
   ```

2. **Fazer upload via FTP** para b-host.me
   - Host: ftp.b-host.me
   - Credenciais: [solicitar ao administrador]

3. **Atualizar connection string** para o SQL Server remoto

---

## 📞 Suporte

### Problemas comuns:
- Consulta o ficheiro `contexto.md` para detalhes do projeto
- Consulta `ESTRUTURA_PROJETO.md` para arquitetura

### Contactos da Equipa:
- **Bruno Alves:** al80990@utad.eu
- **Liane Duarte:** al79012@utad.eu
- **Pedro Braz:** al81311@utad.eu

---

## ✅ Checklist de Configuração

Antes de começar a desenvolver, certifica-te:

- [ ] Visual Studio 2022 instalado
- [ ] .NET SDK 8.0 instalado
- [ ] SQL Server LocalDB a funcionar
- [ ] Pacotes NuGet restaurados
- [ ] Base de dados criada (`dotnet ef database update`)
- [ ] SMTP configurado (ou desativado para testes)
- [ ] Aplicação executa sem erros (F5)
- [ ] Login funciona (admin@email.com / Admin123)
- [ ] Homepage carrega corretamente
- [ ] Anúncios listam (mesmo que vazio)

---

## 🎓 Informações do Projeto

- **Nome:** DriveDeal / 404 Ride
- **UC:** Laboratório de Aplicações Web e Bases de Dados
- **Curso:** Licenciatura em Engenharia Informática - 3º Ano
- **Instituição:** UTAD
- **Ano Letivo:** 2025/2026

---

**Última atualização:** 2025-11-19

✅ **Projeto pronto a executar!** Se seguiste todos os passos, a aplicação deve estar a funcionar corretamente. Boa codificação! 🚀
