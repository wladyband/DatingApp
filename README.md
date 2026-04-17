# DatingApp

## Visão Geral

Este repositório contém uma API em .NET com arquitetura hexagonal em evolução, organizada para reduzir acoplamento entre regras de negócio e detalhes técnicos de infraestrutura.

Atualmente, o backend já está dividido em camadas com separação de responsabilidades entre:

- Core (conceitos centrais do domínio)
- Application (casos de uso e portas)
- Infrastructure (adaptadores técnicos e persistência)
- Adapters In (controllers HTTP)

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
- AppDbContext (EF Core + SQLite)
- Implementação de IUserRepository (UserRepository)
- Registro de DI da camada de infraestrutura

Arquivos principais:
- API/Infrastructure/Persistence/AppDbContext.cs
- API/Infrastructure/Persistence/UserRepository.cs
- API/Infrastructure/InfrastructureServiceExtensions.cs

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
5. Implementação acessa banco via EF Core

Representação simplificada:

Controllers -> Application UseCases -> Application Ports -> Infrastructure Adapters -> Database

---

## Como a Injeção de Dependência Está Organizada

### Composição no Program

No ponto de entrada, o projeto registra camadas separadamente:

- AddApplicationServices: registra use cases
- AddInfrastructureServices: registra DbContext e adaptadores concretos

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
- AppDbContext (SQLite)
- IUserRepository -> UserRepository

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
  - Persistence/
    - AppDbContext.cs
    - UserRepository.cs
  - InfrastructureServiceExtensions.cs
- Controllers/
  - UsersController.cs
  - WeatherForecastController.cs
- Data/
  - Migrations/
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

Persistência atual:
- EF Core com SQLite

DbContext:
- API/Infrastructure/Persistence/AppDbContext.cs

Migrations:
- Pasta atual de migrations em API/Data/Migrations

Observação:
- O DbContext foi movido para Infrastructure/Persistence.
- As migrations continuam em Data/Migrations por compatibilidade com o estado atual do projeto.

---

## O Que Já Está Alinhado com Hexagonal

- Entidade central (AppUser) fora de Infrastructure
- Use cases centralizando regras de aplicação
- Controller desacoplado de repositório direto
- Repositório acessado via port (interface)
- Implementação técnica isolada na infraestrutura
- Registro de DI separado por camada

---

## Limites Arquiteturais Recomendados

Para manter o desenho limpo conforme o projeto cresce:

1. Core não deve depender de ASP.NET, EF Core ou bibliotecas de autenticação.
2. Application não deve depender de detalhes de transporte HTTP.
3. Controllers não devem conter regras de negócio.
4. Infraestrutura não deve vazar tipos técnicos para Core/Application.

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

## Como Rodar o Backend

Pré-requisitos:
- .NET SDK compatível com o TargetFramework do projeto

Passos:

1. Restaurar pacotes
- dotnet restore

2. Compilar
- dotnet build

3. Aplicar migrations (se necessário)
- dotnet ef database update --project API/API.csproj

4. Executar API
- dotnet run --project API/API.csproj

---

## Resumo Técnico

A arquitetura atual do backend está em um nível bom de abstração para continuar evoluindo com segurança.

Principais ganhos já alcançados:
- Separação entre regra de negócio e detalhe técnico
- Uso consistente de portas e adaptadores no fluxo de usuários
- Composição por camada no DI
- Base adequada para suportar múltiplos conectores de autenticação no futuro sem quebrar o core
