# CONTEXTO DO PROJETO - DriveDeal (404 Car Marketplace)

> **Ficheiro de contexto para sessões futuras com Claude Code**
> **Última atualização:** 2025-12-03 (Atualização Fase 3 - Filtros guardados + Notificações)
> **Fase atual:** Fase 3 (em desenvolvimento ativo - Sprint final)
> **Prazo de entrega:** 5 de janeiro de 2026 (47 dias restantes)

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

## 18. CONTACTOS DA EQUIPA

- **Bruno Alves:** al80990@utad.eu
- **Liane Duarte:** al79012@utad.eu
- **Pedro Braz:** al81311@utad.eu

---

**FIM DO CONTEXTO**

> Este ficheiro será atualizado conforme o projeto evolui.
> **Última revisão:** 2025-11-19
>
> **Principais alterações desta revisão:**
> - ✅ Atualização de progresso: ~60% completo (anteriormente 45%)
> - ✅ Documentação de funcionalidades recentemente implementadas:
>   - Sistema de filtros dinâmico funcional
>   - Gestão de utilizadores completa
>   - Edição de perfil funcional
>   - CRUD de anúncios completo
> - ✅ Adição de View Components implementados (GerirUtilizadores, ModerarAnuncios)
> - ✅ Correção da data de última atualização (era 2025-12-17, corrigido para 2025-11-19)
> - ✅ Atualização do roadmap com base no progresso real
> - ✅ Adição de tempo restante: 47 dias até entrega (5 jan 2026)
> - ⚠️ Identificação de ficheiro "nul" não rastreado para remoção
> - ⚠️ 12 ocorrências de TODO/FIXME/IMPORTANTE identificadas no código
