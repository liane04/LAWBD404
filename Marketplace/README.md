# 🚗 DriveDeal - 404 Ride

> Marketplace de Veículos Usados | ASP.NET Core 8.0 MVC

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-8.0-512BD4?style=flat&logo=dotnet)](https://dotnet.microsoft.com/apps/aspnet)
[![Bootstrap](https://img.shields.io/badge/Bootstrap-5-7952B3?style=flat&logo=bootstrap)](https://getbootstrap.com/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-LocalDB-CC2927?style=flat&logo=microsoft-sql-server)](https://www.microsoft.com/sql-server)

---

## 📖 Sobre o Projeto

**DriveDeal** (404 Ride) é um marketplace completo de veículos usados, desenvolvido como projeto académico para a UC de **Laboratório de Aplicações Web e Bases de Dados**.

Inspirado em plataformas como StandVirtual e Auto.pt, permite a interação entre compradores, vendedores e administradores através de um sistema completo de gestão de anúncios, reservas, compras e denúncias.

### 🌐 Demo Online
**Site:** [https://404ride.b-host.me/](https://404ride.b-host.me/)

---

## 🎯 Funcionalidades Principais

### 👥 Para Visitantes (Não Autenticados)
- ✅ Visualizar e pesquisar veículos (paginação de 12 por página)
- ✅ Filtros avançados (marca, modelo, preço, ano, km, combustível, localização)
- ✅ Ordenação (relevância, preço, ano, quilometragem)
- ✅ Detalhes completos do veículo
- ✅ Comparar veículos (até 3)
- ✅ ChatBot informativo com 20+ respostas FAQ

### 🛒 Para Compradores
- ✅ Reservar veículos com pagamento (€50-200, via Stripe)
- ✅ Marcar visitas (data/hora)
- ✅ Realizar compras completas (checkout via Stripe)
- ✅ Guardar anúncios favoritos
- ✅ Definir marcas favoritas
- ✅ Filtros guardados com notificações automáticas
- ✅ Mensagens com vendedores
- ⏳ Denunciar anúncios/utilizadores (modelos criados)
- ✅ Editar perfil e foto
- ✅ Histórico de compras e reservas

### 🏪 Para Vendedores
- ✅ Criar/editar/remover anúncios
- ✅ Upload de imagens (até 20 por anúncio)
- ✅ Gerir estado dos anúncios (ativo, reservado, vendido, pausado)
- ✅ Ver veículos reservados e vendidos
- ✅ Responder a mensagens
- ✅ Gerir visitas agendadas
- ✅ Destacar anúncios (€1.99 por 7 dias, topo das listagens)
- ✅ Funcionalidade dual: vendedores podem comprar/reservar veículos
- ⏳ Estatísticas de desempenho

### 👨‍💼 Para Administradores (Backoffice)
- ✅ Dashboard com estatísticas
- ✅ Gerir utilizadores (visualizar, editar, bloquear/ativar)
- ✅ Aprovar/rejeitar vendedores
- ✅ Moderar anúncios
- ⏳ Gerir denúncias
- ✅ Histórico de ações (auditoria)
- ⏳ Relatórios e gráficos

---

## 🛠️ Stack Tecnológica

### Backend
- **Framework:** ASP.NET Core 8.0 MVC
- **Linguagem:** C# (.NET 8.0)
- **ORM:** Entity Framework Core 9.0.10 (Code-First)
- **Base de Dados:** SQL Server LocalDB
- **Autenticação:** ASP.NET Core Identity
- **Pagamentos:** Stripe API (checkout, reservas, destaque)
- **Email:** SMTP (Gmail)
- **Background Services:** Notificações de filtros guardados

### Frontend
- **View Engine:** Razor Views (.cshtml)
- **CSS Framework:** Bootstrap 5
- **JavaScript:** Vanilla JS
- **Design:** Responsive (mobile-first)

### Ferramentas
- **IDE:** Visual Studio 2022
- **Controlo de Versão:** Git
- **Modelagem:** brModelo, PlantUML
- **Hospedagem:** b-host.me (demo)

---

## 🎨 Design

**Paleta de Cores:**
- Azul moderno: `#2563eb`
- Cinza azulado escuro: `#1e293b`

**Características:**
- ✅ Design profissional e moderno
- ✅ Totalmente responsivo (mobile, tablet, desktop)
- ✅ ChatBot integrado
- ✅ Interfaces intuitivas (max 3 cliques)
- ✅ Mensagens de erro claras em PT-PT

---

## 🚀 Instalação Rápida

### Pré-requisitos
- Visual Studio 2022
- .NET SDK 8.0
- SQL Server LocalDB

### Passos
```bash
# 1. Clone o repositório
git clone [URL_DO_REPOSITORIO]
cd app/Marketplace

# 2. Restaure os pacotes
dotnet restore

# 3. Crie a base de dados
dotnet ef database update

# 4. Execute a aplicação
dotnet run
```

### Credenciais de Teste
| Perfil | Email | Password |
|--------|-------|----------|
| Admin | admin@email.com | `Admin123` |
| Vendedor | vendedor@email.com | `Vende123` |
| Comprador | comprador@email.com | `Compr123` |

📖 **Guia completo:** Ver [SETUP.md](SETUP.md) para instruções detalhadas

---

## 📊 Arquitetura

### Modelo de Dados
- **31 entidades** mapeadas
- **3 hierarquias TPH** (Table Per Hierarchy):
  - Utilizador → Administrador / Vendedor / Comprador
  - Denuncia → DenunciaAnuncio / DenunciaUser
  - HistoricoAcao → AcaoAnuncio / AcaoUser
- **29 DbSets** no contexto principal
- **30+ Foreign Keys**

### Padrões Utilizados
- ✅ MVC (Model-View-Controller)
- ✅ Code-First (EF Core)
- ✅ Repository Pattern (parcial)
- ✅ View Components
- ✅ Data Annotations
- ✅ Fluent API

---

## 📁 Estrutura do Projeto

```
Marketplace/
├── Controllers/         # 6 controladores (749 linhas)
├── Models/             # 36 classes de modelo
├── Views/              # ~22 ficheiros .cshtml
├── Components/         # View Components
├── Services/           # Email, Upload, etc
├── Data/               # Contextos EF Core
├── Migrations/         # 4 migrações
├── wwwroot/            # CSS, JS, Imagens
├── appsettings.json    # Configurações
└── Program.cs          # Entry point
```

---

## 📈 Progresso do Projeto

**Fase Atual:** Fase 3 (72% completo)

**Infraestrutura:** ✅ 100%
- Modelos e BD (32 entidades)
- Migrações (índices de performance aplicados)
- Autenticação (Identity + 2FA)
- Views e Design
- Integrações (Stripe, Gmail, OAuth Google)

**Funcionalidades Core:**
- ✅ CRUD Anúncios: 100% (com paginação)
- ✅ Gestão Utilizadores: 95%
- ✅ Reservas/Compras: 100% (Stripe integrado)
- ✅ Upload Imagens: 100% (até 20 por anúncio)
- ✅ Sistema de Favoritos: 100%
- ✅ Sistema de Mensagens: 100%
- ✅ Destaque de Anúncios: 100%
- ⏳ Dashboard Admin: 60%
- ⏳ Sistema de Denúncias: 40% (modelos criados)

**Prazo Final:** 5 de janeiro de 2026
**Apresentação:** 6-10 de janeiro de 2026

---

## 🧪 Testes

Para testar a aplicação:

1. **Homepage:** `https://localhost:porta/`
2. **Explorar Veículos:** `/Anuncios`
3. **Login:** `/Utilizadores/Login`
4. **Dashboard Admin:** `/Administrador`
5. **Criar Anúncio:** Login como vendedor → Criar Anúncio

---

## 📚 Documentação

- 📖 [SETUP.md](SETUP.md) - Guia completo de instalação
- 📖 [contexto.md](contexto.md) - Contexto detalhado do projeto
- 📖 [ESTRUTURA_PROJETO.md](ESTRUTURA_PROJETO.md) - Arquitetura e estrutura
- 📖 [MELHORIAS_UI.md](MELHORIAS_UI.md) - Documentação de UI/UX

**Relatórios:**
- [Relatório Fase 2](../../Fase2/Relatorio_fase2.pdf)
- [Protocolo LAWBD](../../ProtocoloLAWBD_2025_26.pdf)

---

## 👥 Equipa

**Grupo 404** - 3 alunos

| Nome | Email | Número |
|------|-------|--------|
| Bruno Alves | al80990@utad.eu | al80990 |
| Liane Duarte | al79012@utad.eu | al79012 |
| Pedro Braz | al81311@utad.eu | al81311 |

**Curso:** Licenciatura em Engenharia Informática - 3º Ano
**UC:** Laboratório de Aplicações Web e Bases de Dados
**Instituição:** UTAD (Universidade de Trás-os-Montes e Alto Douro)
**Ano Letivo:** 2025/2026

---

## 📝 Requisitos do Projeto

### Funcionais (38 RF)
- ✅ RF01-RF07: Parte Pública
- ✅ RF08-RF17: Compradores
- ✅ RF18-RF26: Vendedores
- ✅ RF27-RF35: Administradores
- ✅ RF36-RF38: Gestão de Utilizadores

### Não Funcionais (11 RNF)
- ✅ RNF01: Sistema escalável
- ✅ RNF02: Segurança (roles, auth)
- ✅ RNF03: Auditoria (histórico ações)
- ✅ RNF04: Navegação intuitiva (max 3 cliques)
- ✅ RNF05: Mensagens de erro claras
- ✅ RNF06: Boas práticas de código
- ✅ RNF07: Compatibilidade browsers
- ✅ RNF08: SQL compatível
- ✅ RNF09: Validação de imagens (10MB, 20 max)
- ✅ RNF10: Validações de domínio
- ✅ RNF11: Português (PT-PT), formato DD-MM-AAAA

---

## 🔒 Segurança

- ✅ Passwords hashadas (ASP.NET Core Identity)
- ✅ Policy de passwords segura (8+ chars, upper+lower+digits)
- ✅ Lockout protection (5 tentativas, 15 min)
- ✅ Cookie authentication (HttpOnly)
- ✅ Anti-forgery tokens
- ✅ HTTPS redirect
- ⚠️ SMTP credentials (mover para User Secrets)

---

## 🐛 Problemas Conhecidos

- ⚠️ Sistema de denúncias (requisito de exame - parcialmente implementado)
- ⚠️ Ficheiro "nul" não rastreado no repositório

## ✨ Melhorias Recentes (dez 2025)

- ✅ **Paginação implementada:** 12 anúncios por página com navegação dinâmica
- ✅ **Índices de performance:** Queries otimizadas com índices em BD
- ✅ **Otimização de imagens:** Apenas primeira imagem carregada em listagens
- ✅ **Dual-role:** Vendedores podem comprar/reservar veículos
- ✅ **Stripe integrado:** Pagamentos reais para reservas, compras e destaque
- ✅ **ChatBot melhorado:** 20+ respostas específicas para FAQ
- ✅ **Notificações automáticas:** Background service para filtros guardados
- ✅ **Comparação de veículos:** Até 3 veículos simultaneamente

---

## 🔄 Roadmap

**✅ Completado (nov-dez 2025):**
- ✅ Upload de imagens funcional (até 20 por anúncio)
- ✅ Paginação de listagens (12 por página)
- ✅ Sistema de Reservas completo (com Stripe)
- ✅ Sistema de Compras completo (com Stripe)
- ✅ Sistema de Visitas completo
- ✅ Sistema de Favoritos (anúncios + marcas)
- ✅ Sistema de Notificações (filtros guardados)
- ✅ Sistema de Mensagens (chat vendedor-comprador)
- ✅ Sistema de Destaque de Anúncios (€1.99/7 dias)
- ✅ Índices de performance em BD
- ✅ Otimizações de queries

**⏳ Em Andamento (31 dez - 5 jan):**
- 🔥 Sistema de Denúncias (requisito de exame)
- 🔥 Dashboard Admin com estatísticas completas
- 🔥 Relatório Fase 3
- 🔥 Testes finais e correções
- 🔥 Preparar apresentação
- ⏳ User Secrets para SMTP (opcional)

---

## 📄 Licença

Projeto académico desenvolvido para a UC de Laboratório de Aplicações Web e Bases de Dados (LAWBD) - UTAD.

**Uso restrito para fins educacionais.**

---

## 🙏 Agradecimentos

- Plataformas de inspiração: [StandVirtual](https://www.standvirtual.com/), [Auto.pt](https://www.auto.pt/)
- Ferramentas utilizadas: PlantUML, brModelo, OpenAI ChatGPT
- Professores da UC de LAWBD - UTAD

---

**⭐ Se achaste útil, dá uma estrela ao repositório!**

---

**Última atualização:** 2025-12-31

🚗 **Drive safe, deal smart - 404 Ride** 🚗
