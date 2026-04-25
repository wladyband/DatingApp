# DatingApp

## Visão Geral

Este repositório contém uma API em .NET 10 com arquitetura hexagonal, organizada para reduzir acoplamento entre regras de negócio e detalhes técnicos de infraestrutura.

O backend está dividido em camadas com separação de responsabilidades:

- Domain (conceitos centrais do domínio)
- Application (casos de uso e portas)
- Infrastructure (adaptadores técnicos e persistência)
- Web (adaptadores HTTP de entrada)

### Divisão de Responsabilidades por Banco

A aplicação é um site educacional pago por assinatura. As responsabilidades de persistência são divididas por domínio:

| Domínio | Banco | Estado atual |
|---|---|---|
| Conta de usuário e exercícios | MongoDB | Implementado |
| Pagamentos e assinaturas | PostgreSQL | Reservado — entidades a criar |

Essa separação é intencional: o MongoDB é o banco ativo para todas as operações do produto hoje. O PostgreSQL está presente na arquitetura com seu sistema de versionamento de schema já preparado, mas sem nenhuma tabela de negócio criada. As entidades de pagamento serão adicionadas gradualmente quando esse domínio for desenvolvido.

A autenticação ainda não foi implementada. A estrutura atual foi preparada para permitir múltiplas estratégias no futuro (ex: JWT, OAuth, Cookie, API Key) sem afetar o domínio.

---

## Setup Local de TokenKey (Desenvolvimento)

Quando o endpoint de login retorna erro 500 em ambiente local, a causa mais comum e TokenKey ausente.

Comandos .NET usados para configurar e validar:

```powershell
cd API
dotnet user-secrets set "TokenKey" "super secret key super secret key super secret key super secret key"
dotnet user-secrets list
dotnet build API.csproj
dotnet run --launch-profile https
```

Observacoes:
- Em desenvolvimento, use User Secrets para TokenKey (nao versiona no Git).
- Em producao, use variaveis de ambiente.
- A TokenKey deve ter pelo menos 64 caracteres.

---

## Estado Atual da Arquitetura

### 1) Domain

Responsabilidade:
- Representar conceitos centrais de negócio sem depender de tecnologia (web, banco, framework de auth etc).

Conteúdo atual:
- Entidade de usuário.
- Exceções de domínio.
- Serviço de domínio para hash de senha.

Arquivo principal:
- API/Domain/Entities/AppUser.cs

### 2) Application

Responsabilidade:
- Orquestrar regras de negócio por meio de casos de uso.
- Declarar contratos (ports) para comunicação com o mundo externo.

Conteúdo atual:
- Port de repositório: IUserRepository
- Use cases de usuário: criar, buscar por id, listar e deletar.
- Registro de DI da camada de aplicação.

Arquivos principais:
- API/Application/Ports/IUserRepository.cs
- API/Application/UseCases/Users/CreateUserUseCase.cs
- API/Application/UseCases/Users/GetUserByIdUseCase.cs
- API/Application/UseCases/Users/GetAllUsersUseCase.cs
- API/Application/UseCases/Users/DeleteUserUseCase.cs
- API/Application/ApplicationServiceExtensions.cs

### 3) Infrastructure

Responsabilidade:
- Implementar os contratos da Application usando tecnologias concretas.
- Conter persistência e integrações técnicas.

Conteúdo atual:
- MongoDB: adapter ativo para usuários. Cria collection, índice de email e aplica seed no startup.
- PostgreSQL: registrado sempre que a connection string estiver configurada. Aplica migrations de versionamento no startup, mas sem criar tabelas de negócio ainda. Seed de dados não é aplicado ao PostgreSQL.
- Configuração de persistência com provider atual MongoDb (PostgreSQL reservado a assinaturas)
- Runner de migrations versionadas para o PostgreSQL (`__schema_migrations`)

Arquivos principais:
- API/Infrastructure/Configuration/PersistenceOptions.cs
- API/Infrastructure/MongoDb/Configuration/MongoDbOptions.cs
- API/Infrastructure/Configuration/SeedDataOptions.cs
- API/Infrastructure/MongoDb/Persistence/MongoUserRepository.cs
- API/Infrastructure/MongoDb/Persistence/MongoAccountRepository.cs
- API/Infrastructure/PostgreSql/Persistence/PostgreSqlMigrationRunner.cs
- API/Infrastructure/PostgreSql/Migrations/  ← migrations SQL versionadas
- API/Infrastructure/MongoDb/MongoDbServiceExtensions.cs
- API/Infrastructure/PostgreSql/PostgreSqlServiceExtensions.cs
- API/Infrastructure/InfrastructureServiceExtensions.cs
- API/Infrastructure/InfrastructureInitializationExtensions.cs

### 4) Web

Responsabilidade:
- Receber requisições HTTP e delegar execução para use cases.
- Não conter regras de negócio nem acesso direto ao banco.

Conteúdo atual:
- Controllers HTTP consumindo somente use cases.
- Envelope de resposta HTTP.
- Exception filter global.
- DTOs e mapeadores de resposta.

Arquivos principais:
- API/Web/Controllers/UsersController.cs
- API/Web/Controllers/AccountController.cs
- API/Web/ApiResponse.cs
- API/Web/ExceptionHandling/ApiExceptionFilter.cs

---

## Fluxo de Dependências

Regra principal da arquitetura:
- Dependências apontam para dentro (Infrastructure e Web dependem da Application/Domain).
- Domain não depende de Infrastructure nem de Web.

Fluxo de chamada em runtime:

1. Controller recebe HTTP request
2. Controller chama Use Case
3. Use Case usa Port (interface)
4. Infrastructure fornece implementação do Port
5. Adapter selecionado acessa banco conforme provider configurado (PostgreSQL ou MongoDB)

Representação simplificada:

Web Controllers -> Application UseCases -> Application Ports -> Infrastructure Adapters -> Database

---

## Como a Injeção de Dependência Está Organizada

### Composição no Program

No ponto de entrada, o projeto registra camadas separadamente:

- AddApplicationServices: registra use cases
- AddInfrastructureServices: registra provider de persistência e adapter concreto
- InitializePersistenceAsync: aplica migrations PostgreSQL + inicializa MongoDB no startup

Arquivo:
- API/Program.cs

### Registro da Application

Arquivo:
- API/Application/ApplicationServiceExtensions.cs

Atualmente registra:
- CreateUserUseCase
- GetUserByIdUseCase
- GetAllUsersUseCase
- DeleteUserUseCase

### Registro da Infrastructure

Arquivo:
- API/Infrastructure/InfrastructureServiceExtensions.cs

Atualmente registra:
- NpgsqlDataSource: sempre registrado quando ConnectionStrings:PostgreSql está configurada (independente do provider ativo)
- IUserRepository -> MongoUserRepository quando provider = MongoDb
- IAccountRepository -> MongoAccountRepository quando provider = MongoDb
- Repositórios PostgreSQL de assinaturas serão registrados no módulo `PostgreSql/Subscriptions` quando implementados

O `NpgsqlDataSource` ser registrado independentemente do provider ativo é intencional: permite que o runner de migrations do PostgreSQL execute no startup mesmo quando o MongoDB é o provider principal.

---

## Estrutura de Pastas (Backend)

API/
- Domain/
  - Entities/
    - AppUser.cs
  - Exceptions/
    - DomainException.cs
    - UserAlreadyExistsException.cs
    - InvalidCredentialsException.cs
  - Services/
    - PasswordService.cs
- Application/
  - Ports/
    - Persistence/
      - IUserRepository.cs
      - IAccountRepository.cs
    - External/
      - IEmailService.cs
      - ILoggerPort.cs
  - UseCases/
    - Account/
      - CreateAccountUseCase.cs
    - Users/
      - CreateUserUseCase.cs
      - GetUserByIdUseCase.cs
      - GetAllUsersUseCase.cs
      - DeleteUserUseCase.cs
  - ApplicationServiceExtensions.cs
- Infrastructure/
  - External/
    - EmailService.cs
    - LoggerPortAdapter.cs
  - MongoDb/
    - Configuration/
      - MongoDbOptions.cs
      - SeedDataOptions.cs
    - Persistence/
      - MongoUserRepository.cs
      - MongoAccountRepository.cs
    - Users/
      - MongoDbUsersInitializationExtensions.cs
    - MongoDbMappingsExtensions.cs
  - PostgreSql/
    - Persistence/
      - PostgreSqlMigrationRunner.cs
    - Migrations/
      - (arquivos .sql de migrations futuras)
  - InfrastructureServiceExtensions.cs
  - InfrastructureInitializationExtensions.cs
- Web/
  - Controllers/
    - BaseApiController.cs
    - UsersController.cs
    - AccountController.cs
  - Responses/
    - UserResponse.cs
    - AccountResponse.cs
  - Mappers/
    - EntityToResponseMapper.cs
  - ExceptionHandling/
    - ApiExceptionFilter.cs
  - ApiResponse.cs
- Program.cs

---

## Endpoints Atualmente Disponíveis

Base route de usuários:
- /api/users

Endpoints:
- POST /api/users
- GET /api/users/{id}
- GET /api/users
- DELETE /api/users/{id}

Controllers responsáveis:
- API/Web/Controllers/UsersController.cs
- API/Web/Controllers/AccountController.cs

---

## Banco de Dados e Migrations

### Responsabilidade por Banco

**MongoDB** (provider ativo em desenvolvimento):
- Gerencia: conta de usuário, exercícios
- Inicialização automática: cria collection e índice de email no startup
- Seed de dados: configurável por ambiente via SeedData (Upsert ou IfEmpty)
- Tolerância a schema: `SetIgnoreExtraElements(true)` — documentos com campos legados não causam erro

**PostgreSQL** (reservado para pagamentos):
- Gerencia: informações de assinatura e pagamento (a implementar)
- Inicialização automática: aplica migrations SQL versionadas no startup
- Seed de dados: nunca recebe seed automático — cada domínio cuida do próprio seed
- Estado atual: apenas a tabela de histórico `__schema_migrations` existe; nenhuma tabela de negócio foi criada ainda

### Sistema de Migrations do PostgreSQL

O runner de migrations (`PostgreSqlMigrationRunner`) opera da seguinte forma:

1. Cria a tabela `__schema_migrations` se não existir
2. Lê os arquivos `.sql` da pasta `Infrastructure/PostgreSql/Migrations/` em ordem por nome
3. Compara com o histórico já aplicado
4. Executa apenas as migrations pendentes, cada uma em transação individual com rollback automático em caso de erro
5. Registra a migration aplicada com timestamp UTC

O runner executa sempre que a `ConnectionStrings:PostgreSql` estiver configurada, independente do provider ativo de usuários. Isso garante que o schema de pagamentos esteja sempre atualizado, mesmo quando o MongoDB é o banco principal.

### Convenção de Nomes das Migrations

Formato obrigatório: `<timestamp>_<descricao>.sql`

Exemplos:
- 202604180001_create_subscriptions.sql
- 202604180002_add_plan_to_subscriptions.sql

### Seleção do Provider de Usuários

Chave Persistence:Provider em appsettings:
- MongoDb (único provider suportado no momento)

### Configurações Necessárias por Provider

PostgreSQL:
- ConnectionStrings:PostgreSql

MongoDB:
- MongoDb:ConnectionString
- MongoDb:Database
- MongoDb:UsersCollection

### Seed de Dados (MongoDB apenas)

Controlado pela seção SeedData:
- SeedData:Enabled (true/false)
- SeedData:Mode (Upsert ou IfEmpty)
- SeedData:Users (lista de usuários iniciais)

Observação:
- O contrato de usuários (`IUserRepository`) é atendido somente por MongoDB neste estágio.
- PostgreSQL está reservado ao domínio de assinaturas/pagamentos e não recebe entidades espelho de usuário.

---

## O Que Já Está Alinhado com Hexagonal

- Entidade central (AppUser) fora de Infrastructure
- Use cases centralizando regras de aplicação
- Controller desacoplado de repositório direto
- Repositório acessado via port (interface)
- Implementação técnica isolada na infraestrutura
- Registro de DI separado por camada
- Responsabilidade de cada banco delimitada por domínio (usuários/exercícios vs pagamentos)
- Sistema de migrations versionadas para evolução segura do schema PostgreSQL

---

## Limites Arquiteturais Recomendados

Para manter o desenho limpo conforme o projeto cresce:

1. Domain não deve depender de ASP.NET, EF Core ou bibliotecas de autenticação.
2. Application não deve depender de detalhes de transporte HTTP.
3. Controllers não devem conter regras de negócio.
4. Infraestrutura não deve vazar tipos técnicos para Domain/Application.
5. Entidades de usuário e exercícios permanecem no MongoDB. Não criar tabelas espelho no PostgreSQL.
6. Entidades de pagamento e assinatura pertencem ao PostgreSQL. Não armazenar no MongoDB.

---

## Como Evoluir para Múltiplas Estratégias de Autenticação (Sem Implementar Agora)

A estrutura atual já permite evoluir com baixo impacto no domínio.

Estratégia recomendada:

1. Criar novos ports na Application para autenticação
- Exemplo de responsabilidades: emissão de token, validação de credencial, provedor externo, sessão

2. Criar use cases específicos de autenticação
- Exemplo: login, refresh, logout, callback de provedor externo

3. Implementar adaptadores na Infrastructure
- Exemplo: JwtTokenProvider, ExternalAuthProviderX, SessionStore

4. Manter controllers apenas como orquestradores HTTP
- Entradas e saídas web sem regra de negócio

Com isso, trocar ou adicionar estratégia de autenticação passa a ser operação de adaptador/registro de DI, não de domínio.

---

## Melhorias Arquiteturais Implementadas (v2)

### ✅ 1. DTOs de Resposta
- **Arquivos**: `Web/Responses/UserResponse.cs`, `AccountResponse.cs`
- **Benefício**: Previne vazamento de entidades do domínio para clientes HTTP
- **Padrão**: `record UserResponse(string Id, string Email, string DisplayName)`

### ✅ 2. Exception Handling Centralizado
- **Arquivo**: `Web/ExceptionHandling/ApiExceptionFilter.cs`
- **Benefício**: Traduz `DomainException` → HTTP 400, `UserAlreadyExistsException` → 409, etc.
- **Aplicação**: Registrado globalmente em `Program.cs` via `options.Filters.Add<ApiExceptionFilter>()`

### ✅ 3. Use Cases Diretos a Partir da Web
- **Arquivos**: `Web/Controllers/UsersController.cs`, `AccountController.cs`
- **Benefício**: Remove camada intermediária desnecessária e mantém a borda HTTP fina
- **Padrão**: Controller chama use case, e o use case orquestra as portas necessárias

### ⏳ 4. Value Objects (YAGNI - Remover)
- **Status**: Removido (não utilizado no MVP)
- **Reintroduzir quando**: Email/DisplayName tiverem regras complexas ou múltiplos agregados compartilharem validação
- **Padrão**: Será record imutável com factory method `Create()`

### ✅ 5. Domain Services
- **Arquivo**: `Domain/Services/PasswordService.cs`
- **Benefício**: Centraliza lógica criptográfica (HMACSHA512) em uma única responsabilidade
- **Uso**: Chamado diretamente pelos use cases para hash/verificação de senhas

### ✅ 6. Output Ports (Abstrações)
- **Arquivos**: `Application/Ports/External/IEmailService.cs`, `ILoggerPort.cs`
- **Benefício**: Desacopla Application de implementações técnicas (SMTP, file logging, etc.)
- **Implementações**: `Infrastructure/External/EmailService.cs`, `LoggerPortAdapter.cs`

### ✅ 7. Unified Response Envelope
- **Arquivo**: `Web/ApiResponse.cs`
- **Benefício**: Garante consistência em TODAS as respostas HTTP
- **Estrutura**: `{ Success: bool, Data?: T, ErrorMessage?: string, ErrorCode?: string }`
- **Métodos**: `ApiResponse<T>.SuccessResponse(data)`, `ErrorResponse(message, code)`

### ✅ 8. Domain Exceptions
- **Arquivos**: `Domain/Exceptions/DomainException.cs`, `UserAlreadyExistsException.cs`, `InvalidCredentialsException.cs`
- **Benefício**: Distingue erros de negócio (esperados) de erros técnicos
- **Uso**: Lançadas pelos use cases, capturadas pelo exception filter

### ✅ 9. Modularização por Contexto
- **Estrutura**: 
  - `Infrastructure/MongoDb/Users/` — módulo de usuários
  - `Infrastructure/MongoDb/Accounts/` — módulo de contas
  - `Infrastructure/PostgreSql/Subscriptions/` — módulo de assinaturas (futuro)
- **Benefício**: Cada contexto autossuficiente com suas repositories, extensions e inicialização

### ✅ 10. Configuration Organizada
- **Movimento**: `Configuration/SeedDataOptions.cs` → `MongoDb/Configuration/SeedDataOptions.cs`
- **Benefício**: Configuração agora próxima do adapter que a usa
- **Padrão**: Cada provider tem sua própria pasta de configuração

### ✅ 11. Cleanup
- **Removido**: `WeatherForecast.cs`, `WeatherForecastController.cs`
- **Removido**: Pasta `Infrastructure/Configuration/` (vazia após mover SeedDataOptions)
- **Benefício**: Projeto sem placeholders, estrutura focada em negócio

### ✅ 12. Namespaces Padronizados
- **Convenção**: `API.{Camada}.{Contexto}.{Função}`
  - `API.Domain.Services`
  - `API.Application.UseCases.Users`
  - `API.Web.ExceptionHandling`
  - `API.Infrastructure.MongoDb.Users`
- **Benefício**: Fácil navegar e entender responsabilidade de cada arquivo

---

## Fluxo de Requisição Moderno (Após Melhorias)

```
HTTP Request (POST /api/users)
    ↓
UsersController (thin adapter)
  ↓
CreateUserUseCase (business logic)
  ├→ PasswordService.ComputePasswordHash() (domain service)
  ├→ IUserRepository (output port)
  └→ IEmailService (output port)
    │   └→ IUserRepository.AddAsync() (port)
    │
    └→ IEmailService.SendWelcomeEmailAsync() (external port)
    
    ↓
ApiExceptionFilter (centralized exception handling)
    ├ Domain exceptions → HTTP 400/409 + ErrorResponse
    └ Technical exceptions → HTTP 500 + ErrorResponse
    
    ↓
ApiResponse<UserResponse> (unified envelope)
    ↓
HTTP Response (200 + JSON)
```

---

## Próximos Passos Recomendados

1. **Implementar autenticação JWT**
   - Criar `Application/Ports/ITokenProvider.cs`
   - Implementar em `Infrastructure/Authentication/JwtTokenProvider.cs`
   - Adicionar use case de login que valida credenciais

2. **Expandir use cases por contexto**
  - Criar login, refresh e recuperação de senha em `Application/UseCases/Account/`
  - Adicionar casos de uso de assinatura quando PostgreSQL entrar em uso

3. **Implementar email real**
   - Substituir `EmailService.cs` com integração SendGrid ou AWS SES
   - Adicionar templates de email para boas-vindas, reset de senha, etc.

4. **Adicionar logging estruturado**
   - Configurar Serilog em `LoggerPortAdapter.cs`
   - Adicionar logs em points críticos (criação de usuário, falhas de autenticação)

5. **Criptografia de dados sensíveis**
   - Adicionar Value Object para dados criptografados (ex: `EncryptedEmail.cs`)
   - Implementar port `IEncryptionService.cs` em infraestrutura

A migration será aplicada automaticamente no próximo startup.

---

## Como Rodar o Backend

Pré-requisitos:
- .NET 10 SDK
- MongoDB local (ou via Docker) quando usar provider MongoDb
- PostgreSQL local (ou via Docker) quando usar provider PostgreSql

Passos:

1. Restaurar pacotes
- dotnet restore

2. Compilar
- dotnet build

3. Configurar provider em API/appsettings.Development.json
- Persistence:Provider = MongoDb (padrão para desenvolvimento)

4. Ajustar credenciais/conexões do provider escolhido

5. Executar API
- dotnet run --project API/API.csproj

### Rodar com Docker (recomendado para desenvolvimento)

Arquivo de orquestração local:
- docker-compose.yml

Serviços disponíveis:
- mongodb (mongo:8) na porta 27017
- postgres (postgres:17) na porta 5432

Subir ambos:
- docker compose up -d

Subir apenas MongoDB:
- docker compose up -d mongodb

Verificar containers ativos:
- docker ps

Parar containers:

Remover containers e volumes:


## Testes

### Executar todos os testes
```
dotnet test
```

### Executar apenas testes unitários
```
dotnet test tests/API.UnitTests/API.UnitTests.csproj
```

### Executar apenas testes de integração
```
dotnet test tests/API.IntegrationTests/API.IntegrationTests.csproj
```

### Com cobertura
```
dotnet test --collect:"XPlat Code Coverage"
```

---


## Resumo Técnico

A arquitetura atual do backend está em um nível bom de abstração para continuar evoluindo com segurança.

Principais ganhos já alcançados:
- Separação entre regra de negócio e detalhe técnico
- Uso consistente de portas e adaptadores no fluxo de usuários
- Composição por camada no DI
- Domínios separados por banco: MongoDB para produto, PostgreSQL para pagamentos
- Sistema de migrations versionadas para o PostgreSQL já operacional
- Base adequada para suportar múltiplos conectores de autenticação no futuro sem quebrar o core

---

# 📊 Modelo de Dados

## 🗄️ Modelo SQL (PostgreSQL - Permissões e Controle de Acesso)

### Tabelas Principais

| Tabela | Descrição | Registros |
|--------|-----------|-----------|
| `users` | Usuários do sistema | 5 |
| `roles` | Papéis/Funções | 5 |
| `actions` | Ações disponíveis | 5 |
| `resources` | Recursos protegidos | 5 |
| `permissions` | Permissões (Resource + Action) | 5 |
| `user_roles` | Associação Usuário-Papel | 5 |
| `role_permissions` | Associação Papel-Permissão | 5 |

### Scripts de Inserção SQL

#### USERS (Usuários)
```sql
INSERT INTO users (id, email, password_hash, status) VALUES
(1, 'admin@datingapp.com', 'hash_admin', 'ACTIVE'),
(2, 'moderador@datingapp.com', 'hash_mod', 'ACTIVE'),
(3, 'premium@datingapp.com', 'hash_premium', 'ACTIVE'),
(4, 'basico@datingapp.com', 'hash_basic', 'ACTIVE'),
(5, 'suporte@datingapp.com', 'hash_support', 'ACTIVE');
```

| ID | Email | Tipo | Status |
|----|----|------|--------|
| 1 | admin@datingapp.com | Administrador | ✅ ACTIVE |
| 2 | moderador@datingapp.com | Moderador | ✅ ACTIVE |
| 3 | premium@datingapp.com | Premium | ✅ ACTIVE |
| 4 | basico@datingapp.com | Básico | ✅ ACTIVE |
| 5 | suporte@datingapp.com | Suporte | ✅ ACTIVE |

#### ROLES (Papéis)
```sql
INSERT INTO roles (id, name, description) VALUES
(1, 'ADMIN', 'Acesso total ao sistema'),
(2, 'MODERATOR', 'Moderação de conteúdo e usuários'),
(3, 'PREMIUM_USER', 'Usuário com recursos premium'),
(4, 'BASIC_USER', 'Usuário padrão'),
(5, 'SUPPORT', 'Atendimento e suporte interno');
```

| ID | Nome | Descrição |
|----|------|-----------|
| 1 | 👑 ADMIN | Acesso total ao sistema |
| 2 | 🛡️ MODERATOR | Moderação de conteúdo e usuários |
| 3 | ⭐ PREMIUM_USER | Usuário com recursos premium |
| 4 | 👤 BASIC_USER | Usuário padrão |
| 5 | 🎧 SUPPORT | Atendimento e suporte interno |

#### ACTIONS (Ações)
```sql
INSERT INTO actions (id, name) VALUES
(1, 'VIEW'),
(2, 'CLICK'),
(3, 'EDIT'),
(4, 'DELETE'),
(5, 'EXPORT');
```

| ID | Ação | Descrição |
|----|------|-----------|
| 1 | 👁️ VIEW | Visualizar |
| 2 | 🖱️ CLICK | Clicar |
| 3 | ✏️ EDIT | Editar |
| 4 | 🗑️ DELETE | Deletar |
| 5 | 📤 EXPORT | Exportar |

#### RESOURCES (Recursos)
```sql
INSERT INTO resources (id, type, key, parent_resource_id, description) VALUES
(1, 'PAGE', 'page:dashboard', NULL, 'Página principal'),
(2, 'PAGE', 'page:profile', NULL, 'Página de perfil'),
(3, 'COMPONENT', 'component:profile:edit-button', 2, 'Botão editar perfil'),
(4, 'COMPONENT', 'component:dashboard:export-button', 1, 'Botão exportar dados'),
(5, 'API', 'api:users:list', NULL, 'Endpoint de listagem de usuários');
```

| ID | Tipo | Chave | Pai | Descrição |
|----|------|-------|-----|-----------|
| 1 | 📄 PAGE | page:dashboard | — | Página principal |
| 2 | 📄 PAGE | page:profile | — | Página de perfil |
| 3 | ⚙️ COMPONENT | component:profile:edit-button | 2 | Botão editar perfil |
| 4 | ⚙️ COMPONENT | component:dashboard:export-button | 1 | Botão exportar dados |
| 5 | 🔌 API | api:users:list | — | Endpoint de listagem de usuários |

#### PERMISSIONS (Permissões)
```sql
INSERT INTO permissions (id, resource_id, action_id, effect) VALUES
(1, 1, 1, 'ALLOW'),  -- VIEW page:dashboard
(2, 2, 1, 'ALLOW'),  -- VIEW page:profile
(3, 3, 2, 'ALLOW'),  -- CLICK component:profile:edit-button
(4, 4, 5, 'DENY'),   -- EXPORT component:dashboard:export-button
(5, 5, 1, 'ALLOW');  -- VIEW api:users:list
```

| ID | Resource | Ação | Efeito | Descrição |
|----|----------|------|--------|-----------|
| 1 | page:dashboard | VIEW | ✅ ALLOW | Ver dashboard |
| 2 | page:profile | VIEW | ✅ ALLOW | Ver perfil |
| 3 | component:profile:edit-button | CLICK | ✅ ALLOW | Clicável |
| 4 | component:dashboard:export-button | EXPORT | ❌ DENY | Bloqueado |
| 5 | api:users:list | VIEW | ✅ ALLOW | Acessível |

#### USER_ROLES (Usuário-Papel)
```sql
INSERT INTO user_roles (user_id, role_id, assigned_at) VALUES
(1, 1, NOW()),
(2, 2, NOW()),
(3, 3, NOW()),
(4, 4, NOW()),
(5, 5, NOW());
```

| Usuário | Papel | Atribuído em |
|---------|-------|-------------|
| admin@datingapp.com | 👑 ADMIN | Agora |
| moderador@datingapp.com | 🛡️ MODERATOR | Agora |
| premium@datingapp.com | ⭐ PREMIUM_USER | Agora |
| basico@datingapp.com | 👤 BASIC_USER | Agora |
| suporte@datingapp.com | 🎧 SUPPORT | Agora |

#### ROLE_PERMISSIONS (Papel-Permissão)
```sql
INSERT INTO role_permissions (role_id, permission_id) VALUES
(1, 1),  -- ADMIN -> VIEW dashboard
(1, 5),  -- ADMIN -> VIEW api users
(2, 5),  -- MODERATOR -> VIEW api users
(3, 3),  -- PREMIUM_USER -> CLICK edit-button
(4, 4);  -- BASIC_USER -> DENY export-button
```

| Papel | Permissão |
|-------|-----------|
| 👑 ADMIN | ✅ VIEW dashboard |
| 👑 ADMIN | ✅ VIEW api:users:list |
| 🛡️ MODERATOR | ✅ VIEW api:users:list |
| ⭐ PREMIUM_USER | ✅ CLICK edit-button |
| 👤 BASIC_USER | ❌ DENY export-button |

---

## 🍃 Modelo NoSQL (MongoDB)

### Collections

```javascript
use datingapp

const now = new Date();

// USERS (5)
db.users.insertMany([
  { _id: 1, email: "admin@datingapp.com",     password_hash: "hash_admin",   status: "ACTIVE" },
  { _id: 2, email: "moderador@datingapp.com", password_hash: "hash_mod",     status: "ACTIVE" },
  { _id: 3, email: "premium@datingapp.com",   password_hash: "hash_premium", status: "ACTIVE" },
  { _id: 4, email: "basico@datingapp.com",    password_hash: "hash_basic",   status: "ACTIVE" },
  { _id: 5, email: "suporte@datingapp.com",   password_hash: "hash_support", status: "ACTIVE" }
]);

// ROLES (5)
db.roles.insertMany([
  { _id: 1, name: "ADMIN",        description: "Acesso total ao sistema" },
  { _id: 2, name: "MODERATOR",    description: "Moderação de conteúdo e usuários" },
  { _id: 3, name: "PREMIUM_USER", description: "Usuário com recursos premium" },
  { _id: 4, name: "BASIC_USER",   description: "Usuário padrão" },
  { _id: 5, name: "SUPPORT",      description: "Atendimento e suporte interno" }
]);

// ACTIONS (5)
db.actions.insertMany([
  { _id: 1, name: "VIEW" },
  { _id: 2, name: "CLICK" },
  { _id: 3, name: "EDIT" },
  { _id: 4, name: "DELETE" },
  { _id: 5, name: "EXPORT" }
]);

// RESOURCES (5)
db.resources.insertMany([
  { _id: 1, type: "PAGE",      key: "page:dashboard",                   parent_resource_id: null, description: "Página principal" },
  { _id: 2, type: "PAGE",      key: "page:profile",                     parent_resource_id: null, description: "Página de perfil" },
  { _id: 3, type: "COMPONENT", key: "component:profile:edit-button",    parent_resource_id: 2,    description: "Botão editar perfil" },
  { _id: 4, type: "COMPONENT", key: "component:dashboard:export-button",parent_resource_id: 1,    description: "Botão exportar dados" },
  { _id: 5, type: "API",       key: "api:users:list",                   parent_resource_id: null, description: "Endpoint de listagem de usuários" }
]);

// PERMISSIONS (5)
db.permissions.insertMany([
  { _id: 1, resource_id: 1, action_id: 1, effect: "ALLOW" }, // VIEW page:dashboard
  { _id: 2, resource_id: 2, action_id: 1, effect: "ALLOW" }, // VIEW page:profile
  { _id: 3, resource_id: 3, action_id: 2, effect: "ALLOW" }, // CLICK component:profile:edit-button
  { _id: 4, resource_id: 4, action_id: 5, effect: "DENY"  }, // EXPORT component:dashboard:export-button
  { _id: 5, resource_id: 5, action_id: 1, effect: "ALLOW" }  // VIEW api:users:list
]);

// USER_ROLES (5)
db.user_roles.insertMany([
  { user_id: 1, role_id: 1, assigned_at: now },
  { user_id: 2, role_id: 2, assigned_at: now },
  { user_id: 3, role_id: 3, assigned_at: now },
  { user_id: 4, role_id: 4, assigned_at: now },
  { user_id: 5, role_id: 5, assigned_at: now }
]);

// ROLE_PERMISSIONS (5)
db.role_permissions.insertMany([
  { role_id: 1, permission_id: 1 }, // ADMIN -> VIEW dashboard
  { role_id: 1, permission_id: 5 }, // ADMIN -> VIEW api users
  { role_id: 2, permission_id: 5 }, // MODERATOR -> VIEW api users
  { role_id: 3, permission_id: 3 }, // PREMIUM_USER -> CLICK edit-button
  { role_id: 4, permission_id: 4 }  // BASIC_USER -> DENY export-button
]);

// Índices recomendados para consistência/performance
db.users.createIndex({ email: 1 }, { unique: true });
db.roles.createIndex({ name: 1 }, { unique: true });
db.actions.createIndex({ name: 1 }, { unique: true });
db.resources.createIndex({ key: 1 }, { unique: true });
db.user_roles.createIndex({ user_id: 1, role_id: 1 }, { unique: true });
db.role_permissions.createIndex({ role_id: 1, permission_id: 1 }, { unique: true });
db.permissions.createIndex({ resource_id: 1, action_id: 1 }, { unique: true });
```

### Índices Criados

| Collection | Campo(s) | Único |
|-----------|----------|-------|
| users | `email` | ✅ Sim |
| roles | `name` | ✅ Sim |
| actions | `name` | ✅ Sim |
| resources | `key` | ✅ Sim |
| user_roles | `user_id, role_id` | ✅ Sim |
| role_permissions | `role_id, permission_id` | ✅ Sim |
| permissions | `resource_id, action_id` | ✅ Sim |

---

## 📋 Representação em JSON

### Estrutura Completa

```json
{
  "database": "datingapp",
  "collections": {
    "users": [
      { "_id": "2e0202c3-4551-4d75-a4d4-13b276626b75", "email": "admin@datingapp.com", "password_hash": "hash_admin", "status": "ACTIVE" },
      { "_id": "6f6d3a59-0f20-4fe0-a7a7-7a3da3f1f181", "email": "moderador@datingapp.com", "password_hash": "hash_mod", "status": "ACTIVE" },
      { "_id": "ef90f5d5-8c3a-4d26-9b02-6f4f4b0baf70", "email": "premium@datingapp.com", "password_hash": "hash_premium", "status": "ACTIVE" },
      { "_id": "a4d5f2c7-8ff5-4470-909f-96b7f7c16a2f", "email": "basico@datingapp.com", "password_hash": "hash_basic", "status": "ACTIVE" },
      { "_id": "f1e8c7de-5a20-4f55-90ba-2cc82a3d6c14", "email": "suporte@datingapp.com", "password_hash": "hash_support", "status": "ACTIVE" }
    ],
    "roles": [
      { "_id": "9b5ac0d7-b04a-46b7-9a9d-3f1b8368bb95", "name": "ADMIN", "description": "Acesso total ao sistema" },
      { "_id": "f72f4a14-2d14-4f3e-85a5-3ce84f89f9e7", "name": "MODERATOR", "description": "Moderação de conteúdo e usuários" },
      { "_id": "1ca8b8d2-7166-4a37-b771-0f825e6e4d21", "name": "PREMIUM_USER", "description": "Usuário com recursos premium" },
      { "_id": "3f6c4470-6f34-4f78-a7a4-8f2df4e6497a", "name": "BASIC_USER", "description": "Usuário padrão" },
      { "_id": "b49c93cd-cc76-4c4f-bb2b-f0a0b7d45066", "name": "SUPPORT", "description": "Atendimento e suporte interno" }
    ],
    "actions": [
      { "_id": "a0f2d132-7b5f-45a0-8bf3-8f77b25e9c41", "name": "VIEW" },
      { "_id": "0f9225df-5715-4339-bde7-1ff92f2189e5", "name": "CLICK" },
      { "_id": "8b5387ce-c08c-4f6e-a4af-4fd7f0534f64", "name": "EDIT" },
      { "_id": "6dfd38bd-d5d1-4b85-8ecf-53a39f6f2ebd", "name": "DELETE" },
      { "_id": "ccecf94b-9ef3-4816-b76b-c8f0fe58f3d8", "name": "EXPORT" }
    ],
    "resources": [
      { "_id": "d98f6ca0-f96f-4caf-8ec4-c7dbf7e5e87b", "type": "PAGE", "key": "page:dashboard", "parent_resource_id": null, "description": "Página principal" },
      { "_id": "49cd4d0f-2db9-4d9a-8a6f-b64546a1e014", "type": "PAGE", "key": "page:profile", "parent_resource_id": null, "description": "Página de perfil" },
      { "_id": "60f8a75a-b4cc-4638-9f66-84f31d9114cd", "type": "COMPONENT", "key": "component:profile:edit-button", "parent_resource_id": "49cd4d0f-2db9-4d9a-8a6f-b64546a1e014", "description": "Botão editar perfil" },
      { "_id": "88cd30e1-88d4-48b8-a694-82f8b3e0f4a7", "type": "COMPONENT", "key": "component:dashboard:export-button", "parent_resource_id": "d98f6ca0-f96f-4caf-8ec4-c7dbf7e5e87b", "description": "Botão exportar dados" },
      { "_id": "23d19359-38bf-4d3a-bd49-6fd7db8d0d20", "type": "API", "key": "api:users:list", "parent_resource_id": null, "description": "Endpoint de listagem de usuários" }
    ],
    "permissions": [
      { "_id": "cabf1f97-6f0e-4dca-95f1-9ec2be5f7e60", "resource_id": "d98f6ca0-f96f-4caf-8ec4-c7dbf7e5e87b", "action_id": "a0f2d132-7b5f-45a0-8bf3-8f77b25e9c41", "effect": "ALLOW" },
      { "_id": "5e11dfe8-82c3-4f63-91c5-78889ffdcfbc", "resource_id": "49cd4d0f-2db9-4d9a-8a6f-b64546a1e014", "action_id": "a0f2d132-7b5f-45a0-8bf3-8f77b25e9c41", "effect": "ALLOW" },
      { "_id": "bd81c4be-927f-470a-b643-4b0c6b23e8c1", "resource_id": "60f8a75a-b4cc-4638-9f66-84f31d9114cd", "action_id": "0f9225df-5715-4339-bde7-1ff92f2189e5", "effect": "ALLOW" },
      { "_id": "f6389f8f-d2e7-4c16-8e3a-16535e0d48ea", "resource_id": "88cd30e1-88d4-48b8-a694-82f8b3e0f4a7", "action_id": "ccecf94b-9ef3-4816-b76b-c8f0fe58f3d8", "effect": "DENY" },
      { "_id": "d95af443-03ec-4b85-98f4-3f1a3d2d8e8e", "resource_id": "23d19359-38bf-4d3a-bd49-6fd7db8d0d20", "action_id": "a0f2d132-7b5f-45a0-8bf3-8f77b25e9c41", "effect": "ALLOW" }
    ],
    "user_roles": [
      { "user_id": "2e0202c3-4551-4d75-a4d4-13b276626b75", "role_id": "9b5ac0d7-b04a-46b7-9a9d-3f1b8368bb95", "assigned_at": "2026-04-20T00:00:00.000Z" },
      { "user_id": "6f6d3a59-0f20-4fe0-a7a7-7a3da3f1f181", "role_id": "f72f4a14-2d14-4f3e-85a5-3ce84f89f9e7", "assigned_at": "2026-04-20T00:00:00.000Z" },
      { "user_id": "ef90f5d5-8c3a-4d26-9b02-6f4f4b0baf70", "role_id": "1ca8b8d2-7166-4a37-b771-0f825e6e4d21", "assigned_at": "2026-04-20T00:00:00.000Z" },
      { "user_id": "a4d5f2c7-8ff5-4470-909f-96b7f7c16a2f", "role_id": "3f6c4470-6f34-4f78-a7a4-8f2df4e6497a", "assigned_at": "2026-04-20T00:00:00.000Z" },
      { "user_id": "f1e8c7de-5a20-4f55-90ba-2cc82a3d6c14", "role_id": "b49c93cd-cc76-4c4f-bb2b-f0a0b7d45066", "assigned_at": "2026-04-20T00:00:00.000Z" }
    ],
    "role_permissions": [
      { "role_id": "9b5ac0d7-b04a-46b7-9a9d-3f1b8368bb95", "permission_id": "cabf1f97-6f0e-4dca-95f1-9ec2be5f7e60" },
      { "role_id": "9b5ac0d7-b04a-46b7-9a9d-3f1b8368bb95", "permission_id": "d95af443-03ec-4b85-98f4-3f1a3d2d8e8e" },
      { "role_id": "f72f4a14-2d14-4f3e-85a5-3ce84f89f9e7", "permission_id": "d95af443-03ec-4b85-98f4-3f1a3d2d8e8e" },
      { "role_id": "1ca8b8d2-7166-4a37-b771-0f825e6e4d21", "permission_id": "bd81c4be-927f-470a-b643-4b0c6b23e8c1" },
      { "role_id": "3f6c4470-6f34-4f78-a7a4-8f2df4e6497a", "permission_id": "f6389f8f-d2e7-4c16-8e3a-16535e0d48ea" }
    ]
  },
  "indexes": {
    "users": [
      { "keys": { "email": 1 }, "options": { "unique": true } }
    ],
    "roles": [
      { "keys": { "name": 1 }, "options": { "unique": true } }
    ],
    "actions": [
      { "keys": { "name": 1 }, "options": { "unique": true } }
    ],
    "resources": [
      { "keys": { "key": 1 }, "options": { "unique": true } }
    ],
    "user_roles": [
      { "keys": { "user_id": 1, "role_id": 1 }, "options": { "unique": true } }
    ],
    "role_permissions": [
      { "keys": { "role_id": 1, "permission_id": 1 }, "options": { "unique": true } }
    ],
    "permissions": [
      { "keys": { "resource_id": 1, "action_id": 1 }, "options": { "unique": true } }
    ]
  }
}
```

---

## 📈 Relacionamentos e Fluxo de Dados

### Fluxo de Permissões

```
Usuário (user)
    ↓
User-Role (user_roles) ← Associa usuário a papéis
    ↓
Papel (role)
    ↓
Role-Permission (role_permissions) ← Associa papel a permissões
    ↓
Permissão (permission)
    ├→ Resource (resources) ← Página, componente ou API
    └→ Action (actions) ← Visualizar, clicar, editar, etc.
```

### Exemplos de Consultas

#### SQL
```sql
-- Listar todas as permissões de um usuário
SELECT DISTINCT p.id, r.key, a.name, p.effect
FROM users u
JOIN user_roles ur ON u.id = ur.user_id
JOIN role_permissions rp ON ur.role_id = rp.role_id
JOIN permissions p ON rp.permission_id = p.id
JOIN resources r ON p.resource_id = r.id
JOIN actions a ON p.action_id = a.id
WHERE u.email = 'admin@datingapp.com';
```

#### MongoDB
```javascript
// Listar todas as permissões de um usuário
db.user_roles.aggregate([
  { $match: { user_id: 1 } },
  { $lookup: { from: "role_permissions", localField: "role_id", foreignField: "role_id", as: "role_perms" } },
  { $unwind: "$role_perms" },
  { $lookup: { from: "permissions", localField: "role_perms.permission_id", foreignField: "_id", as: "perm" } },
  { $unwind: "$perm" },
  { $lookup: { from: "resources", localField: "perm.resource_id", foreignField: "_id", as: "resource" } },
  { $lookup: { from: "actions", localField: "perm.action_id", foreignField: "_id", as: "action" } },
  { $project: { resource: 1, action: 1, effect: "$perm.effect" } }
])
```

---

## ✅ Notas de Implementação

- **Unicidade**: E-mail de usuário e nomes de papéis são únicos
- **Cascata**: Deletar um usuário requer remover suas associações em `user_roles`
- **Auditoria**: Campo `assigned_at` rastreia quando um papel foi atribuído
- **Performance**: Índices múltiplos garantem queries eficientes
- **Flexibilidade**: Modelo suporta qualquer combinação de Resource + Action
- **Escalabilidade**: Pode crescer para centenas de usuários, papéis e permissões sem degradação

---

## Snapshot Denormalizado de Users

```javascript
// ========================================
// COLLECTION: users (snapshot denormalizado)
// ========================================

// ADMIN
{
  "_id": "2e0202c3-4551-4d75-a4d4-13b276626b75",
  "email": "admin@datingapp.com",
  "passwordHash": "hash_admin",
  "status": "ACTIVE",
  "createdAt": ISODate("2026-04-20T00:00:00.000Z"),
  "updatedAt": ISODate("2026-04-25T10:30:00.000Z"),
  "roles": [
    {
      "roleId": "9b5ac0d7-b04a-46b7-9a9d-3f1b8368bb95",
      "name": "ADMIN",
      "description": "Acesso total ao sistema",
      "assignedAt": ISODate("2026-04-20T00:00:00.000Z"),
      "permissions": [
        {
          "permissionId": "cabf1f97-6f0e-4dca-95f1-9ec2be5f7e60",
          "effect": "ALLOW",
          "resource": {
            "resourceId": "d98f6ca0-f96f-4caf-8ec4-c7dbf7e5e87b",
            "type": "PAGE",
            "key": "page:dashboard",
            "parentResourceId": null,
            "description": "Página principal"
          },
          "action": {
            "actionId": "a0f2d132-7b5f-45a0-8bf3-8f77b25e9c41",
            "name": "VIEW"
          }
        },
        {
          "permissionId": "d95af443-03ec-4b85-98f4-3f1a3d2d8e8e",
          "effect": "ALLOW",
          "resource": {
            "resourceId": "23d19359-38bf-4d3a-bd49-6fd7db8d0d20",
            "type": "API",
            "key": "api:users:list",
            "parentResourceId": null,
            "description": "Endpoint de listagem de usuários"
          },
          "action": {
            "actionId": "a0f2d132-7b5f-45a0-8bf3-8f77b25e9c41",
            "name": "VIEW"
          }
        }
      ]
    }
  ]
}

// MODERADOR
{
  "_id": "6f6d3a59-0f20-4fe0-a7a7-7a3da3f1f181",
  "email": "moderador@datingapp.com",
  "passwordHash": "hash_mod",
  "status": "ACTIVE",
  "createdAt": ISODate("2026-04-20T00:00:00.000Z"),
  "updatedAt": ISODate("2026-04-25T10:30:00.000Z"),
  "roles": [
    {
      "roleId": "f72f4a14-2d14-4f3e-85a5-3ce84f89f9e7",
      "name": "MODERATOR",
      "description": "Moderação de conteúdo e usuários",
      "assignedAt": ISODate("2026-04-20T00:00:00.000Z"),
      "permissions": [
        {
          "permissionId": "d95af443-03ec-4b85-98f4-3f1a3d2d8e8e",
          "effect": "ALLOW",
          "resource": {
            "resourceId": "23d19359-38bf-4d3a-bd49-6fd7db8d0d20",
            "type": "API",
            "key": "api:users:list",
            "parentResourceId": null,
            "description": "Endpoint de listagem de usuários"
          },
          "action": {
            "actionId": "a0f2d132-7b5f-45a0-8bf3-8f77b25e9c41",
            "name": "VIEW"
          }
        }
      ]
    }
  ]
}

// PREMIUM USER
{
  "_id": "ef90f5d5-8c3a-4d26-9b02-6f4f4b0baf70",
  "email": "premium@datingapp.com",
  "passwordHash": "hash_premium",
  "status": "ACTIVE",
  "createdAt": ISODate("2026-04-20T00:00:00.000Z"),
  "updatedAt": ISODate("2026-04-25T10:30:00.000Z"),
  "roles": [
    {
      "roleId": "1ca8b8d2-7166-4a37-b771-0f825e6e4d21",
      "name": "PREMIUM_USER",
      "description": "Usuário com recursos premium",
      "assignedAt": ISODate("2026-04-20T00:00:00.000Z"),
      "permissions": [
        {
          "permissionId": "bd81c4be-927f-470a-b643-4b0c6b23e8c1",
          "effect": "ALLOW",
          "resource": {
            "resourceId": "60f8a75a-b4cc-4638-9f66-84f31d9114cd",
            "type": "COMPONENT",
            "key": "component:profile:edit-button",
            "parentResourceId": "49cd4d0f-2db9-4d9a-8a6f-b64546a1e014",
            "description": "Botão editar perfil"
          },
          "action": {
            "actionId": "0f9225df-5715-4339-bde7-1ff92f2189e5",
            "name": "CLICK"
          }
        }
      ]
    }
  ]
}

// BASIC USER
{
  "_id": "a4d5f2c7-8ff5-4470-909f-96b7f7c16a2f",
  "email": "basico@datingapp.com",
  "passwordHash": "hash_basic",
  "status": "ACTIVE",
  "createdAt": ISODate("2026-04-20T00:00:00.000Z"),
  "updatedAt": ISODate("2026-04-25T10:30:00.000Z"),
  "roles": [
    {
      "roleId": "3f6c4470-6f34-4f78-a7a4-8f2df4e6497a",
      "name": "BASIC_USER",
      "description": "Usuário padrão",
      "assignedAt": ISODate("2026-04-20T00:00:00.000Z"),
      "permissions": [
        {
          "permissionId": "f6389f8f-d2e7-4c16-8e3a-16535e0d48ea",
          "effect": "DENY",
          "resource": {
            "resourceId": "88cd30e1-88d4-48b8-a694-82f8b3e0f4a7",
            "type": "COMPONENT",
            "key": "component:dashboard:export-button",
            "parentResourceId": "d98f6ca0-f96f-4caf-8ec4-c7dbf7e5e87b",
            "description": "Botão exportar dados"
          },
          "action": {
            "actionId": "ccecf94b-9ef3-4816-b76b-c8f0fe58f3d8",
            "name": "EXPORT"
          }
        }
      ]
    }
  ]
}

// SUPORTE
{
  "_id": "f1e8c7de-5a20-4f55-90ba-2cc82a3d6c14",
  "email": "suporte@datingapp.com",
  "passwordHash": "hash_support",
  "status": "ACTIVE",
  "createdAt": ISODate("2026-04-20T00:00:00.000Z"),
  "updatedAt": ISODate("2026-04-25T10:30:00.000Z"),
  "roles": [
    {
      "roleId": "b49c93cd-cc76-4c4f-bb2b-f0a0b7d45066",
      "name": "SUPPORT",
      "description": "Atendimento e suporte interno",
      "assignedAt": ISODate("2026-04-20T00:00:00.000Z"),
      "permissions": []
    }
  ]
}
```
