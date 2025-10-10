# CloudGames.Users - Microserviço de Usuários

## 📋 Visão Geral

O **CloudGames.Users** é o microserviço responsável pelo gerenciamento completo de usuários no sistema CloudGames. Este serviço lida com:

- 🔐 **Registro de novos usuários** - Criação de contas com validação e criptografia de senha
- 🔑 **Autenticação** - Login de usuários e geração de tokens JWT
- 👤 **Perfis de usuário** - Gerenciamento de informações pessoais e preferências
- 📚 **Biblioteca de jogos** - Controle de jogos associados aos usuários (integração futura)
- 🛡️ **Autorização baseada em roles** - Controle de acesso para usuários comuns e administradores

Este serviço é fundamental para o funcionamento da plataforma, sendo responsável pela identidade e autenticação em toda a arquitetura de microserviços.

---

## 🛠️ Tecnologias Utilizadas

### Principais Tecnologias
- **Framework:** ASP.NET Core 8.0
- **Linguagem:** C# (.NET 8.0)
- **Banco de Dados:** SQL Server
- **ORM:** Entity Framework Core 8.0
- **Autenticação:** JWT (JSON Web Tokens)
- **Criptografia de Senha:** BCrypt.Net-Next
- **Observabilidade:** OpenTelemetry, Serilog
- **Documentação:** Swagger/OpenAPI
- **Mensageria:** Azure Storage Queue
- **Padrões:** Event Sourcing, Outbox Pattern, CQRS

### Dependências Principais
- `Microsoft.AspNetCore.Authentication.JwtBearer` - Autenticação JWT
- `Microsoft.EntityFrameworkCore.SqlServer` - Acesso ao SQL Server
- `BCrypt.Net-Next` - Hash de senhas
- `Azure.Storage.Queues` - Integração com Azure Storage Queue
- `Serilog` - Logging estruturado
- `OpenTelemetry` - Telemetria e observabilidade

---

## 🚀 Como Executar Localmente

### Pré-requisitos
- .NET 8.0 SDK
- SQL Server (local ou Docker)
- Azure Storage Emulator ou Azurite (para queues)

### Opção 1: Executar com .NET CLI

1. **Configure a connection string no arquivo `appsettings.json`:**
```json
{
  "ConnectionStrings": {
    "UsersDb": "Server=localhost;Database=CloudGamesUsers;User Id=sa;Password=SuaSenha123;TrustServerCertificate=True;",
    "Storage": "UseDevelopmentStorage=true"
  },
  "JwtSettings": {
    "Secret": "SuaChaveSecretaSuperSeguraComPeloMenos32Caracteres",
    "Issuer": "CloudGames",
    "Audience": "CloudGamesUsers"
  }
}
```

2. **Execute o serviço:**
```bash
cd CloudGames.Users.Web
dotnet restore
dotnet run
```

3. **Acesse a documentação Swagger:**
   - Navegue para: `https://localhost:5001/swagger` (ou a porta configurada)

### Opção 2: Executar com Docker

1. **Build da imagem:**
```bash
docker build -t cloudgames-users -f Dockerfile .
```

2. **Execute o container:**
```bash
docker run -p 8080:80 \
  -e ConnectionStrings__UsersDb="YourConnectionString" \
  -e JwtSettings__Secret="YourSecretKey" \
  cloudgames-users
```

### Aplicar Migrações de Banco de Dados

As migrações são aplicadas automaticamente na inicialização do serviço. Para criar novas migrações:

```bash
cd CloudGames.Users.Web
dotnet ef migrations add NomeDaMigracao --project ../CloudGames.Users.Infra
```

---

## 📡 Documentação dos Endpoints

### Base URL
```
https://localhost:5001/api/users
```

### 🔓 Endpoints Públicos (sem autenticação)

#### 1. Registrar Novo Usuário
Cria uma nova conta de usuário no sistema.

```http
POST /api/users
Content-Type: application/json
```

**Request Body:**
```json
{
  "name": "João Silva",
  "email": "joao.silva@email.com",
  "password": "SenhaSegura123!"
}
```

**Response (201 Created):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "João Silva",
  "email": "joao.silva@email.com",
  "role": "User",
  "isActive": true,
  "createdAt": "2025-10-10T14:30:00Z",
  "updatedAt": null
}
```

**Headers de Resposta:**
- `Location: /api/users/3fa85f64-5717-4562-b3fc-2c963f66afa6`

**Códigos de Status:**
- `201` - Usuário criado com sucesso
- `400` - Dados inválidos (validação falhou)
- `409` - Email já cadastrado

---

#### 2. Autenticar Usuário (Login)
Autentica um usuário e retorna um token JWT para uso em requisições protegidas.

```http
POST /api/users/authenticate
Content-Type: application/json
```

**Request Body:**
```json
{
  "email": "joao.silva@email.com",
  "password": "SenhaSegura123!"
}
```

**Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "João Silva",
    "email": "joao.silva@email.com",
    "role": "User",
    "isActive": true,
    "createdAt": "2025-10-10T14:30:00Z",
    "updatedAt": null
  }
}
```

**Códigos de Status:**
- `200` - Autenticação bem-sucedida
- `401` - Credenciais inválidas (email ou senha incorretos)

**⏱️ Validade do Token:** 8 horas

---

### 🔐 Endpoints Autenticados (requerem JWT)

Para acessar os endpoints abaixo, inclua o token JWT no header:
```
Authorization: Bearer {seu_token_jwt}
```

#### 3. Obter Informações do Usuário Atual
Retorna as informações do usuário autenticado (baseado no token JWT).

```http
GET /api/users/me
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "João Silva",
  "email": "joao.silva@email.com",
  "role": "User",
  "isActive": true,
  "createdAt": "2025-10-10T14:30:00Z",
  "updatedAt": null
}
```

**Códigos de Status:**
- `200` - Informações recuperadas com sucesso
- `401` - Token inválido ou não fornecido

---

#### 4. Buscar Usuário por ID
Retorna as informações de um usuário específico pelo seu ID.

```http
GET /api/users/{id}
Authorization: Bearer {token}
```

**Parâmetros:**
- `id` (UUID) - ID do usuário

**Response (200 OK):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "João Silva",
  "email": "joao.silva@email.com",
  "role": "User",
  "isActive": true,
  "createdAt": "2025-10-10T14:30:00Z",
  "updatedAt": null
}
```

**Códigos de Status:**
- `200` - Usuário encontrado
- `401` - Token inválido ou não fornecido
- `404` - Usuário não encontrado

---

#### 5. Atualizar Perfil de Usuário
Atualiza as informações de um usuário. Usuários comuns só podem atualizar seu próprio perfil.

```http
PUT /api/users/{id}
Authorization: Bearer {token}
Content-Type: application/json
```

**Parâmetros:**
- `id` (UUID) - ID do usuário a ser atualizado

**Request Body:**
```json
{
  "name": "João Silva Santos",
  "email": "joao.santos@email.com"
}
```

**Request Body (Admin - com campos extras):**
```json
{
  "name": "João Silva Santos",
  "email": "joao.santos@email.com",
  "role": "Administrator",
  "isActive": false
}
```

**Response (200 OK):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "João Silva Santos",
  "email": "joao.santos@email.com",
  "role": "User",
  "isActive": true,
  "createdAt": "2025-10-10T14:30:00Z",
  "updatedAt": "2025-10-10T15:45:00Z"
}
```

**Códigos de Status:**
- `200` - Usuário atualizado com sucesso
- `400` - Dados inválidos
- `401` - Token inválido ou não fornecido
- `403` - Você não tem permissão para atualizar este usuário
- `404` - Usuário não encontrado

**⚠️ Regras de Negócio:**
- Usuários comuns só podem atualizar seu próprio perfil (ID do token = ID na URL)
- Usuários comuns não podem alterar `role` ou `isActive`
- Administradores podem atualizar qualquer usuário e alterar todos os campos

---

### 👑 Endpoints de Administrador (requerem role Administrator)

#### 6. Listar Todos os Usuários
Retorna uma lista de todos os usuários cadastrados no sistema. **Acesso exclusivo para administradores.**

```http
GET /api/users
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "João Silva",
    "email": "joao.silva@email.com",
    "role": "User",
    "isActive": true,
    "createdAt": "2025-10-10T14:30:00Z",
    "updatedAt": null
  },
  {
    "id": "7b25f84-9821-4562-b3fc-2c963f66afa7",
    "name": "Maria Santos",
    "email": "maria.santos@email.com",
    "role": "Administrator",
    "isActive": true,
    "createdAt": "2025-10-09T10:15:00Z",
    "updatedAt": null
  }
]
```

**Códigos de Status:**
- `200` - Usuários recuperados com sucesso
- `401` - Token inválido ou não fornecido
- `403` - Acesso negado - Role de Administrador necessária

---

## 👥 Roles de Usuário

O sistema possui dois tipos de usuários com diferentes níveis de acesso:

### 🔵 User (Usuário Comum)
**Valor numérico:** `0`

**Permissões:**
- ✅ Registrar nova conta
- ✅ Fazer login
- ✅ Visualizar próprio perfil (`/api/users/me`)
- ✅ Visualizar informações de outros usuários (`/api/users/{id}`)
- ✅ Atualizar **apenas** seu próprio perfil
- ❌ Não pode alterar `role` ou `isActive`
- ❌ Não pode listar todos os usuários
- ❌ Não pode atualizar outros usuários

**Role padrão:** Novos usuários são criados automaticamente com role `User`.

---

### 🔴 Administrator (Administrador)
**Valor numérico:** `1`

**Permissões:**
- ✅ Todas as permissões de usuário comum
- ✅ Listar **todos** os usuários do sistema (`GET /api/users`)
- ✅ Atualizar **qualquer** usuário
- ✅ Alterar role de outros usuários
- ✅ Ativar/desativar usuários (campo `isActive`)
- ✅ Acesso completo a todas as operações de gerenciamento

**⚠️ Importante:** Apenas administradores podem promover usuários para Administrator.

---

## 🔒 Segurança

### Autenticação JWT
- **Algoritmo:** HMAC-SHA256
- **Validade do Token:** 8 horas
- **Claims incluídas:**
  - `NameIdentifier` - ID do usuário (GUID)
  - `Name` - Nome do usuário
  - `Email` - Email do usuário
  - `Role` - Role do usuário (User ou Administrator)

### Proteção de Senha
- **Algoritmo:** BCrypt (salt rounds configurável)
- As senhas nunca são armazenadas em texto plano
- Hash gerado automaticamente no registro e atualização

### Validações
- Email deve ser único no sistema
- Senhas devem atender aos requisitos de complexidade
- Proteção contra acesso não autorizado com middleware de autenticação

---

## 🗄️ Arquitetura e Padrões

### Clean Architecture
O projeto segue os princípios da Clean Architecture com separação clara de responsabilidades:

- **Domain** - Entidades e regras de negócio
- **Application** - Casos de uso, DTOs, handlers (CQRS)
- **Infrastructure** - Implementação de repositórios, Event Store, Outbox
- **Web** - Controllers, configuração, middleware

### Padrões Implementados
- **CQRS** - Separação entre comandos (write) e consultas (read)
- **Event Sourcing** - Rastreamento de todas as mudanças como eventos
- **Outbox Pattern** - Garantia de consistência eventual com mensageria
- **Repository Pattern** - Abstração do acesso a dados
- **Unit of Work** - Gerenciamento de transações

---

## 📊 Observabilidade

### Logs Estruturados (Serilog)
- Logs em formato estruturado para melhor análise
- Níveis: Information, Warning, Error
- Saída: Console (desenvolvimento) e Application Insights (produção)

### Telemetria (OpenTelemetry)
- Rastreamento de requisições HTTP
- Métricas de performance
- Correlação de logs entre microserviços

### Health Checks
```http
GET /health
```
Verifica o status do serviço e suas dependências.

---

## 🔧 Variáveis de Ambiente

| Variável | Descrição | Obrigatória | Padrão |
|----------|-----------|-------------|---------|
| `ConnectionStrings__UsersDb` | Connection string do SQL Server | Sim | - |
| `ConnectionStrings__Storage` | Connection string do Azure Storage | Não | `UseDevelopmentStorage=true` |
| `JwtSettings__Secret` | Chave secreta para geração de JWT (mínimo 32 caracteres) | Sim | - |
| `JwtSettings__Issuer` | Emissor do token JWT | Não | `CloudGames` |
| `JwtSettings__Audience` | Audiência do token JWT | Não | `CloudGamesUsers` |
| `Queues__Users` | Nome da fila no Azure Storage | Não | `users-events` |

---

## 🧪 Testes

Para executar os testes unitários:

```bash
cd CloudGames.Users.Tests
dotnet test
```

---

## 📦 Integração com Outros Microserviços

### Event Publishing
O serviço publica eventos importantes via Azure Storage Queue:

- `UserRegistered` - Quando um novo usuário é registrado
- `UserUpdated` - Quando um usuário atualiza seu perfil
- `UserAuthenticated` - Quando um usuário faz login (opcional)

Estes eventos podem ser consumidos por outros microserviços para:
- Envio de emails de boas-vindas
- Sincronização de dados
- Auditoria e analytics

---

## 📄 Licença

Este projeto faz parte do sistema CloudGames e está sob licença proprietária.

---

**Versão do serviço:** 1.0.0  
**Última atualização:** Outubro 2025
