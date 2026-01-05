# CONTEXTO DO PROJETO - DriveDeal (404 Car Marketplace)

> **Ficheiro de contexto para sessões futuras com Claude Code**
> **Última atualização:** 2025-12-30 (Sistema de Destaque de Anúncios + Comparação + Imagens)
> **Fase atual:** Fase 3 (em desenvolvimento ativo - Sprint final)
> **Prazo de entrega:** 5 de janeiro de 2026 (6 dias restantes ⚠️)

---

## 1. INFORMAÇÕES GERAIS DO PROJETO

### 1.1 Identificação
- **Nome do Projeto:** DriveDeal / 404 Car Marketplace
- **Unidade Curricular:** Laboratório de Aplicações Web e Bases de Dados (LAWBD)
- **Curso:** Licenciatura em Engenharia Informática - 3º Ano
- **Instituição:** UTAD (Universidade de Trás-os-Montes e Alto Douro)
- **Ano Letivo:** 2025/2026

### 1.2 Equipa
- **Bruno Alves** (al80990)
- **Liane Duarte** (al79012)
- **Pedro Braz** (al81311)

### 1.3 Descrição
Marketplace de veículos usados inspirado em plataformas como StandVirtual e Auto.pt, permitindo a interação entre compradores, vendedores e administradores através de um sistema completo de gestão de anúncios, reservas, compras e denúncias.

### 1.4 URLs Importantes
- **Site Online:** https://404ride.b-host.me/
- **Repositório Local:** C:\Users\bruno\Desktop\utad\a_1_semestre_3_ano\Laboratotio_web_bd\app\Marketplace

### 1.5 Nomes defenidos para a aplicação e logo
- **Nomesite: 404 Ride
- **caminho logo azul:"C:\Users\bruno\Desktop\utad\a_1_semestre_3_ano\Laboratotio_web_bd\app\Marketplace\wwwroot\imagens\logo.png"
- **caminho logo branca:""C:\Users\bruno\Desktop\utad\a_1_semestre_3_ano\Laboratotio_web_bd\app\Marketplace\wwwroot\imagens\logo_branco.png"

---

## 2. STACK TECNOLÓGICA

### 2.1 Backend
- **Framework:** ASP.NET Core 8.0 MVC
- **ORM:** Entity Framework Core 9.0.10 (Code-First)
- **Base de Dados:** SQL Server (LocalDB para desenvolvimento)
- **Linguagem:** C# (.NET 8.0)
- **Autenticação:** ASP.NET Core Identity (✅ COMPLETO)
  - Login, registo, roles (Administrador/Vendedor/Comprador)
  - Recuperação de password funcional
  - Policy de passwords segura (8+ chars, upper, lower, digits)
  - Lockout protection (5 tentativas, 15 min)
  - Integração com domínio via `ApplicationUser` (chave int)
- **Email:** SMTP via Gmail (SmtpEmailSender) ⚠️ Ver secção de segurança
- **Serviços Implementados:**
  - `IEmailSender` / `SmtpEmailSender`
  - `ImageUploadHelper`
  - `PasswordHasher` (legacy, substituído por Identity)

### 2.2 Frontend
- **View Engine:** Razor Views (.cshtml)
- **CSS Framework:** Bootstrap 5
- **JavaScript:** Vanilla JS (com planos para chat em tempo real)
- **Cores do tema:**
  - Azul moderno: `#2563eb`
  - Cinza azulado escuro: `#1e293b`

### 2.3 Ferramentas de Desenvolvimento
- **IDE:** Visual Studio 2022 / VS Code
- **Controlo de Versão:** Git
- **Modelagem:** brModelo, PlantUML
- **Hospedagem:** b-host.me (gratuito)

---

## 3. ARQUITETURA E ESTRUTURA

### 3.3 Novidades Fase 3 (implementado nesta iteração)
- Filtros guardados (Pesquisa): comprador pode guardar a pesquisa atual a partir de `Anúncios/Index`.
- Alertas/Notificações automáticas: serviço em background verifica periodicamente novos anúncios que correspondam aos filtros e cria notificações.
- Endpoints de gestão: guardar, ativar/desativar e apagar filtros (mínimo viável; UI de gestão completa a melhorar).

Componentes alterados/criados:
- `Models/FiltrosFav.cs`: adicionados campos de critérios (marca, modelo, tipo, combustível, preço, ano, km, caixa, localização), controlo de alertas (`Ativo`, `LastCheckedAt`, `MaxAnuncioIdNotificado`) e `Nome`.
- `Services/SavedFiltersNotificationService.cs`: BackgroundService que gera `Notificacoes` para filtros ativos.
- `Program.cs`: registo do hosted service (`AddHostedService<SavedFiltersNotificationService>()`).
- `Controllers/AnunciosController.cs`: novos endpoints `GuardarFiltro`, `ToggleFiltro`, `DeleteFiltro` e exposição de filtros guardados via `ViewBag.SavedFilters`.
- `Views/Anuncios/Index.cshtml`: botão “Guardar Pesquisa” agora efetua POST para guardar o filtro atual.
 - `Views/Utilizadores/Perfil.cshtml`: nova aba "Pesquisas Guardadas" (apenas compradores) para ativar/desativar e apagar pesquisas.
### 3.1 Padrão Arquitetural
- **Padrão MVC** (Model-View-Controller)
- **Code-First** (EF Core)
- **Repository Pattern** (a implementar)

### 3.2 Estrutura de Pastas Principais

```
Marketplace/
├── Controllers/          # 6 controladores
│   ├── HomeController.cs              (39 linhas)
│   ├── AnunciosController.cs          (202 linhas) [EM DESENVOLVIMENTO]
│   ├── UtilizadoresController.cs      (347 linhas) [✅ FUNCIONAL - Auth/Perfil]
│   ├── AdministradorController.cs     (130 linhas) [⏳ PARCIAL - Validação vendedores OK]
│   ├── MensagensController.cs         (16 linhas) [ESQUELETO]
│   └── FaqController.cs               (15 linhas) [ESQUELETO]
│
├── Models/               # 36 classes de modelo
│   ├── ApplicationUser.cs       [✅ Identity user (int key)]
│   ├── Utilizador.cs           [classe abstrata - TPH]
│   ├── Administrador.cs, Vendedor.cs, Comprador.cs
│   ├── Anuncio.cs, Marca.cs, Modelo.cs, Categoria.cs
│   ├── Reserva.cs, Visita.cs, Compra.cs
│   ├── Conversa.cs, Mensagens.cs
│   ├── Denuncia.cs (TPH), DenunciaAnuncio.cs, DenunciaUser.cs [⏳ MODELOS OK]
│   ├── HistoricoAcao.cs (TPH), AcaoAnuncio.cs, AcaoUser.cs
│   ├── AnuncioFav.cs, MarcasFav.cs, FiltrosFav.cs
│   ├── EditProfileViewModel.cs, ProfileViewModel.cs [NOVOS]
│   └── ... (ver lista completa na secção 4)
│
├── Views/                # ~22 ficheiros .cshtml
│   ├── Anuncios/         # Index, Details, Create, Edit, Delete, Compare
│   ├── Utilizadores/     # Index, Details, Edit, Delete, Perfil [✅ Login/Registo OK]
│   ├── Home/             # Index, Privacy
│   ├── Mensagens/        # Index (interface chat)
│   ├── Administrador/    # Index (dashboard), ValidarVendedores [✅ FUNCIONAL]
│   ├── Faq/              # Index
│   └── Shared/           # _Layout, _ChatWidget, Error, StatusCode
│       └── Components/   # ValidarVendedores (view component) [✅ NOVO]
│
├── Components/           # View Components [✅ NOVA PASTA]
│   └── ValidarVendedoresViewComponent.cs
│
├── Services/             # Serviços [✅ NOVA PASTA]
│   ├── IEmailSender.cs
│   ├── SmtpEmailSender.cs
│   ├── ImageUploadHelper.cs
│   └── PasswordHasher.cs (legacy)
│
├── Data/
│   ├── ApplicationDbContext.cs  # Contexto principal (29 DbSets) + Identity integrado
│   ├── MarketplaceContext.cs    # Contexto auxiliar/legado
│   └── Seeders/
│       └── ReferenceDataSeeder.cs [TEMPORARIAMENTE DESATIVADO]
│
├── Migrations/          # 4 migrações
│   ├── 20251023165525_InitialCreate.cs
│   ├── 20251105114921_AddImagemPerfilColumn.cs
│   ├── 20251105123136_RefDataSeed.cs
│   └── 20251112121445_AddIdentityIntegration.cs [✅ MAIS RECENTE]
│
├── wwwroot/              # Recursos estáticos
│   ├── css/
│   ├── js/
│   ├── lib/
│   └── images/
│
├── appsettings.json      # ⚠️ CONTÉM CREDENCIAIS SMTP (PROBLEMA SEGURANÇA!)
├── Program.cs            # Entry point, Identity + Seeding configurado
└── Marketplace.csproj    # Ficheiro de projeto
```

### 3.3 Configuração de DbContexts

#### ApplicationDbContext (PRINCIPAL)
- 29 DbSets mapeados
- 3 hierarquias TPH (Table Per Hierarchy):
  1. **Utilizador** → Administrador, Vendedor, Comprador
  2. **Denuncia** → DenunciaAnuncio, DenunciaUser
  3. **HistoricoAcao** → AcaoAnuncio, AcaoUser
- `DeleteBehavior.Restrict` na maioria das relações
- `Precision(10,2)` para campos decimais de valores monetários
 - Integração com ASP.NET Core Identity (`IdentityDbContext<ApplicationUser, IdentityRole<int>, int>`)
 - Índice único em `Utilizador.IdentityUserId` (ligação 1:1 com `ApplicationUser`)

#### MarketplaceContext (AUXILIAR/LEGADO)
- 4 DbSets básicos
- Contexto secundário (mockups/legado)

---

## 4. MODELO DE DADOS

### 4.1 Entidades Principais (31 entidades)

#### Hierarquia de Utilizadores (TPH)
```
Utilizador (abstrata)
├── id_utilizador (PK)
├── username, email, password, nome, tipo
├── estado, foto_perfil
│
├── Administrador
│   ├── NivelAcesso
│   └── Nav: HistoricoAcoes, DenunciasGeridas
│
├── Vendedor
│   ├── dados_faturacao, nif, contactos (1,n)
│   ├── tipo (particular/empresa)
│   └── Nav: Anuncios, Conversas, DenunciasRecebidas
│
└── Comprador
    ├── preferencias, contactos (1,n), filtros_fav (1,n)
    └── Nav: Reservas, Compras, Visitas, AnunciosFav, MarcasFav
```

#### Anúncios e Veículos
- **Anuncio**: id_anuncio, cor, ano, preco, quilometragem, descricao, titulo, caixa, localizacao, n_visualizacoes, valor_sinal, portas, lugares, potencia, cilindrada, data_expiracao
  - Relacionamentos: Marca (1,1), Modelo (1,1), Categoria (1,1), Combustivel (1,1), Tipo (1,n)
  - Imagens (1,n), AnuncioExtras (1,n), Reservas, Compras, Visitas, Conversas

- **Marca**: id, nome
- **Modelo**: id_modelo, nome, fk_marca
- **Categoria**: id_categoria, nome
- **Combustivel**: id_combustivel, tipo
- **Tipo**: id_tipo, nome
- **Imagem**: id, caminho_imagem, fk_anuncio
- **AnuncioExtra**: id, fk_anuncio, fk_extra (relação N:N entre Anuncio e Extra)
- **Extra**: id, descricao, tipo

#### Transações
- **Reserva**: id_reserva, estado, data, data_expiracao, fk_comprador, fk_anuncio
- **Visita**: id_visita, data, estado, fk_comprador, fk_anuncio
- **Compra**: id_compra, data, estado_pagamento, fk_comprador, fk_anuncio

#### Comunicação
- **Conversa**: id_conversa, tipo (A comprar/A anunciar), fk_comprador, fk_anuncio, fk_vendedor
- **Mensagens**: id_mensagem, conteudo, estado, data_envio, fk_conversa

#### Sistema de Favoritos
- **AnuncioFav**: fk_comprador, fk_anuncio, campo (?)
- **MarcasFav**: fk_comprador, fk_marca
- **FiltrosFav**: id, fk_comprador

#### Sistema de Denúncias (TPH)
```
Denuncia (abstrata)
├── id
├── data, conteudo, estado, motivo
├── fk_denunciante (Utilizador)
│
├── DenunciaAnuncio
│   ├── fk_anuncio
│   └── fk_anuncio_denunciado
│
└── DenunciaUser
    └── fk_utilizador_denunciado
```

#### Sistema de Auditoria (TPH)
```
HistoricoAcao (abstrata)
├── id
├── data, motivo
├── fk_administrador
│
├── AcaoAnuncio
│   └── fk_anuncio
│
└── AcaoUser
    └── fk_utilizador
```

#### Outros
- **Notificacoes**: id_notificacao, data, conteudo, fk_pesquisas_passadas, fk_filtrosFav, fk_anuncioFav, fk_marcaFav
- **PesquisasPassadas**: id, data, count
- **Contactos**: fk_vendedor, contactos_nif, nome
- **ContactosComprador**: fk_comprador, nome
- **Morada**: codigo_postal, descricao, localidade, nif

### 4.2 Relacionamentos Principais
- **1:N** - Vendedor → Anuncios, Comprador → Reservas, Anuncio → Imagens
- **N:N** - Anuncio ↔ Extra (via AnuncioExtra)
- **1:1** - Anuncio → Marca, Modelo, Categoria, Combustivel

---

## 5. REQUISITOS FUNCIONAIS

### 5.1 Parte Pública (Utilizadores não autenticados)

**RF01-RF07:**
- Visualizar página institucional (contactos, termos, políticas)
- Visualizar listagens de veículos (título, marca, modelo, categoria, ano, preço, km, combustível, caixa, localização, descrição, tipo, imagens)
- Pesquisar com filtros (tipo, categoria, marca/modelo, ano, preço, km, combustível, caixa, localização)
- Ordenar listagens (mais recentes, preço, km)
- Visualizar detalhes avançados do veículo
- Comparar veículos (selecionar até 2 veículos)
- FAQ e Chatbot informativo

### 5.2 Parte Privada - Compradores

**RF06-RF17:**
- Pesquisar e guardar filtros favoritos
- Definir marcas favoritas e receber notificações
- Reservar veículo (prazo de expiração configurável)
- Marcar visitas (data/hora) e consultar histórico
- Realizar compra (simulação checkout) com registo de encomenda e estado de pagamento
- Denunciar anúncios enganadores ou utilizadores
- Guardar anúncios favoritos
- Avaliar vendedores
- Mensagens diretas com vendedores
- Sugestões personalizadas
- Receber alertas com base em pesquisas anteriores
- Histórico de interações do comprador

### 5.3 Parte Privada - Vendedores

**RF18-RF26:**
- Criar anúncios (imagens + especificações: marca, modelo, ano, preço, etc.)
- Editar, pausar e remover anúncios
- Atualizar estado dos anúncios (ativo, reservado, vendido, pausado)
- Consultar listagens de veículos reservados e vendidos
- Responder a mensagens e gerir visitas e denúncias recebidas
- Estatísticas de desempenho dos anúncios
- Gestão de perfil do vendedor
- Exportar listagem de anúncios
- Destacar anúncios (pagar ou ativar opção para colocar no topo das listagens)

### 5.4 Backoffice - Administradores

**RF27-RF35:**
- Criar utilizadores com permissões de administrador
- Gerir perfis de utilizadores (atualização, ativação/bloqueio de contas, registo do motivo de bloqueio)
- Moderar anúncios (pausar/remover anúncios em incumprimento)
- Consultar estatísticas (nº compradores/vendedores, anúncios ativos, vendas por período, top marcas/modelos)
- Gerir denúncias (listar por estado, analisar evidências, registar ações, encerrar como procedente/não procedente)
- Manter histórico de ações administrativas (auditoria)
- Bloquear compradores ou vendedores (indicar motivo)
- Dashboard inicial com indicadores
- Gestão de permissões

### 5.5 Gestão de Utilizadores

**RF36-RF38:**
- Registar compradores (validação via email)
- Registar vendedores (validação por administrador: email/telefone, NIF, duplicados, reputação)
- Permitir contas distintas de comprador e vendedor para o mesmo utilizador

---

## 6. REQUISITOS NÃO FUNCIONAIS

**RNF01-RNF11:**
- Sistema escalável (integração futura: alugar, financiamento)
- Apenas administradores autenticados acedem ao backoffice
- Todas as ações administrativas registadas para auditoria
- Navegação intuitiva (máximo 3 cliques até detalhes do anúncio)
- Mensagens de erro claras com orientações de resolução
- Código com boas práticas (documentação, comentários)
- Compatível com browsers modernos (Chrome, Firefox, Edge, Safari)
- Compatível com bases de dados SQL
- Imagens: formatos JPEG/PNG/WebP, até 10 MB por imagem, 20 imagens por anúncio
- Validações de domínio (ano entre 1960 e ano corrente, km ≥ 0)
- Mensagens em português (PT-PT), formato DD-MM-AAAA para datas

---

## 7. FASES DO PROJETO

### 7.1 Fase 1 (Concluída - 6 a 10 outubro 2025)
- ✅ Relatório detalhado
- ✅ Análise dos requisitos de dados
- ✅ Modelo conceptual (diagramas E-R)
- ✅ Análise dos requisitos funcionais
- ✅ Modelo funcional (diagramas Casos de Uso)

**Entregues:**
- Diagramas E-R (4 diagramas separados)
- Diagramas de Casos de Uso (7 diagramas)
- Requisitos Funcionais (RF01-RF38)
- Requisitos Não Funcionais (RNF01-RNF11)

### 7.2 Fase 2 (Concluída - 27 a 31 outubro 2025)
- ✅ Relatório detalhado
- ✅ Mapeamento modelo conceptual → modelo relacional
- ✅ Modelo físico da BD (SQL via EF Core Code-First)
- ✅ Mockups das interfaces (backoffice e frontoffice)

**Entregues:**
- Diagramas Relacionais (4 diagramas)
- Base de dados criada via EF Core migrations
- Mockups implementados em Views Razor
- Site hospedado: https://404ride.b-host.me/

**Alterações ao Modelo E-R:**
- Adição do atributo `NivelAcesso` em Administrador
- Adição de atributos em Anuncio: `Localizacao`, `Valor_sinal`, `n_visualizacoes`, `Portas`, `Lugares`, `Potencia`, `Cilindrada`
- Criação das entidades `AnuncioExtras` e `Extras`

### 7.3 Fase 3 (Em Desenvolvimento - até 6 a 10 janeiro 2026)
**A desenvolver (PRIORIDADES PARA AVALIAÇÃO CONTÍNUA):**
- ⏳ Relatório detalhado
- ⏳ Implementação da integridade da BD
- ⏳ Lógica funcional (ligar interfaces à BD)
- ✅ Sistema de autenticação e autorização (COMPLETO)
- ⏳ Upload de imagens
- ⏳ Sistema de reservas e visitas funcional
- ⏳ Dashboard administrativo com estatísticas
- 🔜 Sistema de favoritos
- 🔜 Sistema de notificações básico

**⚠️ IMPORTANTE - Sistema de Denúncias:**
- ⚠️ **Requisito de EXAME** (não de avaliação contínua)
- ⚠️ **NÃO é prioridade** para a entrega da Fase 3 (5 jan 2026)
- ✅ Modelos já criados (Denuncia TPH, DenunciaAnuncio, DenunciaUser)
- 🔜 Implementação funcional pode ser desenvolvida posteriormente para época de exames

**Funcionalidades do Sistema de Denúncias (para implementação futura):**
- Qualquer comprador ou vendedor pode denunciar outro utilizador ou anúncio enganoso
- Workflow de estados: Aberta → Em análise → Encerrada (procedente/não procedente)
- Registo de histórico de ações do administrador (quem analisou, decisões, notas)
- Notificações aos intervenientes sobre alterações de estado
- Listagens filtradas por estado e detalhe de cada denúncia no backoffice

---

## 8. ESTADO ATUAL DO DESENVOLVIMENTO

### 8.1 Completo ✅
**Infraestrutura:**
- ✅ Estrutura de Models (36 classes; 3 hierarquias TPH)
- ✅ Configuração EF Core (29 DbSets no principal; 4 no auxiliar)
- ✅ 4 Migrações funcionais (última: AddIdentityIntegration - 12/11/2025)
- ✅ Views criadas (~22 ficheiros .cshtml)
- ✅ Layouts e design profissional (Bootstrap 5)
- ✅ ChatBot widget (assistente 404)

**Autenticação e Autorização:**
- ✅ ASP.NET Core Identity totalmente integrado
  - Login/Registo funcionais com validações
  - 3 Roles (Administrador, Vendedor, Comprador)
  - Password Reset funcional via email
  - Policy de passwords segura (8+ chars, upper+lower+digits)
  - Lockout protection (5 tentativas, 15 min)
  - Cookie authentication configurado
- ✅ ApplicationUser com chave int (ligação 1:1 com domínio via IdentityUserId)
- ✅ Seeding automático de roles e utilizadores demo no arranque

**Serviços:**
- ✅ Email Service (SMTP via Gmail) - `SmtpEmailSender`
- ✅ Email Templates para notificações
- ✅ Image Upload Helper (preparado, mas não testado)
- ✅ View Components - `ValidarVendedoresViewComponent`, `GerirUtilizadoresViewComponent`, `ModerarAnunciosViewComponent`

**Backoffice (Administrador):**
- ✅ AdministradorController (130 linhas) com funcionalidades completas:
  - Aprovar/Rejeitar vendedores pendentes
  - Gerir utilizadores (visualizar, editar, bloquear/ativar)
  - Moderar anúncios
  - Notificações por email automáticas
  - View + ViewComponents funcionais
- ✅ Dashboard administrativo (Index) com estatísticas básicas

**Utilizadores:**
- ✅ UtilizadoresController (347 linhas) - FUNCIONAL
  - Login/Logout funcionais
  - Registo de compradores e vendedores
  - Perfil com visualização e edição
  - Recuperação de password via email
  - Upload de imagem de perfil
- ✅ ViewModels (EditProfileViewModel, ProfileViewModel)

### 8.2 Em Desenvolvimento ⏳
**Anúncios:**
- ✅ AnunciosController (202 linhas) - CRUD FUNCIONAL
  - ✅ Create - Criação de anúncios funcional
  - ✅ Edit - Edição de anúncios funcional
  - ✅ Delete - Remoção de anúncios funcional
  - ✅ Index - Listagem dinâmica com filtros funcionais
  - ✅ Details - Página de detalhes ligada à BD (incrementa visualizações)
  - ✅ Sistema de filtros dinâmico (marca, modelo, preço, ano, km, combustível)
  - ⏳ Upload de múltiplas imagens (em teste)
  - ⏳ Galeria de imagens completa

**Sistema de Denúncias (⚠️ Requisito de EXAME - NÃO prioritário para Fase 3):**
- ✅ Modelos criados (Denuncia TPH, DenunciaAnuncio, DenunciaUser)
- 🔜 Controllers não implementados (para época de exames)
- 🔜 Views não criadas (para época de exames)
- 🔜 Workflow de estados (Aberta → Em análise → Encerrada) por implementar
- 🔜 Notificações aos intervenientes por implementar

**Formulários e Interações:**
- ⏳ Formulários de reserva e visita
- ⏳ Sistema de favoritos (modelos OK, lógica por implementar)
- ⏳ Views dinâmicas (substituir mockups remanescentes)

### 8.3 A Implementar 🔜

**PRIORIDADES CRÍTICAS (até 5 jan 2026 - FASE 3):**
1. ✅ **Ligar AnunciosController à BD** ~~(eliminar mockups)~~ - COMPLETO
   - ✅ Index dinâmico com dados reais
   - ✅ Details com informações da BD
   - ✅ Filtros e pesquisa funcionais
   - ⏳ Paginação por implementar

2. 🔥 **Upload de imagens** funcional (máx 10 MB, 20 por anúncio) - ALTA PRIORIDADE
   - ⏳ Testar ImageUploadHelper (50% completo)
   - ⏳ Validações de formato (JPEG/PNG/WebP)
   - ⏳ Integração com Create/Edit de anúncios
   - ⏳ Galeria de imagens múltiplas funcional

3. 🔥 **Sistema de Reservas e Visitas** completo - ALTA PRIORIDADE
   - ⏳ Formulários funcionais (parcialmente implementado)
   - ⏳ Validação de datas e conflitos
   - ⏳ Expiração de reservas automática
   - ✅ Notificações por email (templates prontos)
   - ⏳ Histórico de reservas/visitas

4. ⏳ **Dashboard Administrativo** com estatísticas - MÉDIA PRIORIDADE
   - ⏳ Nº compradores/vendedores (parcial)
   - ⏳ Anúncios ativos/reservados/vendidos
   - ⏳ Vendas por período
   - ⏳ Top marcas/modelos
   - 🔜 Gráficos básicos

**IMPORTANTES (se der tempo):**
5. ⏳ Sistema de favoritos funcional
   - Guardar anúncios favoritos
   - Guardar marcas favoritas
   - Guardar filtros de pesquisa
6. ⏳ Sistema de notificações básico
7. ⏳ Sistema de mensagens entre utilizadores
8. ⏳ Exportação de listagens (CSV/Excel)
9. ⏳ Sistema de compras (simulação de checkout)

**PARA ÉPOCA DE EXAMES (NÃO prioritário agora):**
10. 🔜 **Sistema de denúncias completo**
    - Controllers (DenunciasController)
    - Views (listar, criar, detalhes, gerir)
    - Workflow de estados
    - Integração com histórico de ações
    - Notificações

**NICE-TO-HAVE (opcional):**
11. 💡 Sistema de chat em tempo real (SignalR)
12. 💡 Avaliações de vendedores
13. 💡 Sugestões personalizadas
14. 💡 Sistema de destacar anúncios (pagamento)

**ESQUELETOS:**
- MensagensController (16 linhas)
- FaqController (15 linhas)

---

## 9. FLUXOS DE NEGÓCIO PRINCIPAIS

### 9.1 Fluxo de Venda Completo
1. **Vendedor** cria `Anuncio` com `Imagens` e especificações
2. **Comprador** visualiza `Anuncio` (incrementa `n_visualizacoes`)
3. **Comprador** pode:
   - Adicionar a favoritos (`AnuncioFav`)
   - Fazer `Reserva` com `valor_sinal`
   - Marcar `Visita` (data/hora)
   - Enviar `Mensagem` ao vendedor via `Conversa`
4. **Vendedor** responde mensagens e confirma visita
5. **Comprador** realiza `Compra` (simulação checkout)
6. **Anuncio** muda estado para "Vendido"

### 9.2 Sistema de Favoritos
1. **Comprador** salva:
   - `AnuncioFav` (anúncios específicos)
   - `MarcasFav` (marcas preferidas)
   - `FiltrosFav` (filtros de pesquisa)
2. Sistema mantém `PesquisasPassadas`
3. Quando surge novo anúncio relevante:
   - Sistema cria `Notificacao`
   - Comprador é alertado

### 9.3 Sistema de Moderação e Denúncias
1. **Comprador ou Vendedor** cria `Denuncia`:
   - `DenunciaAnuncio` (anúncio enganoso)
   - `DenunciaUser` (utilizador suspeito)
2. **Administrador** analisa denúncia no backoffice
3. **Administrador** pode:
   - Pausar/remover anúncio (`AcaoAnuncio`)
   - Bloquear utilizador (`AcaoUser`)
   - Registar decisão em `HistoricoAcao`
4. Denúncia é encerrada como procedente ou não procedente
5. Intervenientes recebem `Notificacao` sobre decisão

---

## 10. CONSIDERAÇÕES TÉCNICAS

### 10.1 Segurança
- Senhas devem ser hashadas (usar ASP.NET Core Identity)
- Validação de inputs (proteção contra XSS, SQL Injection)
- Autorização baseada em roles (Comprador, Vendedor, Administrador)
- HTTPS obrigatório em produção

### 10.2 Performance
- Paginação de listagens (evitar carregar todos os anúncios de uma vez)
- Lazy loading de imagens
- Índices na BD (em campos de pesquisa frequente)
- Caching de dados estáticos

### 10.3 Boas Práticas
- Repository Pattern para acesso a dados
- DTOs para transferência de dados
- Separação de concerns (MVC)
- Validações no client-side e server-side
- Logging de erros (usar ILogger)
- Comentários em código complexo

---

## 11. MÉTRICAS DO PROJETO

| Métrica | Valor Atual |
|---------|-------------|
| **Controllers** | 6 (749 linhas totais) |
| **Models** | 36 classes (34 domínio + 2 ViewModels) |
| **DbSets mapeados** | 29 (principal) + 4 (auxiliar) |
| **Views** | ~22 ficheiros .cshtml |
| **View Components** | 1 (ValidarVendedoresViewComponent) |
| **Services** | 4 (IEmailSender, SmtpEmailSender, ImageUploadHelper, PasswordHasher) |
| **Migrações** | 4 (última: AddIdentityIntegration - 12/11/2025) |
| **Hierarquias TPH** | 3 (Utilizador, Denuncia, HistoricoAcao) |
| **Foreign Keys** | 30+ |
| **Relacionamentos N:N** | 1 (AnuncioExtra) |
| **Requisitos Funcionais** | 38 (30% implementados) |
| **Requisitos Não Funcionais** | 11 (80% implementados) |
| **Utilizadores Demo** | 3 (admin@email.com, vendedor@email.com, comprador@email.com) |
| **Roles** | 3 (Administrador, Vendedor, Comprador) |

### Progresso Geral: ~60% Completo (Fase 3)

**Infraestrutura e Base:**
- ✅ Modelos e BD: 100% (31 entidades, 3 TPH, 29 DbSets)
- ✅ Migrações: 100% (4 migrações funcionais)
- ✅ Autenticação (Identity): 100% (Login, Registo, Roles, Password Reset)
- ✅ Views e Design: 95% (22 views, responsive, chatbot)

**Funcionalidades Core (Prioridades Fase 3):**
- ✅ AnunciosController: 85% (CRUD completo, filtros dinâmicos; falta upload imagens)
- ✅ Gestão Utilizadores: 90% (criar, editar, bloquear/ativar, perfil)
- ⏳ Sistema Reservas: 30% (modelos OK, lógica parcialmente implementada)
- ⏳ Sistema Visitas: 30% (modelos OK, lógica parcialmente implementada)
- ⏳ Upload Imagens: 50% (helper criado, em fase de testes)
- ⏳ Dashboard Admin: 50% (view e estatísticas básicas criadas)

**Funcionalidades Secundárias:**
- 🔜 Sistema Favoritos: 10% (modelos OK)
- 🔜 Sistema Notificações: 10% (modelos OK)
- 🔜 Sistema Mensagens: 10% (modelos OK, esqueleto controller)

**Requisito de Exame (NÃO prioritário agora):**
- 🔜 Sistema Denúncias: 20% (modelos OK, zero lógica)

---

## 12. RECURSOS E REFERÊNCIAS

### 12.1 Documentação
- **Relatório Fase 2:** `C:\Users\bruno\Desktop\utad\a_1_semestre_3_ano\Laboratotio_web_bd\Fase2\Relatorio_fase2.pdf`
- **Protocolo LAWBD:** `C:\Users\bruno\Desktop\utad\a_1_semestre_3_ano\Laboratotio_web_bd\ProtocoloLAWBD_2025_26.pdf`
- **Estrutura do Projeto:** Ver ficheiro `ESTRUTURA_PROJETO.md` no diretório do projeto

### 12.2 Bibliografia Utilizada
- PlantUML Documentation (https://plantuml.com)
- OpenAI ChatGPT (https://chat.openai.com)
- brModelo (https://www.brmodeloweb.com)
- Microsoft ASP.NET Core Docs
- Entity Framework Core Docs

### 12.3 Inspiração
- StandVirtual (https://www.standvirtual.com/)
- Auto.pt (https://www.auto.pt/)

---

## 13. ROADMAP FASE 3 (até 5 jan 2026)

⚠️ **PRAZO FINAL:** 5 de janeiro de 2026 (entrega) + Apresentação 6-10 janeiro
⏰ **TEMPO RESTANTE:** 47 dias (a partir de 19/11/2025)

### ✅ PROGRESSO RECENTE (19/11/2025)

**Commits recentes implementados:**
- ✅ explorar veículos e filtros dinâmico
- ✅ gerir utilizadores
- ✅ editar perfil
- ✅ criar anúncio
- ✅ adicionar anúncio
- ✅ recuperação pass e email funcional
- ✅ migração para Identity
- ✅ utilizadores pré-definidos e roles

### 📍 Semana 1-2 (19 nov - 2 dez) - CONSOLIDAÇÃO & TESTES
**Prioridade: ALTA ⚠️**

1. ✅ ~~**Resolver credenciais SMTP expostas**~~ (User Secrets) - CONSIDERAR
   - ⚠️ Mover credenciais de appsettings.json para User Secrets
   - Testar funcionalidade de email após migração

2. ✅ ~~**Ligar AnunciosController à BD**~~ - COMPLETO
   - ✅ Index dinâmico implementado
   - ✅ Filtros e pesquisa funcionais
   - ✅ Ordenação implementada
   - ⏳ Paginação por implementar

3. 🔥 **Upload de imagens funcional** - EM PROGRESSO
   - ⏳ Testar ImageUploadHelper (50%)
   - ⏳ Validações (JPEG/PNG/WebP, máx 10MB, 20 por anúncio)
   - ⏳ Integração completa com Create/Edit
   - ⏳ Preview de imagens
   - ⏳ Galeria funcional

### 📍 Semana 3-4 (3-16 dez) - FUNCIONALIDADES PRINCIPAIS
**Prioridade: CRÍTICA 🔥**

4. **Sistema de Reservas completo** - PRIORIDADE MÁXIMA
   - ⏳ Formulário de reserva funcional
   - ⏳ Validação de datas e valor de sinal
   - ⏳ Expiração automática de reservas
   - ✅ Templates de email prontos
   - ⏳ Atualização de estado do anúncio

5. **Sistema de Visitas completo** - PRIORIDADE MÁXIMA
   - ⏳ Formulário de agendamento
   - ⏳ Validação de conflitos de horários
   - ✅ Templates de email prontos
   - ⏳ Histórico de visitas (comprador e vendedor)

6. **Dashboard Administrativo**
   - ⏳ Estatísticas principais (nº utilizadores, anúncios, vendas)
   - 🔜 Gráficos básicos (vendas por período, top marcas)
   - ⏳ Listagem de pendências (vendedores a validar)
   - ⏳ Ações rápidas

### 📍 Semana 5-6 (17-30 dez) - FUNCIONALIDADES SECUNDÁRIAS
**Prioridade: MÉDIA ⏳**

7. **Sistema de Favoritos** (se der tempo)
   - 🔜 Guardar anúncios favoritos
   - 🔜 Guardar marcas favoritas
   - 🔜 Interface de gestão de favoritos

8. **Sistema de Notificações** básico (se der tempo)
   - 🔜 Notificações in-app
   - 🔜 Badge de contador

9. **Sistema de Mensagens** (se der tempo)
   - 🔜 Chat entre comprador e vendedor
   - 🔜 Histórico de conversas

### 📍 Semana 7 (31 dez - 5 jan) - POLIMENTO & ENTREGA
**Prioridade: CRÍTICA 🔥**

10. **RELATÓRIO FASE 3** - OBRIGATÓRIO
    - Elaborar durante desenvolvimento
    - Documentar decisões técnicas
    - Screenshots e diagramas
    - Conclusões e trabalho futuro
    - Atualizar secção 3 - FASE 3

11. **Testes finais e correções de bugs**
    - Testar todos os fluxos principais
    - Corrigir bugs críticos
    - Validar responsividade
    - Testar em diferentes browsers
    - Remover ficheiro "nul" não rastreado

12. **Preparar apresentação**
    - Criar slides
    - Demonstração funcional
    - Definir divisão de tarefas na apresentação
    - Ensaiar apresentação (6-10 janeiro)

### ⏸️ NÃO PRIORITÁRIO (para época de exames)
- ❌ Sistema de denúncias (requisito de exame, não de avaliação contínua)
- ❌ Chat em tempo real (SignalR)
- ❌ Sistema de compras (simulação checkout)
- ❌ Exportação de listagens
- ❌ Avaliações de vendedores
- ❌ Sugestões personalizadas

---

## 14. UTILIZADORES DEMO & CREDENCIAIS

### 14.1 Contas de Teste (Seeding Automático)

Estas contas são criadas automaticamente ao iniciar a aplicação (Program.cs, linhas 172-218):

| Role | Email | Username | Password | Notas |
|------|-------|----------|----------|-------|
| **Administrador** | admin@email.com | admin | `Admin123` | Acesso total ao backoffice |
| **Vendedor** | vendedor@email.com | vendedor | `Vende123` | Criar/gerir anúncios |
| **Comprador** | comprador@email.com | comprador | `Compr123` | Reservas, favoritos, denúncias |

**Policy de Passwords:**
- Mínimo 8 caracteres
- Pelo menos 1 maiúscula
- Pelo menos 1 minúscula
- Pelo menos 1 dígito
- 3 caracteres únicos mínimo

**Lockout:**
- 5 tentativas falhadas = bloqueio de 15 minutos

### 14.2 Ligação Identity ↔ Domínio

Cada `ApplicationUser` (Identity) está ligado a uma entidade de domínio via `IdentityUserId`:
- `ApplicationUser.Id` (int) ↔ `Utilizador.IdentityUserId` (int)
- Relação 1:1 obrigatória
- Índice único em `IdentityUserId`

**Exemplo de criação:**
```csharp
// 1. Criar ApplicationUser via UserManager
var user = new ApplicationUser { UserName = "...", Email = "..." };
await userManager.CreateAsync(user, "password");
await userManager.AddToRoleAsync(user, "Vendedor");

// 2. Criar entidade de domínio ligada
var vendedor = new Vendedor
{
    Username = user.UserName,
    Email = user.Email,
    IdentityUserId = user.Id  // Link!
};
db.Vendedores.Add(vendedor);
await db.SaveChangesAsync();
```

---

## 15. COMANDOS ÚTEIS

### 15.1 Entity Framework Core
```bash
# Criar nova migração
dotnet ef migrations add NomeDaMigracao

# Atualizar base de dados
dotnet ef database update

# Reverter última migração
dotnet ef migrations remove

# Ver SQL gerado
dotnet ef migrations script
```

### 15.2 User Secrets (Credenciais Seguras)
```bash
# Inicializar User Secrets
dotnet user-secrets init

# Adicionar credenciais SMTP
dotnet user-secrets set "Smtp:Host" "smtp.gmail.com"
dotnet user-secrets set "Smtp:Port" "587"
dotnet user-secrets set "Smtp:Pass" "sua_password_aqui"

# Listar secrets
dotnet user-secrets list

# Remover um secret
dotnet user-secrets remove "Smtp:Pass"

# Limpar todos os secrets
dotnet user-secrets clear
```

### 15.3 Executar Projeto
```bash
# Modo desenvolvimento
dotnet run

# Modo watch (auto-reload)
dotnet watch run

# Build
dotnet build
```

### 15.4 Git
```bash
# Ver estado
git status

# Adicionar alterações
git add .

# Commit
git commit -m "mensagem"

# Push
git push origin main
```

---

## 16. ⚠️ ALERTAS DE SEGURANÇA CRÍTICOS

### 🔴 PROBLEMA CRÍTICO - Credenciais Expostas
**Ficheiro:** `appsettings.json` (linhas 13-19)
**Risco:** ALTO - Credenciais SMTP expostas em ficheiro versionado

```json
"Smtp": {
  "Host": "smtp.gmail.com",
  "Port": 587,
  "EnableSsl": true,
  "User": "404ride@gmail.com",
  "Pass": "yvay wxoq jyih abgl",  // ⚠️ EXPOSTO!
  "From": "404 RIDE <404ride@gmail.com>"
}
```

**AÇÃO IMEDIATA NECESSÁRIA:**

1. **Opção 1: User Secrets (RECOMENDADO para desenvolvimento)**
   ```bash
   # Remover do appsettings.json
   dotnet user-secrets init
   dotnet user-secrets set "Smtp:Host" "smtp.gmail.com"
   dotnet user-secrets set "Smtp:Port" "587"
   dotnet user-secrets set "Smtp:EnableSsl" "true"
   dotnet user-secrets set "Smtp:User" "404ride@gmail.com"
   dotnet user-secrets set "Smtp:Pass" "yvay wxoq jyih abgl"
   dotnet user-secrets set "Smtp:From" "404 RIDE <404ride@gmail.com>"
   ```

2. **Opção 2: appsettings.Development.json (alternativa)**
   - Criar `appsettings.Development.json` (já está no .gitignore)
   - Mover a secção "Smtp" para este ficheiro
   - Remover do `appsettings.json`

3. **Opção 3: Variáveis de Ambiente (produção)**
   ```bash
   export Smtp__Host="smtp.gmail.com"
   export Smtp__Port="587"
   export Smtp__Pass="yvay wxoq jyih abgl"
   ```

**NOTA:** Se este código já foi commitado ao Git, considerar:
- Regenerar a password da conta Gmail
- Limpar histórico do Git (git filter-branch ou BFG Repo-Cleaner)
- Nunca mais commitar credenciais!

### 🟡 Outras Considerações de Segurança

**✅ BOM:**
- Passwords hashadas via Identity (BCrypt/PBKDF2)
- HTTPS redirect configurado
- Anti-forgery tokens nas forms
- Cookie HttpOnly ativado
- Lockout protection ativa (5 tentativas)

**⚠️ A MELHORAR:**
- Email confirmation está desativada (linha 30 Program.cs): `RequireConfirmedEmail = false`
- Connection strings em appsettings.json (OK para desenvolvimento, mas usar Secrets em produção)

---

## 17. NOTAS IMPORTANTES

### ⚠️ ATENÇÃO - PRAZOS E REQUISITOS

**PRAZOS:**
- 📅 **Entrega Fase 3:** 5 de janeiro de 2026 (19 dias restantes! ⏰)
- 🎤 **Apresentação:** 6 a 10 de janeiro de 2026

**REQUISITOS CRÍTICOS DA FASE 3:**
- 🔥 **Ligar AnunciosController à BD** (eliminar mockups) - PRIORITÁRIO
- 🔥 **Sistema de Reservas e Visitas** funcional - PRIORITÁRIO
- 🔥 **Upload de imagens** funcional - PRIORITÁRIO
- 🔥 **Dashboard Administrativo** com estatísticas - PRIORITÁRIO
- 🔥 **Relatório da Fase 3** deve ser elaborado durante o desenvolvimento
- ⚠️ **Resolver credenciais SMTP expostas** - Ver secção 16

**IMPORTANTE:**

**Bugs conhecidos (corrigir brevemente):**
- Encoding corrompido em `UtilizadoresController` (métodos retornam `J�on` em vez de `Json`, e `IsIn�ole` em vez de `IsInRole`) — pode causar falhas em runtime. Requer revisão de encoding do ficheiro e correção dos identificadores.
- Credenciais SMTP reais presentes em `appsettings.json` — mover para User Secrets e remover do repositório (ver secção Segurança/Secrets).
- ⚠️ **Sistema de denúncias** é REQUISITO DE **EXAME** (NÃO de avaliação contínua)
- ⚠️ Denúncias podem ser desenvolvidas **depois**, para época de exames
- ✅ Focar em funcionalidades core do marketplace para a Fase 3

**FICHEIROS NO .GITIGNORE:**
- `contexto.md` - NÃO fazer commit deste ficheiro
- `appsettings.Development.json` - Para credenciais locais
- `*.user` - Configurações pessoais do Visual Studio

### 💡 DICAS
- Usar pattern de nomenclatura consistente (camelCase para variáveis, PascalCase para classes)
- Testar todas as funcionalidades antes da apresentação
- Manter o relatório atualizado durante o desenvolvimento
- Fazer commits frequentes com mensagens descritivas
- Documentar decisões técnicas importantes

---

## 18. CORREÇÕES RECENTES

### 02/01/2026 - Correção de Erros em Visitas Agendadas por Vendedores

**Problemas Reportados:**

1. **Erro 403 - Acesso Negado:**
   Quando um vendedor agendava uma visita a um anúncio de outro vendedor (agindo como comprador), os botões "Ver Detalhes", "Ver Anúncio", "Editar" e "Cancelar" na página de perfil retornavam erro **403 - Acesso Negado**.

2. **Erro de Entity Framework Tracking:**
   Ao cancelar uma visita agendada, ocorria erro:
   ```
   InvalidOperationException: The instance of entity type 'Comprador' cannot be tracked because another instance with the same key value for {'Id'} is already being tracked.
   ```

**Causas Raiz:**

1. **Problema de Autorização (403):**
   O sistema estava a usar queries separadas para buscar utilizadores em `_context.Compradores` e `_context.Vendedores` (DbSets com discriminadores TPH diferentes). Isto causava problemas quando um vendedor tentava aceder às suas próprias visitas agendadas, porque o código não estava a encontrar corretamente o utilizador no contexto de domínio.

2. **Problema de Entity Tracking:**
   O código criava novos objetos `Comprador` e `Vendedor` e atribuía-os às propriedades de navegação da visita **antes** de chamar `SaveChangesAsync()`. Isto causava conflito de tracking no EF porque:
   - A visita já estava sendo tracked pelo contexto
   - Os novos objetos Comprador/Vendedor tinham IDs que já existiam no contexto (do `domainUser`)
   - EF tentava fazer tracking de múltiplas instâncias com o mesmo ID

**Código Problemático 1 - Autorização:**
```csharp
// Buscar utilizador de domínio
Utilizador? domainUser = await _context.Compradores.FirstOrDefaultAsync(c => c.IdentityUserId == user.Id);
if (domainUser == null)
    domainUser = await _context.Vendedores.FirstOrDefaultAsync(v => v.IdentityUserId == user.Id);

if (domainUser == null || (domainUser.Id != visita.CompradorId && domainUser.Id != visita.VendedorId))
    return Forbid();
```

**Código Problemático 2 - Entity Tracking:**
```csharp
// PROBLEMA: Cria novos objetos e atribui à visita ANTES de SaveChanges
var utilizadorComprador = await _context.Set<Utilizador>()
    .FirstOrDefaultAsync(u => u.Id == visita.CompradorId);

visita.Comprador = new Comprador  // ← Causa conflito de tracking!
{
    Id = utilizadorComprador.Id,
    Nome = utilizadorComprador.Nome,
    Email = utilizadorComprador.Email
};

visita.Estado = "Cancelada";
await _context.SaveChangesAsync(); // ← ERRO aqui!
```

**Soluções Implementadas:**

1. **Solução Autorização:**
   Buscar diretamente na tabela base `Utilizador` usando `_context.Set<Utilizador>()`:

   ```csharp
   // Buscar utilizador de domínio (independente do discriminador TPH)
   var domainUser = await _context.Set<Utilizador>()
       .FirstOrDefaultAsync(u => u.IdentityUserId == user.Id);

   if (domainUser == null)
   {
       Console.WriteLine($"[ERRO] Utilizador não encontrado: {user.Id}");
       return Forbid();
   }

   // Permitir acesso se for comprador OU vendedor da visita
   if (domainUser.Id != visita.CompradorId && domainUser.Id != visita.VendedorId)
   {
       Console.WriteLine($"[ERRO] Acesso negado.");
       return Forbid();
   }
   ```

2. **Solução Entity Tracking:**
   Usar `.AsNoTracking()` e criar variáveis locais em vez de atribuir à visita:

   ```csharp
   // Buscar dados SEM anexar ao contexto
   var utilizadorComprador = await _context.Set<Utilizador>()
       .AsNoTracking()  // ← Não faz tracking!
       .FirstOrDefaultAsync(u => u.Id == visita.CompradorId);

   visita.Estado = "Cancelada";
   await _context.SaveChangesAsync(); // ✅ Funciona!

   // Usar dados em variável local para email
   if (utilizadorComprador != null)
   {
       await _emailSender.SendAsync(utilizadorComprador.Email, ...);
   }
   ```

**Métodos Corrigidos em `Controllers/VisitasController.cs`:**

**Problema 1 (Autorização 403):**
1. ✅ **Details** (GET) - Linha 150
2. ✅ **Edit** (GET) - Linha 515
3. ✅ **Edit** (POST) - Linha 590
4. ✅ **Cancelar** (POST) - Linha 760
5. ✅ **Delete** (GET) - Linha 872
6. ✅ **Delete** (POST) - Linha 930

**Problema 2 (Entity Tracking):**
7. ✅ **Confirmar** (POST) - Linha 700 - Adicionado `.AsNoTracking()`
8. ✅ **Cancelar** (POST) - Linha 760 - Adicionado `.AsNoTracking()`
9. ✅ **Edit** (POST) - Linha 650 - Adicionado `.AsNoTracking()` em queries de email

**Logs de Debug Adicionados:**
Para facilitar diagnóstico futuro, foram adicionados logs `Console.WriteLine` em cada validação:
- Log de sucesso quando acesso é permitido
- Log de erro quando utilizador não é encontrado
- Log de erro quando acesso é negado (com IDs para debug)

**Impacto:**
- ✅ Vendedores podem agora aceder, editar e cancelar visitas que agendaram
- ✅ Sistema mantém segurança: apenas intervenientes da visita (comprador OU vendedor) têm acesso
- ✅ Elimina a necessidade de criar contas separadas de Comprador e Vendedor
- ✅ Resolve problema de design TPH onde utilizador só pode ter um discriminador

**Teste Recomendado:**
1. Login como vendedor
2. Agendar visita a anúncio de outro vendedor
3. Aceder ao perfil → Aba "Visitas Agendadas"
4. Clicar nos botões: Ver Detalhes, Editar, Cancelar
5. Verificar que não há mais erro 403

---

### 30/12/2025 (Tarde) - Sistema de Destaque de Anúncios com Stripe

**Contexto:** Implementação completa de um sistema de destaque pago para anúncios, permitindo que vendedores paguem para ter seus anúncios em destaque no topo das listagens.

**Alterações no Modelo de Dados:**

Migration: `AdicionarCamposDestaque` (20251230230024)

**Campos adicionados à tabela `Anuncios`:**
```csharp
public bool Destacado { get; set; } = false;
public DateTime? DataDestaque { get; set; }
public DateTime? DestaqueAte { get; set; }
```

**Funcionalidades Implementadas:**

1. **Sistema de Pagamento via Stripe** ✅
   - **Preço fixo:** 9,99€ por 30 dias de destaque
   - **Integração completa** com Stripe Checkout
   - **Metadata:** Inclui `anuncio_id`, `dias_destaque`, `tipo` (destaque)
   - **Callback de sucesso:** `DestaqueSuccess` confirma pagamento e ativa destaque

2. **Actions do Controller** (`Controllers/AnunciosController.cs`)
   - **`DestacarAnuncio(id)`** (linhas 709-744):
     - GET action para mostrar página de confirmação
     - Verifica se anúncio pertence ao vendedor atual
     - Valida se anúncio já está destacado

   - **`ProcessarDestaque(id)`** (linhas 746-813):
     - POST action que cria sessão Stripe
     - Configura line items com valor e descrição
     - Redireciona para checkout Stripe

   - **`DestaqueSuccess(session_id)`** (linhas 815-864):
     - Valida pagamento via Stripe
     - Atualiza campos `Destacado`, `DataDestaque`, `DestaqueAte`
     - Redireciona para detalhes do anúncio

3. **Ordenação de Listagem** (`Controllers/AnunciosController.cs`, linhas 89-107)
   - **Anúncios destacados sempre primeiro** independentemente da ordenação
   - Ordenação aplicada: `OrderByDescending(a => a.Destacado && a.DestaqueAte > DateTime.Now)`
   - Funciona com todos os tipos de ordenação: preço, ano, km, relevância

4. **View de Confirmação** (`Views/Anuncios/DestacarAnuncio.cshtml`)
   - **Página de confirmação elegante** antes do pagamento
   - Mostra:
     - Informações do anúncio (imagem, título, preço)
     - Plano de destaque com benefícios
     - Valor (9,99€) e duração (30 dias)
     - Botão de pagamento Stripe
   - **Benefícios destacados:**
     - Aparecer no topo das listagens
     - Badge "Destaque" visível
     - Maior visibilidade
     - Venda mais rápido

5. **Badges Visuais** ✅
   - **Na listagem** (`Views/Anuncios/Index.cshtml`, linhas 312-315):
     ```razor
     @if (anuncio.Destacado && anuncio.DestaqueAte.HasValue && anuncio.DestaqueAte.Value > DateTime.Now)
     {
         <span class="badge bg-warning text-dark">
             <i class="bi bi-star-fill me-1"></i>Destaque
         </span>
     }
     ```

   - **Na página de detalhes** (`Views/Anuncios/Details.cshtml`, linhas 61-64):
     - Badge "Destaque" no topo da página
     - Dinâmico baseado no estado real do anúncio

6. **Botão de Destaque para Vendedores** (`Views/Anuncios/Details.cshtml`, linhas 492-507)
   - **Visível apenas para o próprio vendedor**
   - **Estados:**
     - Se **não destacado**: Botão "Destacar Anúncio" (amarelo/warning)
     - Se **já destacado**: Alert informativo com data de expiração
   - Localizado na secção "Gerir o seu anúncio"

**Validações de Segurança:**
- ✅ Apenas vendedores podem destacar
- ✅ Verificação de proprietário do anúncio
- ✅ Validação de pagamento via Stripe
- ✅ Verificação de duplicação (se já destacado)
- ✅ Role `[Authorize(Roles = "Vendedor")]` em todas as actions

**Fluxo Completo:**
1. Vendedor acede aos detalhes do seu anúncio
2. Clica no botão "Destacar Anúncio"
3. Visualiza página de confirmação com benefícios
4. Clica "Pagar com Stripe"
5. Completa pagamento no Stripe Checkout
6. Sistema valida pagamento
7. Anúncio é marcado como destacado por 30 dias
8. Anúncio aparece no topo de todas as listagens
9. Badge "Destaque" fica visível em listagem e detalhes

**Ficheiros Modificados:**
- ✅ `Models/Anuncio.cs` - Campos de destaque
- ✅ `Controllers/AnunciosController.cs` - 3 novas actions + ordenação
- ✅ `Views/Anuncios/Index.cshtml` - Badge de destaque
- ✅ `Views/Anuncios/Details.cshtml` - Badge + botão de destaque
- ✅ `Views/Anuncios/DestacarAnuncio.cshtml` - Nova view (criada)

**Migration:**
- ✅ `20251230230024_AdicionarCamposDestaque` - Aplicada com sucesso

**Testes:**
- ✅ Build bem-sucedido (0 erros)
- ✅ Migration aplicada à base de dados

---

### 30/12/2025 (Manhã) - Funcionalidade de Comparação de Veículos + Correção de Caminhos de Imagens

**Contexto:** A funcionalidade de comparação de veículos estava previamente implementada mas tinha sido perdida durante o desenvolvimento. As imagens dos anúncios não estavam a aparecer devido a inconsistência nos caminhos.

**Soluções Implementadas:**

1. **Funcionalidade de Comparação Restaurada** ✅

   **a) Botão "COMPARAR" nos Cards de Veículos** (`Views/Anuncios/Index.cshtml`, linhas 340-361)
   - Adicionado botão "COMPARAR" em cada card de veículo
   - Posicionado verticalmente (por baixo do botão "Ver Detalhes")
   - Integrado com JavaScript existente via `onclick="addToCompare(...)"`
   - Passa todos os dados do veículo: id, titulo, imagem, preço, ano, km, combustível, caixa, potência, cilindrada
   - Estados do botão:
     - Normal: `<i class="bi bi-arrow-left-right"></i> Comparar`
     - Ativo: `<i class="bi bi-check-circle-fill"></i> Adicionado`

   **b) CSS para Layout Vertical** (`wwwroot/css/site.css`, linhas 2181-2186)
   ```css
   .card-footer-actions {
       display: flex;
       flex-direction: column;
       gap: 0.5rem;
       margin-top: 1rem;
   }
   ```

   **c) Infraestrutura Existente (já implementada anteriormente):**
   - **JavaScript** (`Views/Anuncios/Index.cshtml`, linhas 473-647):
     - `addToCompare()` - Adiciona/remove veículos (max 3)
     - `removeFromComparison()` - Remove veículo específico
     - `clearComparison()` - Limpa toda a comparação
     - `updateComparisonBar()` - Atualiza barra flutuante
     - localStorage para persistência de dados

   - **Barra Flutuante de Comparação** (`Views/Anuncios/Index.cshtml`, linhas 384-408):
     - Aparece na parte inferior quando há veículos selecionados
     - Mostra thumbnails, títulos e preços
     - Botão "Comparar" (ativado com ≥2 veículos)
     - Botão "Limpar" para resetar seleção

   - **Página de Comparação** (`Views/Anuncios/Compare.cshtml`):
     - Tabela side-by-side com especificações completas
     - Comparação de: preço, ano, km, combustível, transmissão, potência, cilindrada
     - Mockup de extras (GPS, câmara, AC, sensores, LED)
     - Carregamento dinâmico via localStorage

   - **Controller** (`Controllers/AnunciosController.cs`, linhas 615-621):
     - Ação `Compare()` retorna a view
     - Dados carregados do localStorage no client-side

   - **CSS Completo** (`wwwroot/css/site.css`, linhas 2593-3005):
     - `.comparison-bar` - Barra flutuante com animação
     - `.btn-compare.active` - Estado ativo do botão
     - `.comparison-table` - Tabela de comparação responsiva
     - Media queries para mobile

2. **Correção de Caminhos de Imagens** ✅

   **Problema Identificado:**
   - **AnuncioSeeder.cs** usava: `/imagens/anuncios/{id}/foto-XX.jpg` (português)
   - **AnunciosController.cs** usava: `/images/anuncios/{id}/foto-XX.jpg` (inglês)
   - Diretório físico: `wwwroot/imagens/` (português)
   - Resultado: Imagens adicionadas via controller não carregavam

   **Solução:**

   **a) Controller Corrigido** (`Controllers/AnunciosController.cs`, linha 669)
   ```csharp
   // ❌ ANTES
   ImagemCaminho = $"/images/anuncios/{anuncioId}/{nomeUnico}"

   // ✅ DEPOIS
   ImagemCaminho = $"/imagens/anuncios/{anuncioId}/{nomeUnico}"
   ```

   **b) Scripts SQL Criados:**

   - **`fix_image_paths.sql`** - Corretor de caminhos existentes:
     ```sql
     UPDATE Imagens
     SET ImagemCaminho = REPLACE(ImagemCaminho, '/images/', '/imagens/')
     WHERE ImagemCaminho LIKE '/images/%';
     ```
     - Resultado: 0 registos a corrigir (tabela estava vazia)

   - **`populate_images.sql`** - Populador da tabela Imagens:
     ```sql
     -- Para cada anúncio (1-21), insere 3 imagens
     INSERT INTO Imagens (ImagemCaminho, AnuncioId)
     VALUES ('/imagens/anuncios/{id}/foto-01.jpg', {id}),
            ('/imagens/anuncios/{id}/foto-02.jpg', {id}),
            ('/imagens/anuncios/{id}/foto-03.jpg', {id});
     ```
     - Executado com sucesso via `sqlcmd`
     - Resultado: **63 imagens inseridas** (21 anúncios × 3 imagens)

**Funcionalidades Completas:**

1. **Fluxo de Comparação:**
   - Utilizador navega em `/Anuncios`
   - Clica "COMPARAR" em até 3 veículos
   - Barra flutuante aparece na parte inferior
   - Botão "Comparar" fica ativo (≥2 veículos)
   - Redireciona para `/Anuncios/Compare`
   - Tabela mostra comparação lado a lado
   - Pode remover veículos individualmente
   - Pode limpar toda a comparação

2. **Persistência:**
   - Dados guardados em `localStorage`
   - Chaves: `compareVehicles` (objetos completos), `compareIds` (apenas IDs)
   - Estado dos botões restaurado ao recarregar página
   - Comparação persiste entre navegações

3. **Validações:**
   - Máximo 3 veículos simultaneamente
   - Alert quando tenta adicionar mais que 3
   - Mínimo 2 veículos para ativar botão "Comparar"
   - Toggle: clicar novamente remove da comparação

**Ficheiros Criados:**
- `fix_image_paths.sql` - Script de correção de caminhos
- `populate_images.sql` - Script de população de imagens

**Ficheiros Modificados:**
- ✅ `Views/Anuncios/Index.cshtml` - Botão "COMPARAR" adicionado (linhas 340-361)
- ✅ `wwwroot/css/site.css` - CSS para layout vertical (linhas 2181-2186)
- ✅ `Controllers/AnunciosController.cs` - Caminho corrigido (linha 669)

**Testes Realizados:**
- ✅ Build bem-sucedido (0 erros, 0 warnings)
- ✅ Scripts SQL executados com sucesso
- ✅ 63 imagens inseridas na base de dados
- ✅ Aplicação iniciada em http://localhost:5184

**Notas Técnicas:**
- A funcionalidade de comparação é totalmente client-side (JavaScript + localStorage)
- Não requer autenticação - disponível para todos os visitantes
- CSS totalmente responsivo com media queries para mobile
- Integração perfeita com design existente (Bootstrap 5 + cores do tema)

---

### 27/12/2025 - Sistema de Compra Completo com Stripe

**Implementação:** Sistema completo de compra de veículos integrado com Stripe, incluindo dedução automática do sinal pago em reservas, emails estilizados e gestão de histórico de compras.

**Ficheiros Criados:**
- `Controllers/ComprasController.cs` - Controller completo de compras
- `Views/Compras/Index.cshtml` - Lista de compras do utilizador
- `Views/Compras/Success.cshtml` - Página de sucesso pós-pagamento
- `Views/Compras/Cancel.cshtml` - Página de cancelamento

**Ficheiros Modificados:**
- `Views/Anuncios/Details.cshtml` - Modal de compra + JavaScript de verificação de reserva
- `Controllers/ReservasController.cs` - Emails HTML estilizados
- `Controllers/UtilizadoresController.cs` - Carregamento de compras
- `Views/Utilizadores/Perfil.cshtml` - Link "Minhas Compras"

**Funcionalidades Principais:**

1. **Modal de Compra Inteligente** (Details.cshtml linhas 988-1131)
   - Verificação automática de reserva ativa via JavaScript
   - Dedução automática do sinal se tiver reserva
   - Mostra breakdown: Preço Total - Sinal Pago = Total a Pagar
   - Design moderno com gradiente verde

2. **Fluxo de Compra com Stripe** (ComprasController.cs)
   - Verifica se tem reserva ativa
   - Calcula valor correto: `valorAPagar = anuncio.Preco - valorSinal` (se tiver reserva)
   - Cria sessão Stripe com metadata completa
   - Processa pagamento e cria compra na BD
   - Marca reserva como "Concluída" (se existir)

3. **Emails HTML Estilizados** (ReservasController.cs e ComprasController.cs)
   - Templates HTML completos com gradientes e design profissional
   - Email de reserva com **botão de link direto** para concluir compra
   - Email de compra mostra breakdown de pagamento (sinal + restante)
   - Usa variáveis dinâmicas do domínio para links

4. **Gestão de Compras**
   - View Index.cshtml com lista de todas as compras
   - Cards bonitos com imagem, detalhes e status
   - Integração no perfil do utilizador (badge com contador)

**Fluxo Completo:**
1. Comprador reserva veículo → Paga sinal via Stripe
2. Recebe email com link de pagamento direto
3. Clica "Comprar Agora" → Modal detecta reserva automaticamente
4. Mostra valor a pagar (preço - sinal)
5. Processa pagamento do restante
6. Marca reserva como concluída
7. Envia emails de confirmação de venda

---

### 27/12/2025 (Tarde) - Sistema de Estados de Anúncios e Secção Minhas Compras

**Problema Reportado:**
1. Secção "Minhas Compras" não aparecia nada no perfil do utilizador
2. Veículos vendidos continuavam a aparecer na listagem pública
3. Modelo Anuncio não tinha campo de Estado (faltava padrão de design em relação a outras entidades)

**Evolução da Solução:**
- **Inicialmente:** Adicionado campo booleano `Vendido`
- **Refinamento:** Substituído por campo `Estado` (string) para maior flexibilidade e consistência com outras entidades (Reserva, Visita, Compra, Utilizador)

**Soluções Implementadas:**

1. **Secção "Minhas Compras" Adicionada** (`Views/Utilizadores/Perfil.cshtml`, linha 1916)
   - Nova tab completa com lista de compras do comprador
   - Cards detalhados com imagem, informações do veículo e dados de pagamento
   - Mostra: Data da compra, Valor pago, Estado de pagamento
   - Informações do vendedor com foto de perfil
   - Links para contactar vendedor e ver anúncio
   - Estado vazio estilizado quando não há compras
   - Total de compras exibido no badge do menu lateral

2. **Campo "Estado" Adicionado ao Modelo Anuncio** (`Models/Anuncio.cs`, linha 50-52)
   ```csharp
   [StringLength(20)]
   public string Estado { get; set; } = "Ativo";
   ```
   **Estados possíveis:**
   - `"Ativo"` - Anúncio visível e disponível para compra (padrão)
   - `"Reservado"` - Tem reserva ativa
   - `"Vendido"` - Foi comprado
   - `"Pausado"` - Vendedor pausou temporariamente
   - `"Bloqueado"` - Bloqueado por admin
   - `"Expirado"` - Anúncio expirou

3. **Gestão Automática de Estados:**

   **a) Reserva Criada** (`Controllers/ReservasController.cs`, linha 233)
   ```csharp
   anuncio.Estado = "Reservado";
   ```

   **b) Compra Concluída** (`Controllers/ComprasController.cs`, linha 244)
   ```csharp
   anuncio.Estado = "Vendido";
   ```

4. **Filtragem Inteligente de Anúncios** (`Controllers/AnunciosController.cs`, linha 39)
   ```csharp
   .Where(a => a.Estado == "Ativo" || a.Estado == "Reservado")
   ```
   - Anúncios "Vendido", "Bloqueado", "Pausado" e "Expirado" não aparecem na listagem pública
   - Anúncios "Reservado" continuam visíveis (transparência para compradores)

**Migration com Migração de Dados:**
- **Migration:** `20251227222440_SubstituirVendidoPorEstado`
- **Processo em 3 etapas:**
  1. Adiciona coluna `Estado` (nvarchar(20), default "Ativo")
  2. Migra dados existentes:
     ```sql
     UPDATE Anuncios
     SET Estado = CASE
         WHEN Vendido = 1 THEN 'Vendido'
         ELSE 'Ativo'
     END
     ```
  3. Remove coluna `Vendido` (bit)
- **Resultado:** Todos os anúncios existentes preservam o estado correto

**Correções de Erros de Compilação:**
- **ComprasController.cs (linha 262):** Corrigido conversão `decimal?` → `decimal`
- **ReservasController.cs (linha 239):** Mesma correção de conversão
- **Views/Compras/Cancel.cshtml (linha 97):** Escapado `@keyframes` → `@@keyframes`
- **Views/Compras/Success.cshtml (linhas 197, 205):** Escapado `@keyframes` → `@@keyframes`

**Ficheiros Modificados:**
- ✅ `Models/Anuncio.cs` - Substituído `Vendido` (bool) por `Estado` (string)
- ✅ `Controllers/ComprasController.cs` - Usa `Estado = "Vendido"`
- ✅ `Controllers/ReservasController.cs` - Usa `Estado = "Reservado"`
- ✅ `Controllers/AnunciosController.cs` - Filtra por Estado
- ✅ `Views/Utilizadores/Perfil.cshtml` - Secção "Minhas Compras" completa
- ✅ `Views/Compras/Cancel.cshtml` - Correção Razor
- ✅ `Views/Compras/Success.cshtml` - Correção Razor
- ✅ `Migrations/20251227222440_SubstituirVendidoPorEstado.cs` - Migration personalizada com migração de dados

**Fluxo Completo Atualizado:**

**Cenário 1: Compra Direta**
1. Anúncio em estado "Ativo"
2. Comprador compra → Estado muda para "Vendido"
3. Anúncio desaparece da listagem pública
4. Compra aparece em "Minhas Compras"

**Cenário 2: Compra com Reserva**
1. Anúncio em estado "Ativo"
2. Comprador reserva → Estado muda para "Reservado" ✨
3. Anúncio continua visível mas marcado como reservado
4. Comprador completa compra → Estado muda para "Vendido"
5. Anúncio desaparece da listagem pública
6. Compra aparece em "Minhas Compras"

---

### 27/12/2025 - Integração do Stripe no Modal de Reserva

**Problema:** Foi criado um novo formulário de reserva (Create.cshtml) que não usava o modal existente e calculava o valor do sinal como percentagem do preço (10%), em vez de usar o `ValorSinal` definido pelo vendedor no anúncio.

**Solução Implementada:**

1. **ReservasController.cs:**
   - Modificado método `Create` (linha 111): Agora usa `anuncio.ValorSinal` em vez de `CalcularValorReserva()`
   - Modificado método `CreateCheckoutSession` (linha 138): Usa `anuncio.ValorSinal` diretamente
   - Removido método helper `CalcularValorReserva()` que calculava percentagem
   - Fallback: Se `ValorSinal` não estiver definido (= 0), usa 10% do preço como backup

2. **Views/Anuncios/Details.cshtml:**
   - **Modal de Reserva (linhas 649-823):**
     - Integrado Stripe no formulário existente do modal
     - Formulário agora submete via POST para `Reservas/CreateCheckoutSession`
     - Simplificado o formulário - removidos campos desnecessários (nome, email, telefone)
     - Adicionado alert informativo sobre redirecionamento para Stripe
     - Botão "Confirmar Reserva" agora é `type="submit"` com `form="formReserva"`
   - **Botão "Reservar Veículo" (linha 472):**
     - Alterado de link para botão que abre o modal (`data-bs-toggle="modal"`)
   - **JavaScript (linha 1020):**
     - Removida lógica mockup do `btnConfirmarReserva`
     - Modal agora funciona com submit real do formulário

3. **Create.cshtml de Reservas:**
   - Mantida como backup (tem bom conteúdo de UI)
   - Não está sendo usada no fluxo principal (preferência pelo modal)

**Fluxo de Reserva Atual:**
1. Utilizador clica em "Reservar Veículo" na página de detalhes do anúncio
2. Modal de reserva abre com informações do veículo e valor do sinal (`Model.ValorSinal`)
3. Utilizador aceita termos e clica em "Pagar Sinal com Stripe"
4. Sistema redireciona para Stripe Checkout com o valor correto (`ValorSinal`)
5. Após pagamento bem-sucedido, cria a reserva na BD
6. Envia emails de confirmação ao comprador e vendedor
7. Redireciona para página de sucesso (`Reservas/Success`)

**Ficheiros Alterados:**
- `Controllers/ReservasController.cs`
- `Views/Anuncios/Details.cshtml`

---

### 26/12/2025 - Feedback Visual em Formulários de Visitas

## 19. CORREÇÕES ANTIGAS (26/12/2025)

### ✅ Erro de Compilação Razor - Views/Visitas/Create.cshtml
**Problema:** Erro RZ1010 - `@{ }` dentro de bloco `@if { }`
**Linha:** 140
**Solução:** Removido `@{ }` desnecessário (já em contexto C# dentro do `@if`)

```csharp
// ❌ ANTES
@if (temDisponibilidades && slotsDisponiveis.Any())
{
    @{
        var slotsPorDia = slotsDisponiveis.Take(60).GroupBy(s => s.Date).Take(14);
    }
}

// ✅ DEPOIS
@if (temDisponibilidades && slotsDisponiveis.Any())
{
    var slotsPorDia = slotsDisponiveis.Take(60).GroupBy(s => s.Date).Take(14);
}
```

### ✅ Melhoria de Feedback Visual - Formulário de Visitas
**Problema:** Após submeter o formulário de agendamento, não havia feedback visual claro de sucesso/erro
**Solução Implementada:**
- ✅ Adicionado alert de erro visível no topo do formulário quando ModelState é inválido
- ✅ Mensagem de sucesso já existia na view Index (via TempData) - funcional
- ✅ Removido validation-summary duplicado do formulário

**Ficheiros Alterados:**
- `Views/Visitas/Create.cshtml` (linhas 34-47)
- `Controllers/VisitasController.cs` (já tinha TempData configurado - linha 309)

**Fluxo Atual:**
1. Utilizador submete formulário
2. **Se válido:** Redireciona para Index com mensagem de sucesso verde + botão "Ver Detalhes"
3. **Se inválido:** Recarrega Create com alert vermelho mostrando erros específicos

### ✅ Erro de Validação - Modelo Visita
**Problema:** Erro "The Comprador/Anuncio/Vendedor field is required" ao submeter formulário
**Causa:** Propriedades de navegação estavam a ser validadas durante o model binding
**Solução:** Adicionado atributo `[ValidateNever]` nas propriedades de navegação

**Ficheiro Alterado:**
- `Models/Visita.cs` (linhas 28, 34, 40, 46)
- Adicionado `using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;`

```csharp
[ValidateNever]
public Comprador Comprador { get; set; } = null!;

[ValidateNever]
public Anuncio Anuncio { get; set; } = null!;

[ValidateNever]
public Vendedor Vendedor { get; set; } = null!;

[ValidateNever]
public Reserva? Reserva { get; set; }
```

**Nota:** Esta correção pode ser necessária em outros modelos com relações (Reserva, Compra, etc.)

### ✅ Erro 404 - Botão "Ver Detalhes da Visita"
**Problema:** Após agendar visita com sucesso, o botão "Ver Detalhes da Visita" redirecionava para URL incorreta (erro 404)
**Causa:** Faltava especificar o controller no tag helper `asp-action`
**Solução:** Adicionado `asp-controller="Visitas"` ao link

**Ficheiro Alterado:**
- `Views/Visitas/Index.cshtml` (linha 65)

```csharp
// ❌ ANTES (linha 65)
<a asp-action="Details" asp-route-id="@TempData["VisitaId"]" class="btn btn-sm btn-success mt-2">

// ✅ DEPOIS
<a asp-controller="Visitas" asp-action="Details" asp-route-id="@TempData["VisitaId"]" class="btn btn-sm btn-success mt-2">
```

**Nota:** Sempre especificar explicitamente o controller em tag helpers para evitar ambiguidade de roteamento.

---

## 19. CONTACTOS DA EQUIPA

- **Bruno Alves:** al80990@utad.eu
- **Liane Duarte:** al79012@utad.eu
- **Pedro Braz:** al81311@utad.eu

---

### 02/01/2026 - Branding do Email de Confirmação

- Email de confirmação de registo passa a mostrar o nome correto "404 Ride" (antes "DriveDeal").
- Template de email inclui agora o logotipo oficial no cabeçalho (`https://404ride.b-host.me/imagens/logo.png`).

### 02/01/2026 - Obrigatoriedade de Email Confirmado para Login

- `RequireConfirmedEmail` ativado (Program.cs) — utilizador só faz login após confirmar email.
- Mensagem dedicada no login para contas não confirmadas + instrução de reenvio.
- Registo deixa de marcar email como confirmado automaticamente em caso de falha de envio.

---

**FIM DO CONTEXTO**

> Este ficheiro será atualizado conforme o projeto evolui.
> **Última revisão:** 2025-12-26
>
> **Principais alterações desta revisão:**
> - ✅ **Correção:** Erro de compilação RZ1010 em Views/Visitas/Create.cshtml (linha 140)
> - ✅ **Melhoria:** Feedback visual no formulário de agendamento de visitas
>   - Alert de erro visível quando há problemas de validação
>   - Mensagem de sucesso com botão "Ver Detalhes" após criar visita
> - 🔥 **Alerta:** Apenas 10 dias restantes para entrega da Fase 3 (5 janeiro 2026)
