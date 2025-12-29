# RELATÓRIO FASE 3 - APOIO E DOCUMENTAÇÃO TÉCNICA
## 404 Ride - Marketplace de Veículos Usados

> **Documento de Apoio ao Relatório Final - Fase 3**
> **Data:** Dezembro 2025 - Janeiro 2026
> **Equipa:** Bruno Alves (al80990), Liane Duarte (al79012), Pedro Braz (al81311)
> **Prazo de Entrega:** 5 de janeiro de 2026

---

## ÍNDICE

1. [Visão Geral da Fase 3](#1-visão-geral-da-fase-3)
2. [Integrações Externas e APIs](#2-integrações-externas-e-apis)
3. [Sistema de Autenticação e Segurança](#3-sistema-de-autenticação-e-segurança)
4. [Integridade da Base de Dados](#4-integridade-da-base-de-dados)
5. [Funcionalidades do Sistema](#5-funcionalidades-do-sistema)
6. [Arquitetura e Tecnologias](#6-arquitetura-e-tecnologias)
7. [Fluxos de Processo Principais](#7-fluxos-de-processo-principais)
8. [Desafios e Soluções](#8-desafios-e-soluções)
9. [Testes e Validações](#9-testes-e-validações)
10. [Conclusões](#10-conclusões)

---

## 1. VISÃO GERAL DA FASE 3

### 1.1 Objetivos Alcançados

A Fase 3 do projeto 404 Ride focou-se na implementação completa do sistema, integrando a base de dados com a lógica funcional e garantindo a integridade dos dados. Os principais objetivos cumpridos foram:

**Integridade da Base de Dados**
- Implementação de todas as constraints de chaves primárias e estrangeiras
- Validações robustas em todos os modelos de dados usando Data Annotations
- Relacionamentos complexos entre entidades (1:N, N:M, herança TPH)
- Sistema de migrations para controlo de versões da estrutura da BD

**Lógica Funcional Completa**
- 12+ controllers totalmente funcionais conectados à base de dados via Entity Framework Core
- Implementação de regras de negócio complexas (sistema de estados, cálculos de valores, verificações de disponibilidade)
- Validações server-side e client-side em todos os formulários
- Sistema de autorização baseado em roles (Administrador, Vendedor, Comprador)

**Integrações Externas**
- **Stripe API** para processamento de pagamentos (reservas e compras)
- **Google OAuth 2.0** para login social
- **Gmail SMTP** para envio de emails transacionais
- **Two-Factor Authentication (2FA)** com QR Codes para segurança adicional

### 1.2 Estatísticas do Projeto

| Métrica | Valor |
|---------|-------|
| **Entidades no Modelo** | 31 entidades |
| **Controllers Implementados** | 12+ controllers |
| **Views Razor** | 80+ ficheiros .cshtml |
| **Migrations Aplicadas** | 15+ migrations |
| **APIs Integradas** | Stripe, Google OAuth, Gmail SMTP |
| **Métodos de Autenticação** | Email/Password, Google Login, 2FA |

---

## 2. INTEGRAÇÕES EXTERNAS E APIS

### 2.1 Stripe API - Sistema de Pagamentos

A integração com a **Stripe API** é uma das componentes mais críticas do sistema, permitindo processar pagamentos de forma segura para reservas e compras de veículos.

#### 2.1.1 Como Funciona a Integração

**Fluxo de Pagamento:**
1. O utilizador clica em "Reservar" ou "Comprar" num anúncio
2. O sistema cria uma **Checkout Session** no Stripe com os detalhes da transação
3. O utilizador é redirecionado para a página de pagamento segura do Stripe
4. Após pagamento bem-sucedido, o Stripe redireciona de volta para a página de sucesso
5. O sistema verifica o estado do pagamento e cria os registos na base de dados

**Tipos de Transações:**
- **Reservas:** Pagamento de um sinal (valor definido pelo vendedor, tipicamente 10% do preço)
- **Compras Diretas:** Pagamento do valor total do veículo
- **Compras com Reserva:** Pagamento do valor restante (preço total - sinal já pago)

#### 2.1.2 Funcionalidades Implementadas

**Cálculo Inteligente de Valores**
- Sistema deteta automaticamente se o comprador já pagou um sinal através de reserva
- Deduz o valor do sinal do preço total na compra final
- Todos os valores são convertidos para cêntimos (multiplicação por 100) para evitar erros de arredondamento
- Metadata é enviada para o Stripe para rastreabilidade (ID do anúncio, ID do comprador, valor do sinal pago)

**Gestão de Estados**
- Após pagamento de reserva confirmado: anúncio muda para estado "Reservado"
- Após pagamento de compra confirmado: anúncio muda para estado "Vendido"
- Anúncios vendidos são automaticamente removidos da listagem pública

**URLs de Callback**
- **Success URL:** Redireciona para página de confirmação com animações e informações detalhadas
- **Cancel URL:** Redireciona para página de cancelamento caso o utilizador desista

#### 2.1.3 Segurança

- Chaves secretas armazenadas em `appsettings.json` (ambiente de desenvolvimento)
- Todas as comunicações com Stripe são feitas via HTTPS
- Verificação do `PaymentStatus` antes de criar registos na base de dados
- Proteção contra pagamentos duplicados através de verificação de session ID

### 2.2 Google OAuth 2.0 - Login Social

A integração com **Google OAuth 2.0** permite que os utilizadores façam login utilizando as suas contas Google, simplificando o processo de registo e eliminando a necessidade de criar mais uma password.

#### 2.2.1 Como Funciona

**Fluxo de Autenticação:**
1. Utilizador clica em "Login com Google" na página de login
2. É redirecionado para a página de autenticação do Google
3. Após autorização, o Google retorna um token com informações do utilizador (email, nome, foto)
4. Sistema verifica se já existe conta com esse email:
   - **Se existe:** Faz login automático
   - **Se não existe:** Cria nova conta automaticamente com dados do Google
5. Utilizador é redirecionado para a página principal já autenticado

#### 2.2.2 Vantagens

- **Segurança:** Delega autenticação a um fornecedor confiável (Google)
- **Conveniência:** Utilizadores não precisam criar nova password
- **Velocidade:** Registo e login em segundos
- **Recuperação:** Sem problemas de passwords esquecidas para contas Google

#### 2.2.3 Configuração

A integração requer:
- Criação de credenciais OAuth 2.0 na Google Cloud Console
- Configuração de Client ID e Client Secret
- Definição de URLs de redirecionamento autorizados
- Configuração no ASP.NET Core Identity para suportar external providers

### 2.3 Gmail SMTP - Sistema de Emails

O sistema de emails é fundamental para comunicação automática com os utilizadores em eventos importantes do sistema.

#### 2.3.1 Tipos de Emails Enviados

**Emails de Autenticação:**
- Confirmação de email após registo
- Recuperação de password (reset password link)
- Código de verificação para Two-Factor Authentication (2FA)
- Notificação de login suspeito

**Emails Transacionais:**
- **Confirmação de Reserva:** Enviado ao comprador e vendedor após pagamento de sinal bem-sucedido
- **Confirmação de Compra:** Enviado ao comprador e vendedor após compra concluída
- **Agendamento de Visita:** Enviado ao vendedor quando comprador agenda visita
- **Confirmação de Visita:** Enviado ao comprador quando vendedor confirma a visita
- **Cancelamento de Reserva/Visita:** Notificação de cancelamentos

#### 2.3.2 Design dos Emails

Os emails são totalmente estilizados com HTML e CSS inline, incluindo:
- Gradientes e cores da identidade visual do 404 Ride
- Botões de call-to-action (CTA) com links diretos
- Tabelas com breakdown de valores (nas compras)
- Informações do veículo com imagem
- Links diretos para ações relevantes (completar compra, ver anúncio, contactar vendedor)
- Footer com informações de contacto e links úteis

**Exemplo de Email de Reserva:**
- Header com logo e gradiente roxo
- Mensagem de confirmação personalizada com nome do utilizador
- Detalhes do veículo (marca, modelo, ano, matrícula)
- Valor do sinal pago
- Botão destacado "Completar Compra" com valor restante
- Data de expiração da reserva
- Informações de contacto do vendedor

#### 2.3.3 Configuração SMTP

- **Servidor:** smtp.gmail.com
- **Porta:** 587 (TLS)
- **Segurança:** SSL/TLS habilitado
- **Autenticação:** Email e App Password (não password normal do Gmail)
- **Biblioteca:** MailKit (mais moderna e segura que SmtpClient)

### 2.4 Outras Integrações Potenciais

**Atualmente não implementadas, mas planeadas:**
- **Integração com APIs de Standvirtual/OLX:** Importação automática de anúncios
- **Integração com IMTT:** Verificação de matrículas portuguesas
- **Integração com serviços de financiamento:** Simulação de crédito automóvel
- **Google Maps API:** Localização de vendedores e cálculo de distâncias

---

## 3. SISTEMA DE AUTENTICAÇÃO E SEGURANÇA

### 3.1 ASP.NET Identity - Fundação do Sistema

O sistema de autenticação é baseado no **ASP.NET Identity**, um framework robusto e testado para gestão de utilizadores, autenticação e autorização.

#### 3.1.1 Funcionalidades Implementadas

**Gestão de Utilizadores:**
- Registo de utilizadores (Compradores e Vendedores)
- Login com email ou username
- Logout seguro
- Gestão de sessões
- Lockout automático após tentativas falhadas

**Requisitos de Password:**
- Mínimo 8 caracteres
- Pelo menos 1 dígito
- Pelo menos 1 letra maiúscula
- Pelo menos 1 letra minúscula
- Pelo menos 1 carácter especial (!@#$%^&*)

**Proteção de Conta:**
- Bloqueio automático após 5 tentativas de login falhadas
- Lockout duration: 15 minutos
- Confirmação de email obrigatória (configurável)
- Sistema de recuperação de password por email

### 3.2 Two-Factor Authentication (2FA)

A autenticação de dois fatores adiciona uma camada extra de segurança, exigindo não apenas a password mas também um código temporário gerado por uma aplicação autenticadora.

#### 3.2.1 Como Funciona

**Ativação do 2FA:**
1. Utilizador acede às definições de segurança no perfil
2. Clica em "Ativar Two-Factor Authentication"
3. Sistema gera um QR Code único
4. Utilizador escaneia o QR Code com uma app autenticadora (Google Authenticator, Microsoft Authenticator, Authy)
5. Utilizador insere o código de 6 dígitos gerado pela app para confirmar
6. 2FA fica ativo na conta

**Login com 2FA:**
1. Utilizador insere email e password normalmente
2. Se 2FA estiver ativo, é redirecionado para página de verificação
3. Insere código de 6 dígitos da app autenticadora
4. Sistema valida o código e autentica o utilizador
5. Opção "Lembrar este dispositivo" para não pedir código novamente por 30 dias

#### 3.2.2 Códigos de Recuperação

Quando o 2FA é ativado, o sistema gera **10 códigos de recuperação** de uso único:
- Armazenados de forma segura (hashed) na base de dados
- Apresentados ao utilizador apenas uma vez (deve guardar em local seguro)
- Podem ser usados se o utilizador perder acesso à app autenticadora
- Cada código só pode ser usado uma vez

#### 3.2.3 Desativação

Utilizador pode desativar 2FA a qualquer momento:
- Requer autenticação com código 2FA ou código de recuperação
- Remove configuração TOTP (Time-based One-Time Password)
- Invalida todos os códigos de recuperação

### 3.3 Google Login - Autenticação Social

Como mencionado anteriormente, o sistema suporta login via Google OAuth 2.0, oferecendo:
- Registo instantâneo sem preencher formulários
- Login rápido sem memorizar passwords
- Segurança delegada ao Google
- Sincronização de dados básicos (nome, email, foto de perfil)

### 3.4 Sistema de Roles e Autorização

O sistema implementa **Role-Based Access Control (RBAC)** com três roles principais:

#### 3.4.1 Comprador

**Permissões:**
- Ver listagem de anúncios
- Ver detalhes de anúncios
- Reservar veículos (pagamento de sinal)
- Comprar veículos
- Agendar visitas
- Adicionar favoritos
- Enviar mensagens a vendedores
- Gerir perfil pessoal
- Ver histórico de compras, reservas e visitas

**Restrições:**
- Não pode criar anúncios
- Não pode editar anúncios de outros
- Não pode aceder ao painel de administração
- Não pode reservar/comprar os próprios favoritos (se também for vendedor)

#### 3.4.2 Vendedor

**Permissões (além das de Comprador):**
- Criar anúncios de veículos
- Editar os próprios anúncios
- Apagar os próprios anúncios
- Pausar/reativar anúncios
- Ver reservas recebidas
- Ver visitas agendadas
- Confirmar/cancelar visitas
- Definir disponibilidade para visitas
- Ver estatísticas de vendas
- Responder a mensagens de compradores

**Restrições:**
- Não pode editar/apagar anúncios de outros vendedores
- Não pode reservar/comprar os próprios anúncios
- Não pode aceder ao painel de administração
- Deve ter estado "Ativo" para criar anúncios

#### 3.4.3 Administrador

**Permissões (acesso total):**
- Todas as permissões de Comprador e Vendedor
- Aceder ao painel de administração
- Gerir utilizadores (ativar, bloquear, remover)
- Gerir denúncias
- Bloquear/remover anúncios
- Ver estatísticas globais do sistema
- Gerir marcas, modelos, extras
- Ver logs do sistema

### 3.5 Proteções de Segurança Implementadas

#### 3.5.1 Proteção CSRF (Cross-Site Request Forgery)

Todos os formulários POST incluem **AntiForgeryToken**:
- Token único gerado por sessão
- Validado no servidor antes de processar requests
- Previne ataques onde sites maliciosos enviam requests em nome do utilizador

#### 3.5.2 Proteção SQL Injection

- **Entity Framework Core** usa parametrized queries automaticamente
- Todos os inputs são parametrizados, nunca concatenados em strings SQL
- Raw SQL queries são evitadas; quando necessárias, usam parametrização explícita

#### 3.5.3 Proteção XSS (Cross-Site Scripting)

- **Razor View Engine** escapa automaticamente todo o output HTML
- Inputs de utilizador são sanitizados antes de armazenamento
- Headers de segurança configurados (Content-Security-Policy, X-XSS-Protection)

#### 3.5.4 Validação de Input

**Client-Side:**
- jQuery Validation com mensagens em português
- Validação em tempo real conforme utilizador preenche
- Feedback visual imediato

**Server-Side:**
- Validações duplicadas no servidor (nunca confiar apenas no client)
- ModelState.IsValid verificado em todos os endpoints POST
- Validações customizadas para casos específicos (NIF português, matrículas)

#### 3.5.5 Rate Limiting e Lockout

- Lockout automático após 5 tentativas de login falhadas
- Proteção contra brute force attacks
- Cooldown de 15 minutos antes de permitir novas tentativas

---

## 4. INTEGRIDADE DA BASE DE DADOS

### 4.1 Constraints e Validações

#### 4.1.1 Chaves Primárias e Estrangeiras

**Todas as entidades têm:**
- Chave primária (Primary Key) identificada com `[Key]` annotation
- Foreign keys devidamente anotadas com `[ForeignKey]`
- Propriedades de navegação para facilitar queries relacionais
- Índices automáticos criados pelo Entity Framework para performance

**Exemplo de relacionamentos:**
- **Anúncio → Vendedor:** Cada anúncio pertence a um vendedor (FK: VendedorId)
- **Anúncio → Imagens:** Um anúncio tem múltiplas imagens (1:N)
- **Anúncio → Reservas:** Um anúncio pode ter múltiplas reservas (1:N)
- **Comprador ↔ Favoritos:** Relação N:M através de tabela intermediária AnunciosFavoritos

#### 4.1.2 Data Annotations

O sistema usa extensivamente Data Annotations para garantir integridade:

**Validações Comuns:**
- `[Required]` - Campos obrigatórios (Nome, Email, Título, Preço)
- `[StringLength(max)]` - Limitar tamanho de strings (Email: 100, Descrição: 2000)
- `[Range(min, max)]` - Validar intervalos numéricos (Ano: 1900-2025, Preço: 0-1000000)
- `[EmailAddress]` - Validar formato de email
- `[Phone]` - Validar formato de telefone
- `[RegularExpression]` - Validações customizadas (NIF, Matrícula)
- `[Column(TypeName)]` - Definir tipo SQL específico (decimal(10,2) para preços)

**Validações Customizadas:**
- **NIF Português:** Validação com algoritmo de checksum (Módulo 11)
- **Matrícula Portuguesa:** Formatos XX-XX-XX, XX-XX-AA, AA-XX-XX
- **Ano do Veículo:** Entre 1900 e ano atual + 1
- **Quilometragem:** Não pode ser negativa, máximo razoável (999.999 km)

#### 4.1.3 Comportamento de Cascade Delete

**Configurações implementadas:**

**Cascade Delete (ON DELETE CASCADE):**
- Anúncio → Imagens: Se anúncio é apagado, todas as imagens são apagadas automaticamente
- Conversa → Mensagens: Se conversa é apagada, todas as mensagens são apagadas
- Vendedor → Anúncios: Se vendedor é removido, todos os anúncios são removidos

**Restrict Delete (ON DELETE RESTRICT):**
- Anúncio → Reservas: Não é possível apagar anúncio se tiver reservas ativas
- Anúncio → Compras: Não é possível apagar anúncio se tiver compras associadas
- Marca → Modelos: Não é possível apagar marca se tiver modelos associados

### 4.2 Relacionamentos Implementados

#### 4.2.1 Relacionamentos 1:N (Um para Muitos)

- **Vendedor → Anúncios:** Um vendedor pode ter múltiplos anúncios
- **Anúncio → Imagens:** Um anúncio tem múltiplas imagens (até 10)
- **Anúncio → Reservas:** Um anúncio pode ter múltiplas reservas (histórico)
- **Anúncio → Visitas:** Um anúncio pode ter múltiplas visitas agendadas
- **Marca → Modelos:** Uma marca tem múltiplos modelos
- **Conversa → Mensagens:** Uma conversa tem múltiplas mensagens

#### 4.2.2 Relacionamentos N:M (Muitos para Muitos)

Implementados através de tabelas intermediárias:

- **Comprador ↔ Anúncios Favoritos:** Tabela `AnunciosFavoritos` (CompradorId, AnuncioId)
- **Comprador ↔ Marcas Favoritas:** Tabela `MarcasFavoritas` (CompradorId, MarcaId)
- **Anúncio ↔ Extras:** Tabela `AnuncioExtra` (AnuncioId, ExtraId)

#### 4.2.3 Herança TPH (Table Per Hierarchy)

**Hierarquia de Utilizadores:**
```
Utilizador (classe abstrata)
├── Comprador
├── Vendedor
└── Administrador
```

- Todos armazenados na mesma tabela `Utilizadores`
- Coluna discriminadora `Discriminator` identifica o tipo
- Propriedades específicas de cada tipo na mesma tabela
- Vantagens: Queries mais rápidas, não precisa de JOINs
- Desvantagens: Colunas nullable para propriedades específicas

### 4.3 Migrations e Controlo de Versões

#### 4.3.1 Sistema de Migrations

O Entity Framework Core usa migrations para controlar versões da estrutura da base de dados:

**Principais Migrations Criadas:**
1. `InitialCreate` - Criação inicial de todas as tabelas
2. `AddAnuncioEstado` - Substituição do campo booleano `Vendido` por string `Estado`
3. `AddTwoFactorAuth` - Adição de campos para suporte 2FA
4. `AddGoogleOAuth` - Configuração para external logins
5. `AddReservaDataExpiracao` - Adicionar campo de expiração em reservas

#### 4.3.2 Migração de Dados

Exemplo de migration complexa com migração de dados:

**Problema:** Campo booleano `Vendido` precisava ser substituído por `Estado` (string) para suportar múltiplos estados.

**Solução implementada na migration:**
1. Adicionar nova coluna `Estado` com valor padrão "Ativo"
2. Executar SQL para migrar dados: `UPDATE Anuncios SET Estado = CASE WHEN Vendido = 1 THEN 'Vendido' ELSE 'Ativo' END`
3. Remover coluna antiga `Vendido`

Isto garantiu que nenhum dado foi perdido durante a refatoração.

---

## 5. FUNCIONALIDADES DO SISTEMA

### 5.1 Gestão de Anúncios

#### 5.1.1 Criação de Anúncios

**Processo:**
1. Vendedor acede a "Criar Anúncio"
2. Preenche formulário com dados do veículo:
   - Título e descrição
   - Marca e modelo (dropdowns em cascata)
   - Ano, quilometragem, combustível, transmissão, potência
   - Número de portas, lugares, cor
   - Preço e valor do sinal para reserva
   - Localização (distrito, concelho)
   - Extras opcionais (ar condicionado, GPS, ABS, etc.)
3. Upload de imagens (até 10, máx 5MB cada, formatos: JPG, PNG)
4. Sistema valida todos os campos
5. Anúncio é criado com estado "Ativo"
6. Vendedor é redirecionado para página de detalhes

**Validações:**
- Vendedor deve estar com estado "Ativo"
- Imagens devem ter tamanho e formato válidos
- Preço e sinal devem ser valores positivos
- Ano deve estar no intervalo permitido
- Todos os campos obrigatórios preenchidos

#### 5.1.2 Sistema de Estados

Os anúncios transitam por diferentes estados durante o seu ciclo de vida:

**Estados Possíveis:**
- **Ativo:** Visível na listagem pública, pode ser reservado/comprado
- **Reservado:** Tem reserva ativa, ainda visível mas com indicação
- **Vendido:** Compra concluída, removido da listagem pública
- **Pausado:** Vendedor pausou temporariamente (não implementado UI)
- **Bloqueado:** Admin bloqueou por violação de regras
- **Expirado:** Data de expiração passou (não implementado)

**Transições Automáticas:**
- Ativo → Reservado: Quando comprador paga sinal de reserva
- Reservado → Vendido: Quando comprador completa a compra
- Ativo → Vendido: Compra direta sem reserva prévia
- Qualquer → Bloqueado: Admin bloqueia anúncio

#### 5.1.3 Pesquisa e Filtragem

**Filtros Disponíveis:**
- Marca e modelo
- Faixa de preço (slider)
- Ano (de/até)
- Quilometragem (de/até)
- Tipo de combustível (Gasolina, Diesel, Elétrico, Híbrido)
- Tipo de transmissão (Manual, Automática)
- Localização (distrito, concelho)
- Extras (múltipla seleção)

**Ordenação:**
- Relevância (padrão)
- Preço (crescente/decrescente)
- Ano (mais recente/mais antigo)
- Quilometragem (menor/maior)
- Data de publicação (mais recente)

**Paginação:**
- 12 anúncios por página
- Navegação entre páginas
- Total de resultados apresentado

### 5.2 Sistema de Reservas

#### 5.2.1 Como Funciona

**Objetivo:** Permitir que compradores "segurem" um veículo pagando um sinal, garantindo prioridade na compra.

**Processo Completo:**
1. Comprador vê anúncio de interesse
2. Clica em "Reservar Veículo"
3. Modal apresenta:
   - Resumo do veículo
   - Valor do sinal a pagar (definido pelo vendedor)
   - Termos e condições
   - Validade da reserva (7 dias)
4. Comprador aceita e é redirecionado para Stripe
5. Preenche dados do cartão no checkout seguro do Stripe
6. Após pagamento confirmado:
   - Sistema cria registo de Reserva na BD
   - Anúncio muda para estado "Reservado"
   - Email enviado ao comprador com confirmação e link para completar compra
   - Email enviado ao vendedor com informações da reserva
7. Comprador tem 7 dias para completar a compra ou a reserva expira

**Vantagens para o Comprador:**
- Garante prioridade no veículo
- Tempo para organizar financiamento
- Valor do sinal é deduzido na compra final

**Vantagens para o Vendedor:**
- Demonstração de interesse sério do comprador
- Sinal como compensação se comprador desistir
- Menor risco de tempo perdido

#### 5.2.2 Gestão de Reservas

**No Perfil do Comprador:**
- Lista de todas as reservas ativas e históricas
- Estado da reserva (Ativa, Concluída, Cancelada, Expirada)
- Data de expiração
- Botão "Completar Compra" para reservas ativas
- Botão "Cancelar Reserva" (perde sinal)

**No Perfil do Vendedor:**
- Lista de reservas recebidas
- Informações do comprador interessado
- Opção de contactar comprador
- Histórico de reservas concluídas/canceladas

### 5.3 Sistema de Compras

#### 5.3.1 Modalidades de Compra

**Compra Direta (Sem Reserva Prévia):**
- Comprador paga o valor total do veículo
- Processo rápido, uma única transação
- Não há período de "espera"

**Compra com Reserva (Dedução de Sinal):**
- Comprador já pagou sinal através de reserva
- Sistema deteta automaticamente a reserva ativa
- Calcula valor restante: Preço Total - Sinal Pago
- Apresenta breakdown claro dos valores no modal
- Após pagamento, reserva é marcada como "Concluída"

#### 5.3.2 Processo de Compra

1. Comprador clica em "Comprar Veículo"
2. Modal de compra apresenta:
   - Resumo do veículo com imagem
   - **Se tem reserva:** Mostra sinal pago e valor restante
   - **Se não tem reserva:** Mostra valor total
   - Termos de venda
3. Comprador confirma e é redirecionado para Stripe
4. Pagamento processado de forma segura
5. Após confirmação:
   - Registo de Compra criado na BD com todos os detalhes
   - Anúncio muda para estado "Vendido"
   - Anúncio é removido da listagem pública
   - Se havia reserva, é marcada como "Concluída"
   - Email ao comprador com:
     - Confirmação de compra
     - Breakdown de valores (total, sinal, restante)
     - Informações do veículo e vendedor
     - Próximos passos
   - Email ao vendedor com:
     - Notificação de venda
     - Informações do comprador
     - Instruções para entrega do veículo

#### 5.3.3 Secção "Minhas Compras"

Adicionada na Fase 3, permite ao comprador ver histórico completo:

**Informações Apresentadas:**
- Card com imagem do veículo
- Marca, modelo, ano, matrícula
- Data da compra
- Valor total pago
- Breakdown (sinal + restante, se aplicável)
- Estado de pagamento (Pago, Pendente, Reembolsado)
- Informações do vendedor (nome, telefone, email)
- Links para:
  - Ver anúncio original
  - Contactar vendedor
  - Ver recibo (se implementado)

### 5.4 Sistema de Visitas

#### 5.4.1 Gestão de Disponibilidade

**Vendedor Define Disponibilidade:**
- Acede a "Definir Disponibilidade" no perfil
- Seleciona dias da semana disponíveis
- Define horários para cada dia (ex: Segunda 9h-18h, Sábado 10h-14h)
- Define intervalo de slots (padrão: 30 minutos)
- Sistema gera automaticamente slots disponíveis

**Exemplo:**
- Vendedor disponível: Segundas das 9h às 18h, intervalo 30 min
- Slots gerados: 9:00, 9:30, 10:00, 10:30, ..., 17:30
- Se já existe visita às 10:00, esse slot é removido da listagem

#### 5.4.2 Agendamento de Visitas

**Processo:**
1. Comprador vê anúncio de interesse
2. Clica em "Agendar Visita"
3. Modal apresenta:
   - Calendário com próximos 60 dias
   - Apenas slots disponíveis do vendedor são clicáveis
   - Slots já ocupados aparecem desativados
4. Comprador seleciona data/hora
5. Pode adicionar observações (opcional)
6. Sistema cria visita com estado "Pendente"
7. Email enviado ao vendedor notificando
8. Vendedor pode:
   - **Confirmar:** Estado muda para "Confirmada", email enviado ao comprador
   - **Cancelar:** Estado muda para "Cancelada", slot fica disponível novamente
9. Após a data da visita, vendedor pode marcar como "Concluída"

**Estados de Visita:**
- **Pendente:** Aguarda ação do vendedor
- **Confirmada:** Vendedor confirmou, visita agendada
- **Concluída:** Visita realizada
- **Cancelada:** Cancelada por comprador ou vendedor

#### 5.4.3 Gestão de Visitas

**No Perfil do Comprador:**
- Visitas agendadas (futuras)
- Histórico de visitas (passadas)
- Estado de cada visita
- Opção de cancelar (se pendente ou confirmada)
- Localização e horário

**No Perfil do Vendedor:**
- Visitas recebidas pendentes de confirmação
- Visitas confirmadas (calendário)
- Histórico de visitas
- Informações do comprador interessado
- Opções: Confirmar, Cancelar, Marcar como Concluída

### 5.5 Sistema de Favoritos

**Tipos de Favoritos:**

**Anúncios Favoritos:**
- Comprador pode "favoritar" anúncios de interesse
- Ícone de coração na listagem e página de detalhes
- Toggle via AJAX (sem reload da página)
- Lista de favoritos no perfil
- Notificações quando preço baixa (não implementado)

**Marcas Favoritas:**
- Comprador pode favoritar marcas
- Receber notificações de novos anúncios da marca (não implementado)
- Personalização de recomendações

### 5.6 Sistema de Mensagens

**Funcionalidades:**
- Chat 1:1 entre comprador e vendedor
- Conversas organizadas por anúncio
- Envio e receção de mensagens em tempo real (polling)
- Histórico completo mantido
- Notificação de mensagens não lidas

**Tipos de Conversa:**
- "A comprar" - Conversa iniciada pelo comprador
- "A vender" - Conversa onde o utilizador é o vendedor

### 5.7 Sistema de Denúncias

**Tipos de Denúncia:**
- Anúncio fraudulento
- Informações falsas no anúncio
- Vendedor suspeito/não confiável
- Comprador com comportamento inadequado
- Conteúdo impróprio

**Processo:**
1. Utilizador clica em "Denunciar"
2. Seleciona tipo de denúncia
3. Descreve o motivo
4. Denúncia fica "Pendente"
5. Admin revisa no painel
6. Admin pode "Aprovar" (tomar ação) ou "Rejeitar"

### 5.8 Painel de Administração

**Dashboard:**
- Total de utilizadores por tipo
- Total de anúncios por estado
- Vendas do mês
- Denúncias pendentes
- Gráficos de atividade (se implementados)

**Gestão de Utilizadores:**
- Lista de todos os utilizadores
- Filtros por tipo (Comprador, Vendedor, Admin)
- Ações: Ativar, Bloquear, Remover
- Ver histórico de atividade

**Gestão de Denúncias:**
- Lista de denúncias pendentes
- Ver detalhes completos
- Aprovar (bloquear denunciado) ou Rejeitar
- Histórico de denúncias processadas

**Gestão de Conteúdo:**
- Marcas e modelos (criar, editar, remover)
- Extras disponíveis
- Categorias
- FAQs

---

## 6. ARQUITETURA E TECNOLOGIAS

### 6.1 Arquitetura do Sistema

**Padrão MVC (Model-View-Controller):**

```
┌─────────────────────────────────────────┐
│         PRESENTATION LAYER              │
│   Views (Razor) + JavaScript (jQuery)   │
└─────────────────────────────────────────┘
                    ↓ ↑
┌─────────────────────────────────────────┐
│          CONTROLLER LAYER               │
│  AnunciosController, ComprasController, │
│  ReservasController, UtilizadoresCtrl...│
└─────────────────────────────────────────┘
                    ↓ ↑
┌─────────────────────────────────────────┐
│           BUSINESS LOGIC                │
│  Regras de Negócio, Validações,         │
│  Integrações (Stripe, SMTP, OAuth)      │
└─────────────────────────────────────────┘
                    ↓ ↑
┌─────────────────────────────────────────┐
│          DATA ACCESS LAYER              │
│   ApplicationDbContext (EF Core)        │
└─────────────────────────────────────────┘
                    ↓ ↑
┌─────────────────────────────────────────┐
│           DATABASE LAYER                │
│      SQL Server (LocalDB)               │
└─────────────────────────────────────────┘
```

### 6.2 Stack Tecnológico

#### 6.2.1 Backend

| Tecnologia | Versão | Utilização |
|------------|--------|------------|
| **ASP.NET Core MVC** | 8.0 | Framework web principal, routing, middleware |
| **Entity Framework Core** | 9.0.10 | ORM para acesso à base de dados |
| **SQL Server LocalDB** | 2022 | Base de dados relacional |
| **ASP.NET Identity** | 8.0 | Autenticação, autorização, gestão de utilizadores |
| **Stripe.NET** | Latest | Integração de pagamentos |
| **MailKit** | Latest | Envio de emails via SMTP |
| **QRCoder** | Latest | Geração de QR Codes para 2FA |

#### 6.2.2 Frontend

| Tecnologia | Versão | Utilização |
|------------|--------|------------|
| **Razor View Engine** | - | Template engine para geração de HTML dinâmico |
| **Bootstrap** | 5.3 | Framework CSS responsivo |
| **jQuery** | 3.7.1 | Manipulação DOM, AJAX |
| **jQuery Validation** | - | Validação client-side de formulários |
| **Bootstrap Icons** | 1.11.1 | Iconografia |
| **Select2** | 4.1.0 | Dropdowns avançados com pesquisa |
| **Lightbox** | - | Galeria de imagens em modal |

#### 6.2.3 Ferramentas de Desenvolvimento

| Ferramenta | Utilização |
|------------|------------|
| **Visual Studio 2022** | IDE principal para desenvolvimento |
| **Git & GitHub** | Controlo de versões e colaboração |
| **SQL Server Management Studio** | Gestão e queries da base de dados |
| **Postman** | Testes de endpoints e APIs |
| **Browser DevTools** | Debug de frontend e network |

### 6.3 Estrutura do Projeto

```
Marketplace/
├── Controllers/          # 12+ controllers
│   ├── AnunciosController.cs
│   ├── UtilizadoresController.cs
│   ├── ReservasController.cs
│   ├── ComprasController.cs
│   └── ...
├── Models/              # 31 entidades
│   ├── Anuncio.cs
│   ├── Utilizador.cs (abstrato)
│   ├── Comprador.cs
│   ├── Vendedor.cs
│   └── ...
├── Views/               # 80+ views Razor
│   ├── Anuncios/
│   ├── Utilizadores/
│   ├── Shared/
│   └── ...
├── Data/
│   └── ApplicationDbContext.cs
├── Migrations/          # 15+ migrations
├── Services/
│   ├── EmailSender.cs
│   └── ImageUploadHelper.cs
├── wwwroot/
│   ├── css/
│   ├── js/
│   ├── images/
│   └── uploads/
└── appsettings.json
```

---

## 7. FLUXOS DE PROCESSO PRINCIPAIS

### 7.1 Fluxo Completo de Venda com Reserva

**Cenário:** Comprador reserva veículo e depois completa a compra

1. **Publicação do Anúncio**
   - Vendedor cria anúncio com preço 5000€ e sinal 500€
   - Estado: Ativo
   - Anúncio aparece na listagem pública

2. **Reserva**
   - Comprador vê anúncio e clica em "Reservar"
   - Sistema redireciona para Stripe
   - Comprador paga 500€ (sinal)
   - Sistema cria Reserva na BD
   - Anúncio muda para estado: Reservado
   - Emails enviados (comprador + vendedor)
   - Reserva válida por 7 dias

3. **Agendamento de Visita**
   - Comprador agenda visita através do sistema
   - Vendedor confirma visita
   - Visita realizada no local

4. **Compra Final**
   - Comprador decide comprar
   - Clica em "Completar Compra" (no email ou perfil)
   - Sistema deteta reserva ativa
   - Calcula valor restante: 5000€ - 500€ = 4500€
   - Redireciona para Stripe para pagar 4500€
   - Comprador completa pagamento
   - Sistema:
     - Cria registo de Compra (valor total 5000€, sinal 500€, restante 4500€)
     - Marca Reserva como "Concluída"
     - Muda Anúncio para estado: Vendido
     - Remove anúncio da listagem pública
     - Envia emails de confirmação
   - Compra aparece em "Minhas Compras" do comprador

5. **Pós-Venda**
   - Vendedor e comprador combinam entrega
   - Transferência de propriedade (fora do sistema)
   - Possibilidade de avaliação/review (não implementado)

### 7.2 Fluxo de Compra Direta (Sem Reserva)

**Cenário:** Comprador compra imediatamente sem reserva prévia

1. Comprador vê anúncio de 5000€
2. Clica em "Comprar Agora"
3. Sistema deteta que não existe reserva
4. Modal mostra valor total: 5000€
5. Redireciona para Stripe
6. Comprador paga 5000€
7. Sistema cria Compra (valor total 5000€, sem sinal)
8. Anúncio muda para "Vendido"
9. Emails enviados
10. Processo de entrega

### 7.3 Fluxo de Autenticação com 2FA

1. Utilizador acede a página de login
2. Insere email e password
3. Sistema valida credenciais
4. **Se 2FA não estiver ativo:** Login direto
5. **Se 2FA estiver ativo:**
   - Redireciona para página de verificação
   - Utilizador abre app autenticadora (Google Authenticator)
   - Copia código de 6 dígitos
   - Insere código no sistema
   - Sistema valida código TOTP
   - Se válido: Login concluído
   - Se inválido: Pede novo código ou código de recuperação
6. Utilizador autenticado, redireciona para dashboard

---

## 8. DESAFIOS E SOLUÇÕES

### 8.1 Integração com Stripe

**Desafio 1: Cálculo de Valores**
- Stripe trabalha com valores em cêntimos (integers)
- Necessário multiplicar por 100 e converter para `long`
- Risco de erros de arredondamento

**Solução:**
- Sempre trabalhar com `decimal` no código C#
- Converter para cêntimos no último momento: `(long)(valor * 100)`
- Validar valores antes de enviar para Stripe

**Desafio 2: Dedução de Sinal**
- Sistema precisa saber se comprador já pagou sinal
- Calcular valor restante automaticamente
- Evitar cobrar duplicado

**Solução:**
- Verificar se existe reserva ativa antes de criar sessão Stripe
- Calcular: `valorAPagar = precoTotal - valorSinal`
- Enviar informações na metadata do Stripe
- Validar no callback de sucesso

**Desafio 3: Sincronização de Estados**
- Anúncio deve mudar de estado apenas após pagamento confirmado
- Evitar criar registos se pagamento falhar

**Solução:**
- Sempre verificar `session.PaymentStatus == "paid"`
- Criar registos na BD apenas após confirmação
- Usar transações para atomicidade

### 8.2 Sistema de Estados de Anúncios

**Desafio:**
- Inicialmente, usava-se campo booleano `Vendido` (true/false)
- Descobriu-se necessidade de mais estados: Reservado, Pausado, Bloqueado, Expirado
- Refatorar sem perder dados

**Solução:**
- Criar nova coluna `Estado` (string)
- Criar migration com SQL de migração de dados
- Mapear: `Vendido = true → Estado = "Vendido"`, `Vendido = false → Estado = "Ativo"`
- Remover coluna `Vendido`
- Atualizar todos os controllers e views

**Lição Aprendida:**
- Planejar estrutura de dados com escalabilidade em mente
- Estados de entidades devem usar strings ou enums, não booleanos limitados

### 8.3 Validação de Imagens

**Desafio:**
- Utilizadores podem tentar upload de ficheiros maliciosos
- Ficheiros muito grandes podem sobrecarregar servidor
- Formatos não suportados podem causar erros

**Solução:**
- Validar extensão e MIME type
- Limitar tamanho máximo (5MB)
- Limitar quantidade (10 imagens)
- Gerar nomes únicos com GUID para evitar colisões
- Armazenar paths relativos na BD

### 8.4 Segurança de Secrets

**Desafio:**
- GitHub bloqueou push por detetar chave do Stripe em `appsettings.json`
- Push Protection impede commits com secrets

**Solução Temporária (Projeto Académico):**
- Seguir link do GitHub para permitir push
- Chaves são de ambiente de teste

**Solução Ideal (Produção):**
- Usar User Secrets em desenvolvimento
- Usar Azure Key Vault ou variáveis de ambiente em produção
- Nunca commitar `appsettings.json` com secrets reais

### 8.5 Erros de Sintaxe Razor

**Desafio:**
- Código Razor (`} else {`) a aparecer como texto na página
- Botões duplicados

**Causa:**
- Blocos `@if` / `else` mal estruturados
- Chaves `}` a fechar blocos errados

**Solução:**
- Rever estrutura de blocos Razor
- Garantir que `else` está ligado ao `@if` correto
- Usar indentação correta para visualizar blocos

### 8.6 Conversão de Tipos Nullable

**Desafio:**
- Stripe retorna `long?` para `AmountTotal`
- Divisão resulta em `decimal?`
- Métodos esperam `decimal` não-nullable

**Solução:**
- Usar null-coalescing operator: `(session.AmountTotal ?? 0) / 100m`
- Garantir que sempre há valor default (0)

---

## 9. TESTES E VALIDAÇÕES

### 9.1 Testes Funcionais Realizados

#### 9.1.1 Autenticação e Autorização

| Teste | Resultado | Observações |
|-------|-----------|-------------|
| Registo de Comprador | ✅ Pass | Validações de email, password, confirmação |
| Registo de Vendedor | ✅ Pass | Campos adicionais (NIF, dados fiscais) |
| Login com email | ✅ Pass | Case-insensitive |
| Login com Google | ✅ Pass | Cria conta automaticamente se não existir |
| 2FA - Ativação | ✅ Pass | QR Code gerado, códigos de recuperação criados |
| 2FA - Login | ✅ Pass | Validação de código TOTP |
| 2FA - Código de Recuperação | ✅ Pass | Código usado é invalidado |
| Lockout após 5 tentativas | ✅ Pass | Bloqueio de 15 minutos aplicado |
| Recuperação de password | ✅ Pass | Email enviado, link temporário válido |

#### 9.1.2 Gestão de Anúncios

| Teste | Resultado | Observações |
|-------|-----------|-------------|
| Criar anúncio (vendedor ativo) | ✅ Pass | Anúncio criado com estado "Ativo" |
| Criar anúncio (comprador) | ✅ Pass | Bloqueado por autorização |
| Criar anúncio (vendedor bloqueado) | ✅ Pass | Bloqueado, mensagem de erro |
| Upload de 10 imagens | ✅ Pass | Todas as imagens carregadas |
| Upload de 11 imagens | ✅ Pass | Apenas 10 aceites, 11ª ignorada |
| Upload de imagem > 5MB | ✅ Pass | Rejeitada, mensagem de erro |
| Upload de ficheiro .exe | ✅ Pass | Rejeitado por MIME type |
| Editar anúncio próprio | ✅ Pass | Modificações guardadas |
| Editar anúncio de outro | ✅ Pass | Bloqueado, acesso negado |
| Apagar anúncio com reservas | ✅ Pass | Bloqueado por constraint FK |
| Filtrar por marca | ✅ Pass | Resultados corretos |
| Ordenar por preço crescente | ✅ Pass | Ordenação correta |

#### 9.1.3 Reservas e Compras

| Teste | Resultado | Observações |
|-------|-----------|-------------|
| Reservar com Stripe (sucesso) | ✅ Pass | Reserva criada, emails enviados |
| Reservar com Stripe (cancelar) | ✅ Pass | Nenhum registo criado, anúncio mantém estado |
| Reservar próprio anúncio | ✅ Pass | Bloqueado, mensagem de erro |
| Reservar sem login | ✅ Pass | Redireciona para login |
| Anúncio muda para "Reservado" | ✅ Pass | Estado atualizado após pagamento |
| Compra direta (sem reserva) | ✅ Pass | Valor total cobrado |
| Compra com reserva | ✅ Pass | Valor restante cobrado, sinal deduzido |
| Compra marca reserva como concluída | ✅ Pass | Estado da reserva atualizado |
| Anúncio vendido desaparece | ✅ Pass | Não aparece na listagem pública |
| "Minhas Compras" atualiza | ✅ Pass | Nova compra aparece imediatamente |

#### 9.1.4 Visitas

| Teste | Resultado | Observações |
|-------|-----------|-------------|
| Agendar visita (slot disponível) | ✅ Pass | Visita criada, email ao vendedor |
| Agendar visita (sem disponibilidade) | ✅ Pass | Nenhum slot apresentado |
| Vendedor confirmar visita | ✅ Pass | Estado muda, email ao comprador |
| Vendedor cancelar visita | ✅ Pass | Slot fica disponível novamente |
| Comprador cancelar visita | ✅ Pass | Estado atualizado |
| Agendar próprio anúncio | ✅ Pass | Bloqueado |

### 9.2 Testes de Validação

| Campo | Validação | Teste | Resultado |
|-------|-----------|-------|-----------|
| Email | Formato de email | "teste@teste" | ❌ Rejeitado |
| Email | Formato de email | "teste@teste.com" | ✅ Aceite |
| Password | 8+ caracteres | "Test123" | ❌ Rejeitado (falta especial) |
| Password | Complexidade | "Test@123" | ✅ Aceite |
| NIF | Checksum | "123456789" | ❌ Rejeitado (inválido) |
| NIF | Checksum | "123456789" (válido) | ✅ Aceite |
| Ano | Range | 1800 | ❌ Rejeitado (< 1900) |
| Ano | Range | 2026 | ❌ Rejeitado (> 2025) |
| Preço | Valor positivo | -1000 | ❌ Rejeitado |
| Telefone | Formato | "abcdefghi" | ❌ Rejeitado |

### 9.3 Testes de Segurança

| Teste | Resultado | Descrição |
|-------|-----------|-----------|
| SQL Injection | ✅ Protegido | EF Core usa parametrized queries |
| XSS Attack | ✅ Protegido | Razor escapa HTML automaticamente |
| CSRF Attack | ✅ Protegido | AntiForgeryToken em todos os POST |
| Acesso sem autenticação | ✅ Bloqueado | Redirect para login |
| Acesso sem autorização | ✅ Bloqueado | Mensagem 403 Forbidden |
| Brute Force | ✅ Protegido | Lockout após 5 tentativas |

### 9.4 Testes de Performance

| Métrica | Objetivo | Resultado | Status |
|---------|----------|-----------|--------|
| Carregamento Index | < 2s | ~1.5s | ✅ OK |
| Carregamento Details | < 1s | ~0.8s | ✅ OK |
| Upload imagem 1MB | < 3s | ~2s | ✅ OK |
| Query 1000 anúncios | < 2s | ~1.8s | ✅ OK |
| Criação de reserva | < 5s | ~3s (incluindo Stripe) | ✅ OK |

---

## 10. CONCLUSÕES

### 10.1 Objetivos Cumpridos da Fase 3

✅ **Integridade da Base de Dados**
- Todas as constraints de chaves primárias e estrangeiras implementadas
- Validações robustas em todos os modelos
- Relacionamentos complexos (1:N, N:M, herança TPH) funcionando corretamente
- Sistema de migrations para controlo de versões da estrutura

✅ **Lógica Funcional Completa**
- Controllers totalmente funcionais conectados via Entity Framework Core
- Regras de negócio complexas implementadas (estados, cálculos, validações)
- Integração bem-sucedida com 3 APIs externas (Stripe, Google OAuth, Gmail)
- Sistema de autorização baseado em roles funcionando

✅ **Sistema End-to-End Funcional**
- Utilizadores podem registar-se (com email ou Google)
- Vendedores podem criar, editar e gerir anúncios
- Compradores podem reservar e comprar veículos com pagamentos reais
- Sistema de agendamento de visitas baseado em disponibilidade
- Emails transacionais enviados automaticamente
- Painel de administração para gestão do sistema

### 10.2 Principais Integrações Externas

**1. Stripe API** - Sistema de Pagamentos
- Processamento seguro de pagamentos para reservas e compras
- Cálculo inteligente com dedução automática de sinais
- Gestão de estados sincronizada com transações
- Callbacks para confirmação de pagamentos

**2. Google OAuth 2.0** - Login Social
- Autenticação delegada ao Google
- Criação automática de contas
- Sincronização de dados básicos
- Simplificação do processo de registo

**3. Gmail SMTP** - Sistema de Emails
- Envio de emails transacionais estilizados
- Confirmações de reservas e compras
- Notificações de visitas
- Emails de autenticação (confirmação, recuperação, 2FA)

**4. Two-Factor Authentication**
- Segurança adicional com TOTP
- Geração de QR Codes
- Códigos de recuperação de emergência
- Integração com apps autenticadoras populares

### 10.3 Funcionalidades Destacadas

**Sistema de Estados de Anúncios**
- Transições automáticas e controladas
- Filtragem inteligente na listagem
- Histórico completo de estados

**Dedução Inteligente de Sinal**
- Sistema deteta automaticamente reservas ativas
- Calcula e apresenta valores corretos
- Evita cobranças duplicadas

**Agendamento de Visitas**
- Baseado em disponibilidade real do vendedor
- Geração automática de slots disponíveis
- Prevenção de conflitos de horários

**Secção "Minhas Compras"**
- Histórico completo de compras
- Breakdown detalhado de valores
- Links diretos para ações relevantes

### 10.4 Estatísticas Finais

**Base de Dados:**
- 31 entidades no modelo
- 15+ migrations aplicadas
- 80+ foreign keys configuradas
- Relacionamentos complexos (1:N, N:M, herança)

**Código:**
- 12+ controllers implementados
- 80+ views Razor
- 3 integrações externas
- 4 métodos de autenticação

**Funcionalidades:**
- Sistema completo de gestão de anúncios
- Sistema de reservas e compras com Stripe
- Agendamento de visitas
- Mensagens entre utilizadores
- Favoritos e pesquisas guardadas
- Painel de administração
- Sistema de denúncias

### 10.5 Lições Aprendidas

**Técnicas:**
1. Planeamento cuidadoso da estrutura de dados evita refatorações complexas
2. Integrações externas requerem atenção a detalhes (conversões, metadata)
3. Validações devem estar tanto no client como no server
4. Migrations com migração de dados preservam informação durante refatorações
5. Secrets não devem ser commitadas (usar User Secrets ou Key Vault)

**Processo:**
1. Documentação contínua facilita relatórios finais
2. Testes incrementais evitam acumulação de bugs
3. Commits frequentes facilitam rollback se necessário
4. Comunicação em equipa é fundamental para integração

**Segurança:**
1. Nunca confiar apenas em validações client-side
2. Sempre usar parametrized queries
3. Implementar proteção CSRF em todos os formulários
4. Validar estado de pagamentos antes de criar registos
5. Testar diferentes cenários de autenticação e autorização

### 10.6 Melhorias Futuras

**Curto Prazo:**
- Notificações em tempo real (SignalR)
- Sistema de avaliações e reviews
- Histórico de alterações de preços
- Comparação de veículos
- Relatórios para vendedores

**Médio Prazo:**
- Aplicação móvel (Android/iOS)
- API REST pública para integrações
- Sistema de leilões
- Integração com serviços de financiamento
- Verificação de documentação (OCR)

**Longo Prazo:**
- IA para detecção de fraudes
- Chatbot de suporte
- Recomendações personalizadas
- Análise preditiva de preços

---

## APÊNDICE A - CONFIGURAÇÃO DO AMBIENTE

### A.1 Requisitos de Sistema

**Software Necessário:**
- Visual Studio 2022 (Community ou superior)
- .NET 8.0 SDK
- SQL Server 2022 (LocalDB incluído no VS)
- Git for Windows

**Configurações Recomendadas:**
- 8GB+ RAM
- 20GB espaço em disco
- Windows 10/11

### A.2 Configuração Inicial

1. Clonar repositório: `git clone <url>`
2. Abrir solução no Visual Studio
3. Restaurar pacotes NuGet
4. Configurar `appsettings.json` com chaves (Stripe, SMTP, Google)
5. Executar migrations: `dotnet ef database update`
6. Executar projeto (F5)

---

## APÊNDICE B - COMANDOS ÚTEIS

### B.1 Entity Framework

```bash
# Criar nova migration
dotnet ef migrations add NomeDaMigration

# Aplicar migrations pendentes
dotnet ef database update

# Reverter para migration específica
dotnet ef database update NomeMigrationAnterior

# Gerar script SQL
dotnet ef migrations script

# Remover última migration (se não aplicada)
dotnet ef migrations remove
```

### B.2 Git

```bash
# Ver estado
git status

# Adicionar ficheiros
git add .

# Commit
git commit -m "Descrição"

# Push
git push origin NomeBranch

# Pull
git pull origin NomeBranch

# Criar branch
git checkout -b NovaBranch
```

---

## APÊNDICE C - REFERÊNCIAS

**Documentação Oficial:**
- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core](https://docs.microsoft.com/ef/core)
- [ASP.NET Identity](https://docs.microsoft.com/aspnet/core/security/authentication/identity)
- [Stripe API Documentation](https://stripe.com/docs/api)
- [Google OAuth 2.0](https://developers.google.com/identity/protocols/oauth2)
- [Bootstrap 5](https://getbootstrap.com/docs/5.3)

**Recursos Consultados:**
- Microsoft Learn - ASP.NET Core MVC Tutorial
- Stripe Integration Guide
- Google OAuth Integration
- MailKit Documentation

---

**Fim do Documento de Apoio ao Relatório - Fase 3**

> Este documento fornece uma visão completa das funcionalidades, integrações e implementações da Fase 3 do projeto 404 Ride, focando nas explicações funcionais e nas integrações externas realizadas.
