# Testando os Endpoints da API de Usuários

## Swagger

Com a configuração atual da API, você pode acessar o Swagger pelos links abaixo:

- HTTP: http://localhost:5001/swagger
- HTTPS: https://localhost:7023/swagger

OpenAPI JSON:

- HTTP: http://localhost:5001/swagger/v1/swagger.json
- HTTPS: https://localhost:7023/swagger/v1/swagger.json

> Observação: o Swagger só é habilitado em ambiente Development.

## Endpoints Disponíveis

A aplicação agora possui 4 endpoints para gerenciar usuários, seguindo a arquitetura hexagonal.

### 1. Criar Usuário (POST)

```http
POST http://localhost:5000/api/users
Content-Type: application/json

{
  "email": "joao@example.com",
  "displayName": "João Silva"
}
```

**Respostas:**
- `201 Created`: Usuário criado com sucesso
- `400 Bad Request`: Email ou displayName vazio
- `409 Conflict`: Email já existe

**Exemplo cURL:**
```bash
curl -X POST http://localhost:5000/api/users \
  -H "Content-Type: application/json" \
  -d '{
    "email": "joao@example.com",
    "displayName": "João Silva"
  }'
```

---

### 2. Obter Usuário por ID (GET)

```http
GET http://localhost:5000/api/users/{id}
```

**Respostas:**
- `200 OK`: Retorna o usuário
- `404 Not Found`: Usuário não existe

**Exemplo:**
```bash
curl http://localhost:5000/api/users/123e4567-e89b-12d3-a456-426614174000
```

---

### 3. Listar Todos os Usuários (GET)

```http
GET http://localhost:5000/api/users
```

**Respostas:**
- `200 OK`: Retorna array de usuários

**Exemplo:**
```bash
curl http://localhost:5000/api/users
```

---

### 4. Deletar Usuário (DELETE)

```http
DELETE http://localhost:5000/api/users/{id}
```

**Respostas:**
- `204 No Content`: Usuário deletado
- `404 Not Found`: Usuário não existe

**Exemplo:**
```bash
curl -X DELETE http://localhost:5000/api/users/123e4567-e89b-12d3-a456-426614174000
```

---

## Como Testar Localmente

### Opção 1: Usando Postman/Insomnia

1. Inicie a aplicação: `dotnet run`
2. A API estará em `http://localhost:5000`
3. Importe os endpoints acima em seu cliente HTTP preferido

### Opção 2: Usando cURL no PowerShell

```powershell
# Criar usuário
$body = @{
  email = "maria@example.com"
  displayName = "Maria"
} | ConvertTo-Json

$response = Invoke-WebRequest -Uri "http://localhost:5000/api/users" `
  -Method POST `
  -Headers @{"Content-Type"="application/json"} `
  -Body $body

$userId = ($response.Content | ConvertFrom-Json).id

# Obter usuário
Invoke-WebRequest -Uri "http://localhost:5000/api/users/$userId" -Method GET

# Listar todos
Invoke-WebRequest -Uri "http://localhost:5000/api/users" -Method GET

# Deletar
Invoke-WebRequest -Uri "http://localhost:5000/api/users/$userId" -Method DELETE
```

### Opção 3: Via .http file (VS Code REST Client)

Crie um arquivo `test-api.http` na raiz do projeto:

```http
### Criar Usuário
POST http://localhost:5000/api/users
Content-Type: application/json

{
  "email": "carlos@example.com",
  "displayName": "Carlos"
}

### Listar Usuários
GET http://localhost:5000/api/users

### Obter Usuário (substituir ID)
GET http://localhost:5000/api/users/{id}

### Deletar Usuário (substituir ID)
DELETE http://localhost:5000/api/users/{id}
```

Depois clique em "Send Request" acima de cada bloco.

---

## Fluxo de Arquitetura Hexagonal em Ação

```
HTTP Request (Entrada)
    ↓
UsersController (Adapter In)
    ↓
Use Case (Lógica de Negócio)
    ├── Validações
    ├── Verificações
    └── Orquestração
    ↓
IUserRepository (Port - Abstração)
    ↓
UserRepository (Adapter Out - Persistência)
    ↓
AppDbContext (EF Core)
    ↓
SQLite (Banco de Dados)
```

**Benefício**: Se você trocar SQLite por PostgreSQL amanhã, apenas o `UserRepository` muda. Tudo acima continua igual!

---

## Estrutura após Fase 1 + Fase 2

```
API/
├── Web/
│   ├── WeatherForecastController.cs  (Lógica simples)
│   └── UsersController.cs             (Usa Use Cases) ← ⭐
├── Application/
│   ├── ApplicationServiceExtensions.cs (DI)
│   ├── Ports/
│   │   └── IUserRepository.cs
│   └── UseCases/
│       └── Users/
│           ├── CreateUserUseCase.cs
│           └── GetUserByIdUseCase.cs
├── Infrastructure/
│   └── Persistence/
│       └── UserRepository.cs
├── Data/
│   └── AppDbContext.cs
└── Program.cs
```

---

## ✨ Próximas Melhorias

1. **Use Case para listar usuários** (em vez de ir direto ao repositório)
2. **Use Case para deletar usuários**
3. **Validações customizadas** (email válido, etc)
4. **Exceções customizadas de domínio**
5. **DTOs de saída** (para não expor `AppUser` diretamente)

Tudo pode ser adicionado gradualmente! 🚀
