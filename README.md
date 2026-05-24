# 📚 Plataforma de Cursos — API REST

![CI](https://github.com/hemervalviana/plataforma-cursos-api/actions/workflows/ci.yml/badge.svg)

API REST desenvolvida em .NET 10 para gerenciamento de cursos, estudantes, matrículas e pagamentos, com autenticação baseada em JWT e integração com ASP.NET Identity.

---

## 🎯 Objetivo

Fornecer uma API segura e escalável para:
- Cadastro e autenticação de usuários (Students)
- Gerenciamento de cursos
- Matrícula de alunos
- Processamento de pagamentos com idempotência e controle de estados
- Controle de acesso por papéis (Roles)

---

## 🛠️ Tecnologias

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core 10
- ASP.NET Identity
- JWT (JSON Web Token)
- SQL Server
- AutoMapper
- Scalar (documentação OpenAPI)
- FluentValidation
- xUnit + FluentAssertions + Moq
- Docker + Docker Compose
- GitHub Actions (CI/CD)

---

## 📂 Arquitetura

O projeto segue Clean Architecture + DDD em camadas:
PlataformaCursos.API
│
├── Domain                  ← núcleo — sem dependências externas
│   ├── Entities            ← agregados (Payment, Enrollment, Course, Student)
│   ├── Enums               ← PaymentStatus, PaymentMethod
│   ├── ValueObjects        ← Money
│   └── DTOs                ← contratos de entrada/saída por feature
│
├── Application             ← orquestração — depende só do Domain
│   ├── Services            ← casos de uso (PaymentService, EnrollmentService...)
│   ├── Interfaces          ← contratos (IPaymentGateway, IPaymentRepository)
│   ├── Validators          ← FluentValidation por DTO
│   ├── Mapping             ← AutoMapper profiles
│   └── Common              ← PagedResult, Exceptions, ETagHelper
│
├── Infrastructure          ← detalhes técnicos — implementa contratos do Domain
│   ├── Data                ← ApplicationDbContext, DbInitializer, PaymentRepository
│   ├── Configurations      ← EF Core entity configurations
│   ├── Gateways            ← FakePaymentGateway (adapter)
│   └── Middleware          ← ExceptionMiddleware, CorrelationIdMiddleware
│
├── Controllers             ← exposição HTTP — depende só da Application
│
└── Program.cs              ← composição raiz (DI, pipeline)

### Regras de dependência
Controllers → Application → Domain
Infrastructure → Domain (via interfaces)
Domain → nada (independente)

A camada de Domínio nunca conhece EF Core, HTTP ou qualquer detalhe de infraestrutura.

---

## 🗄️ Modelo de Dados

### Entidades principais

**Student** (herda IdentityUser)
- `FullName`, `CreatedAt`, `IsActive`, `IsDeleted`

**Course**
- `Title`, `Description`, `Category`, `Workload`, `CreatedAt`, `IsDeleted`

**Enrollment**
- `StudentId`, `CourseId`, `Status`, `CreatedAt`, `IsDeleted`

**Payment** (agregado principal do Nível 2)
- `Id`, `EnrollmentId`, `StudentId`
- `Amount` (Money — Value Object), `Currency`
- `Status` (Pending → Paid/Failed → Refunded)
- `Method` (Pix, CreditCard, DebitCard, BankSlip)
- `IdempotencyKey`, `TransactionId`, `FailureReason`
- `CreatedAt`, `PaidAt`, `UpdatedAt`, `IsDeleted`

### Fluxo de estados do Payment
     ┌─────────┐
     │ Pending │
     └────┬────┘
   ┌──────┴──────┐
   ▼             ▼
┌──────┐      ┌────────┐
│ Paid │      │ Failed │
└──┬───┘      └────────┘
▼
┌──────────┐
│ Refunded │
└──────────┘

---

## 🔐 Autenticação e Segurança

A autenticação é feita via JWT. Todas as requisições protegidas devem conter:
Authorization: Bearer {token}

### Seed inicial

| Usuário | Email | Senha | Role |
|---|---|---|---|
| Administrador | admin@plataforma.com | Admin123! | Admin |
| Instrutor | instructor@plataforma.com | Instructor123! | Instructor |
| Aluno | student@plataforma.com | Student123! | Student |

### Matriz de permissões

| Endpoint | Público | Student | Admin |
|---|---|---|---|
| POST /api/auth/register | ✅ | | |
| POST /api/auth/login | ✅ | | |
| GET /api/courses | ✅ | ✅ | ✅ |
| POST /api/courses | | | ✅ |
| POST /api/enrollments | | ✅ | ✅ |
| GET /api/enrollments | | próprias | todas |
| POST /api/payments | | ✅ | ✅ |
| POST /api/payments/{id}/confirm | | | ✅ |
| POST /api/payments/{id}/refund | | | ✅ |
| GET /api/payments | | próprios | todos |
| GET /health | ✅ | | |

---

## ⚙️ Configuração do Ambiente

### Requisitos
- .NET SDK 10+
- SQL Server (local ou Docker)

### User Secrets (desenvolvimento local)

```bash
dotnet user-secrets init --project PlataformaCursos.API

dotnet user-secrets set "ConnectionStrings:Default" \
  "Server=localhost;Database=PlataformaCursosDb;User Id=sa;Password=SUASENHA;TrustServerCertificate=True" \
  --project PlataformaCursos.API

dotnet user-secrets set "Jwt:Key" "SUA_CHAVE_SECRETA_COM_MAIS_DE_32_CHARS" \
  --project PlataformaCursos.API

dotnet user-secrets set "Jwt:Issuer" "PlataformaCursosAPI" \
  --project PlataformaCursos.API

dotnet user-secrets set "Jwt:Audience" "PlataformaCursosClient" \
  --project PlataformaCursos.API
```

### Verificar secrets configurados

```bash
dotnet user-secrets list --project PlataformaCursos.API
```

---

## 🗃️ Migrations

### Ordem das migrations

| # | Migration | Descrição |
|---|---|---|
| 1 | `InitialCreate` | Tabelas base (Students, Courses, Enrollments, Identity) |
| 2 | `AddPaymentModule` | Tabela Payments com índices e constraints |

### Comandos

```bash
# Criar migration
dotnet ef migrations add NomeDaMigration --project PlataformaCursos.API

# Aplicar no banco
dotnet ef database update --project PlataformaCursos.API

# Desfazer última migration
dotnet ef migrations remove --project PlataformaCursos.API
```

---

## 🚀 Execução Local

```bash
# Restaurar dependências
dotnet restore

# Rodar a API
dotnet run --project PlataformaCursos.API
```

Documentação disponível em: `https://localhost:7293/scalar/v1`

---

## 🐳 Docker

### Variáveis de ambiente

Copie o arquivo de exemplo e preencha os valores:

```bash
cp .env.example .env
```

Edite o `.env` com suas configurações (nunca suba esse arquivo para o repositório).

### Subir com Docker Compose

```bash
# Subir API + banco
docker compose up -d

# Ver logs
docker compose logs -f api

# Derrubar
docker compose down

# Derrubar e remover volumes (limpa o banco)
docker compose down -v
```

A API ficará disponível em: `http://localhost:8080/scalar/v1`

### Aplicar migrations no container

```bash
docker compose exec api dotnet ef database update
```

---

## 📡 Endpoints de Pagamento

### Criar intenção de pagamento
POST /api/payments
Authorization: Bearer {token}
Idempotency-Key: {chave-unica}
{
"enrollmentId": "guid-da-matricula",
"amount": 299.90,
"currency": "BRL",
"method": 3
}

Métodos: `1=CreditCard, 2=DebitCard, 3=Pix, 4=BankSlip`

### Confirmar pagamento (Admin)
POST /api/payments/{id}/confirm
Authorization: Bearer {token-admin}
{
"transactionId": "TXN-12345"
}

### Marcar como falho (Admin)
POST /api/payments/{id}/fail?reason=Cartão recusado
Authorization: Bearer {token-admin}

### Estornar pagamento (Admin)
POST /api/payments/{id}/refund
Authorization: Bearer {token-admin}

### Buscar pagamento por Id
GET /api/payments/{id}
Authorization: Bearer {token}

### Listar pagamentos do estudante
GET /api/students/{studentId}/payments?page=1&pageSize=10&status=Paid
Authorization: Bearer {token}

### Listar todos (Admin)
GET /api/payments?page=1&pageSize=10&status=Pending&enrollmentId={guid}
Authorization: Bearer {token-admin}

---

## 🏥 Observabilidade

### Health Checks

| Endpoint | Descrição |
|---|---|
| `GET /health` | Status geral da API |
| `GET /health/db` | Status da conexão com o banco |

Resposta esperada:
Healthy

### Logs estruturados

Os logs seguem o padrão:
[LEVEL] Mensagem | CorrelationId={id} | PaymentId={id} | StudentId={id}

Exemplos:
[INFO]  Criando pagamento. StudentId=abc EnrollmentId=xyz Key=key-001
[INFO]  Pagamento confirmado pelo gateway. PaymentId=123 TransactionId=FAKE-...
[WARN]  Pagamento duplicado bloqueado. EnrollmentId=xyz StudentId=abc
[WARN]  Pagamento recusado pelo gateway. PaymentId=123 Reason=Cartão recusado
[ERROR] Falha ao confirmar no gateway. PaymentId=123 Reason=Timeout

### Correlation ID

Toda requisição recebe um `X-Correlation-ID` único no header de resposta.
Envie no request para rastrear em todos os logs:
X-Correlation-ID: meu-id-customizado

### Como ler logs no Docker

```bash
# Logs em tempo real
docker compose logs -f api

# Filtrar por nível
docker compose logs api | grep WARN
docker compose logs api | grep ERROR
```

---

## 🧪 Testes

```bash
# Rodar todos os testes
dotnet test

# Com detalhes
dotnet test --verbosity normal

# Só testes de unidade
dotnet test --filter "FullyQualifiedName~Unit"

# Só testes de integração
dotnet test --filter "FullyQualifiedName~Integration"
```

### Estrutura
PlataformaCursos.Tests
├── Unit
│   └── PaymentDomainTests.cs     ← testa domínio puro (sem banco)
└── Integration
├── ApiFactory.cs             ← sobe API em modo teste
├── TokenHelper.cs            ← gera tokens por papel
└── PaymentsApiTests.cs       ← testa API ponta a ponta

### Resultado atual
Total: 26 | Passou: 26 | Falhou: 0

---

## 🔄 Pipeline CI/CD

O pipeline roda automaticamente via GitHub Actions:

| Gatilho | O que executa |
|---|---|
| Pull Request | restore → build → testes |
| Merge na main | restore → build → testes → artefato → imagem Docker |

### Secrets necessários no GitHub

| Secret | Descrição |
|---|---|
| `DOCKER_USERNAME` | Usuário do Docker Hub |
| `DOCKER_PASSWORD` | Token do Docker Hub |

Configurar em: `Settings → Secrets and variables → Actions`

---

## 🏗️ Padrões e Princípios Aplicados

### SOLID

| Princípio | Como aplicado |
|---|---|
| **SRP** | Cada classe tem uma responsabilidade (Service, Gateway, Validator, Repository separados) |
| **OCP** | Novo gateway = nova classe implementando `IPaymentGateway` sem modificar existente |
| **LSP** | `FakePaymentGateway` substitui qualquer `IPaymentGateway` sem quebrar o sistema |
| **ISP** | Interfaces pequenas e focadas (`IPaymentGateway`, `IPaymentRepository`) |
| **DIP** | `PaymentService` depende de interfaces, nunca de implementações concretas |

### Design Patterns

| Pattern | Onde |
|---|---|
| **Factory Method** | `Payment.Create()` — único ponto de criação válido |
| **Value Object** | `Money` — imutável, compara por valor |
| **Adapter** | `FakePaymentGateway` — isola integração externa |
| **Repository** | `IPaymentRepository` — acesso a dados isolado do domínio |

---

## 🔌 Trocando o Gateway de Pagamento

O gateway é isolado atrás de uma interface:

```csharp
public interface IPaymentGateway
{
    Task<GatewayResult> ProcessAsync(...);
    Task<GatewayResult> ConfirmAsync(string transactionId);
    Task<GatewayResult> RefundAsync(string transactionId);
}
```

**Para trocar para o Stripe por exemplo:**

1. Crie `StripePaymentGateway : IPaymentGateway` na Infrastructure
2. No `Program.cs` troque:

```csharp
// De:
builder.Services.AddScoped<IPaymentGateway, FakePaymentGateway>();

// Para:
builder.Services.AddScoped<IPaymentGateway, StripePaymentGateway>();
```

Nenhuma outra alteração necessária.

---

## 🚨 Erros Comuns e Soluções

| Erro | Causa | Solução |
|---|---|---|
| `JWT:Key não configurada` | User secrets não configurados | Rodar `dotnet user-secrets set` |
| `Cannot open database` | Banco não existe ou senha errada | Verificar connection string e rodar migrations |
| `MissingMethodException` no EF | Versão do EF Tools desatualizada | `dotnet tool update --global dotnet-ef` |
| `26 testes, 0 falhas` esperado mas falhando | Banco de teste não migrado | `ApiFactory` aplica migrations automaticamente |
| Container não sobe | Porta 1433 já em uso | Parar SQL Server local ou mudar a porta no compose |

---

## 📋 Decision Log

| Decisão | Escolha | Motivo |
|---|---|---|
| Framework docs | Scalar (não Swagger) | Compatibilidade com .NET 10 |
| Tipo monetário | `decimal(18,2)` | Precisão financeira |
| Estado inicial | `Pending` no domínio | Regra de negócio, não no banco |
| Idempotência | Header `Idempotency-Key` | Padrão de mercado (Stripe, PagSeguro) |
| Soft delete | `IsDeleted` | Histórico para auditoria |
| Testes de integração | SQL Server local | Fidelidade ao ambiente real |
| Gateway | Adapter simulado | Isola domínio, fácil trocar |
| Observabilidade | ILogger nativo | Sem dependência extra |
| Container | Docker multi-stage | Imagem menor e mais segura |

---

## 👨‍💻 Autor

Hemerval Viana — Analista de Sistemas

Projeto desenvolvido para fins educacionais e portfólio profissional.

## 📄 Licença

Este projeto é livre para fins educacionais.
