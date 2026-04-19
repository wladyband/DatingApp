# DatingApp

## Visão Geral

Este repositório contém uma API em .NET 10 com arquitetura hexagonal, organizada para reduzir acoplamento entre regras de negócio e detalhes técnicos de infraestrutura.

O backend está dividido em camadas com separação de responsabilidades:

- Core (conceitos centrais do domínio)
- Application (casos de uso e portas)
- Infrastructure (adaptadores técnicos e persistência)
- Adapters In (controllers HTTP)

### Divisão de Responsabilidades por Banco

A aplicação é um site educacional pago por assinatura. As responsabilidades de persistência são divididas por domínio:

| Domínio | Banco | Estado atual |
|---|---|---|
| Conta de usuário e exercícios | MongoDB | Implementado |
| Pagamentos e assinaturas | PostgreSQL | Reservado — entidades a criar |

Essa separação é intencional: o MongoDB é o banco ativo para todas as operações do produto hoje. O PostgreSQL está presente na arquitetura com seu sistema de versionamento de schema já preparado, mas sem nenhuma tabela de negócio criada. As entidades de pagamento serão adicionadas gradualmente quando esse domínio for desenvolvido.

A autenticação ainda não foi implementada. A estrutura atual foi preparada para permitir múltiplas estratégias no futuro (ex: JWT, OAuth, Cookie, API Key) sem afetar o core.

---

## Estado Atual da Arquitetura

### 1) Core

Responsabilidade:
- Representar conceitos centrais de negócio sem depender de tecnologia (web, banco, framework de auth etc).

Conteúdo atual:
- Entidade de usuário em Core.

Arquivo principal:
- API/Core/Entities/AppUser.cs

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

### 4) Adapters In (HTTP)

Responsabilidade:
- Receber requisições HTTP e delegar execução para use cases.
- Não conter regras de negócio nem acesso direto ao banco.

Conteúdo atual:
- UsersController consumindo somente use cases.

Arquivo principal:
- API/Controllers/UsersController.cs

---

## Fluxo de Dependências

Regra principal da arquitetura:
- Dependências apontam para dentro (Infrastructure e Controllers dependem da Application/Core).
- Core não depende de Infrastructure.

Fluxo de chamada em runtime:

1. Controller recebe HTTP request
2. Controller chama Use Case
3. Use Case usa Port (interface)
4. Infrastructure fornece implementação do Port
5. Adapter selecionado acessa banco conforme provider configurado (PostgreSQL ou MongoDB)

Representação simplificada:

Controllers -> Application UseCases -> Application Ports -> Infrastructure Adapters -> Database

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
- Core/
  - Entities/
    - AppUser.cs
- Application/
  - Ports/
    - IUserRepository.cs
  - UseCases/
    - Users/
      - CreateUserUseCase.cs
      - GetUserByIdUseCase.cs
      - GetAllUsersUseCase.cs
      - DeleteUserUseCase.cs
  - ApplicationServiceExtensions.cs
- Infrastructure/
  - Configuration/
    - PersistenceOptions.cs
    - SeedDataOptions.cs
  - MongoDb/
    - Configuration/
      - MongoDbOptions.cs
    - Persistence/
      - MongoUserRepository.cs
      - MongoAccountRepository.cs
    - Users/
      - MongoDbUsersModuleExtensions.cs
      - MongoDbUsersInitializationExtensions.cs
    - Accounts/
      - MongoDbAccountsModuleExtensions.cs
      - MongoDbAccountsInitializationExtensions.cs
    - MongoDbServiceExtensions.cs
    - MongoDbInitializationExtensions.cs
    - MongoDbMappingsExtensions.cs
  - PostgreSql/
    - Persistence/
      - PostgreSqlMigrationRunner.cs
    - Subscriptions/
      - PostgreSqlSubscriptionsModuleExtensions.cs
    - PostgreSqlServiceExtensions.cs
    - PostgreSqlInitializationExtensions.cs
    - Migrations/
      - (arquivos .sql de migrations futuras)
  - InfrastructureServiceExtensions.cs
  - InfrastructureInitializationExtensions.cs
- Controllers/
  - UsersController.cs
  - WeatherForecastController.cs
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

Controller responsável:
- API/Controllers/UsersController.cs

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

1. Core não deve depender de ASP.NET, EF Core ou bibliotecas de autenticação.
2. Application não deve depender de detalhes de transporte HTTP.
3. Controllers não devem conter regras de negócio.
4. Infraestrutura não deve vazar tipos técnicos para Core/Application.
5. Entidades de usuário e exercícios permanecem no MongoDB. Não criar tabelas espelho no PostgreSQL.
6. Entidades de pagamento e assinatura pertencem ao PostgreSQL. Não armazenar no MongoDB.

---

## Como Evoluir para Múltiplas Estratégias de Autenticação (Sem Implementar Agora)

A estrutura atual já permite evoluir com baixo impacto no core.

Estratégia recomendada:

1. Criar novos ports na Application para autenticação
- Exemplo de responsabilidades: emissão de token, validação de credencial, provedor externo, sessão

2. Criar use cases específicos de autenticação
- Exemplo: login, refresh, logout, callback de provedor externo

3. Implementar adaptadores na Infrastructure
- Exemplo: JwtTokenProvider, ExternalAuthProviderX, SessionStore

4. Manter controllers apenas como orquestradores HTTP
- Entradas e saídas web sem regra de negócio

Com isso, trocar ou adicionar estratégia de autenticação passa a ser operação de adaptador/registro de DI, não de core.

---

## Melhorias Arquiteturais Implementadas (v2)

### ✅ 1. DTOs de Resposta
- **Arquivos**: `Application/DTOs/Responses/UserResponse.cs`, `AccountResponse.cs`
- **Benefício**: Previne vazamento de entidades do core para clientes HTTP
- **Padrão**: `record UserResponse(string Id, string Email, string Displayname)`

### ✅ 2. Exception Handling Centralizado
- **Arquivo**: `Infrastructure/Http/Filters/ApiExceptionFilter.cs`
- **Benefício**: Traduz `DomainException` → HTTP 400, `UserAlreadyExistsException` → 409, etc.
- **Aplicação**: Registrado globalmente em `Program.cs` via `options.Filters.Add<ApiExceptionFilter>()`

### ✅ 3. Application Services (Orquestração)
- **Arquivos**: `Application/Services/UserApplicationService.cs`, `AccountApplicationService.cs`
- **Benefício**: Orquestra múltiplos use cases e integra com ports externos (email, logger)
- **Padrão**: Uma service por contexto de negócio (Users, Accounts, etc.)

### ⏳ 4. Value Objects (YAGNI - Remover)
- **Status**: Removido (não utilizado no MVP)
- **Reintroduzir quando**: Email/Displayname tiverem regras complexas ou múltiplos agregados compartilharem validação
- **Padrão**: Será record imutável com factory method `Create()`

### ✅ 5. Domain Services
- **Arquivo**: `Core/DomainServices/PasswordService.cs`
- **Benefício**: Centraliza lógica criptográfica (HMACSHA512) em uma única responsabilidade
- **Uso**: Injetado nos use cases para hash/verificação de senhas

### ✅ 6. Output Ports (Abstrações)
- **Arquivos**: `Application/Ports/IEmailService.cs`, `ILoggerPort.cs`
- **Benefício**: Desacopla Application de implementações técnicas (SMTP, file logging, etc.)
- **Implementações**: `Infrastructure/Services/EmailService.cs`, `LoggerPortAdapter.cs`

### ✅ 7. Unified Response Envelope
- **Arquivo**: `Infrastructure/Http/ApiResponse.cs`
- **Benefício**: Garante consistência em TODAS as respostas HTTP
- **Estrutura**: `{ Success: bool, Data?: T, ErrorMessage?: string, ErrorCode?: string }`
- **Métodos**: `ApiResponse<T>.SuccessResponse(data)`, `ErrorResponse(message, code)`

### ✅ 8. Domain Exceptions
- **Arquivos**: `Core/Exceptions/DomainException.cs`, `UserAlreadyExistsException.cs`, `InvalidCredentialsException.cs`
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
  - `API.Core.DomainServices`
  - `API.Application.Services`
  - `API.Infrastructure.Http.Filters`
  - `API.Infrastructure.MongoDb.Users`
- **Benefício**: Fácil navegar e entender responsabilidade de cada arquivo

---

## Fluxo de Requisição Moderno (Após Melhorias)

```
HTTP Request (POST /api/users)
    ↓
UsersController (thin adapter)
    ↓
UserApplicationService (orquestration)
    ├→ CreateUserUseCase (business logic)
    │   ├→ Email.Create() (value object validation)
    │   ├→ PasswordService.ComputePasswordHash() (domain service)
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

2. **Expandir Application Services**
   - Criar `AccountApplicationService` completo (ainda apenas CreateAccountUseCase)
   - Adicionar `SubscriptionApplicationService` quando PostgreSQL entrar em uso

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
- docker compose stop

Remover containers e volumes:
- docker compose down -v

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
