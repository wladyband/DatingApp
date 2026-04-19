# Fase 1: Arquitetura Hexagonal - Ports de Repositório ✅

## O Que Foi Criado

### 1. **Camada de Ports (Abstração)**
📁 `Application/Ports/Persistence/IUserRepository.cs`
- Define o contrato entre o domínio e a persistência
- Isolado de qualquer implementação específica (EF Core, SQL, etc)
- É a "porta" de saída para dados

### 2. **Adapter de Saída (Persistência)**
📁 `Infrastructure/MongoDb/Persistence/MongoUserRepository.cs`
- Implementação concreta usando MongoDB
- Comunica com a collection configurada em `MongoDbOptions`
- Implementa as operações CRUD

### 3. **Injeção de Dependência**
📝 `Program.cs` (Atualizado)
- Registra `IUserRepository` → `UserRepository` no container DI
- Garante que todas as dependências sejam injetadas corretamente

### 4. **Use Cases**
📁 `Application/UseCases/Users/`
- `CreateUserUseCase.cs`: Demonstra como a lógica de negócio será separada
- `GetUserByIdUseCase.cs`: Busca por id
- `GetAllUsersUseCase.cs`: Lista usuários
- `DeleteUserUseCase.cs`: Remove usuário
- Contêm validações de negócio e orquestram as operações

---

## Nova Estrutura do Projeto

```
API/
├── Domain/                        # DOMÍNIO
│   ├── Entities/
│   │   └── AppUser.cs
│   ├── Exceptions/
│   └── Services/
├── Application/                   # CASOS DE USO
│   ├── Ports/                     # Abstrações (Contratos)
│   │   ├── Persistence/
│   │   │   ├── IUserRepository.cs
│   │   │   └── IAccountRepository.cs
│   │   └── External/
│   │       ├── IEmailService.cs
│   │       └── ILoggerPort.cs
│   └── UseCases/                  # Orquestração de negócio
│       ├── Account/
│       │   └── CreateAccountUseCase.cs
│       └── Users/
│           ├── CreateUserUseCase.cs
│           ├── GetUserByIdUseCase.cs
│           ├── GetAllUsersUseCase.cs
│           └── DeleteUserUseCase.cs
├── Infrastructure/                # ADAPTADORES DE SAÍDA
│   ├── External/
│   ├── MongoDb/
│   └── PostgreSql/
├── Web/                           # ADAPTADORES DE ENTRADA
│   ├── Controllers/
│   ├── Responses/
│   ├── Mappers/
│   ├── ExceptionHandling/
│   └── ApiResponse.cs
└── Program.cs
```

---

## ✨ Benefícios Conseguidos Nesta Fase

✅ **Inversão de Dependência**: Controllers não falam diretamente com banco ou adaptadores  
✅ **Testabilidade**: Você pode mockar `IUserRepository` em testes  
✅ **Flexibilidade**: Trocar MongoDB por outro adapter é só implementar a mesma port  
✅ **Separação de Conceitos**: Lógica de negócio isolada em use cases  
✅ **Sem quebras**: Código existente continua funcionando!

---

## 🔄 Próximas Fases (Quando Estiver Pronto)

### Situação atual
- Controllers finos em `Web/Controllers`
- DTOs e envelope HTTP em `Web/`
- Exceções de domínio centralizadas em `Domain/Exceptions`
- Tratamento HTTP centralizado em `Web/ExceptionHandling`
- Estrutura física já alinhada com a hexagonal

---

## 🚀 Como Usar Agora

Os use cases estão prontos para serem chamados a partir dos controllers:

```csharp
// Em um controller (exemplo)
[HttpPost]
public async Task<IActionResult> CreateUser([FromBody] CreateUserInput input)
{
    var useCase = new CreateUserUseCase(_userRepository, _emailService);
    var user = await useCase.ExecuteAsync(input);
    return Created($"/users/{user.Id}", user);
}
```

Ou registre no DI e injete via `AddApplicationServices()`.

---

## 📋 Checklist da Fase 1

- ✅ Interfaces de ports organizadas por tipo
- ✅ Adapters MongoDB registrados no DI
- ✅ Use cases e camada Web separados
- ✅ Sem quebras lógicas no código
- ✅ Estrutura física alinhada com a hexagonal

Quando estiver pronto, me avise para passarmos para a Fase 2! 🎯
