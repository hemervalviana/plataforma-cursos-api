# 📚 Plataforma de Cursos — API REST

API REST desenvolvida em .NET 8 para gerenciamento de cursos, estudantes e matrículas, com autenticação baseada em JWT e integração completa com ASP.NET Identity.

Este projeto foi construído seguindo boas práticas de arquitetura, segurança e separação de responsabilidades, servindo como base para sistemas educacionais modernos.

## 🎯 Objetivo

Fornecer uma API segura e escalável para:

- Cadastro e autenticação de usuários (Students)

- Gerenciamento de cursos

- Matrícula de alunos

- Controle de acesso por papéis

- Integração com Identity e JWT

## 🛠️ Tecnologias Utilizadas
.NET 8

- ASP.NET Core Web API

- Entity Framework Core

- ASP.NET Identity

- JWT (JSON Web Token)

- SQL Server

- AutoMapper

- Swagger (OpenAPI)

## 📂 Arquitetura
O projeto segue uma arquitetura em camadas:
```bash
PlataformaCursos.API
│
├── Domain
│   ├── Entities
│   └── Dtos
│
├── Infrastructure
│   └── Data
│
├── Services
│
├── Controllers
│
└── Program.cs
```
Camadas
| Camada         | Responsabilidade              |
| -------------- | ----------------------------- |
| Domain         | Regras de negócio e entidades |
| Infrastructure | Persistência e EF Core        |
| Services       | Regras de aplicação           |
| Controllers    | Exposição dos endpoints       |
| API            | Configuração e pipeline       |

## 🗄️ Modelo de Dados
📌 Diagrama ER

Principais Entidades
Student (Identity)

- Herda de IdentityUser

- Armazena dados de autenticação

- Possui dados customizados
```
FullName
CreatedAt
IsActive
IsDeleted
```
Course
```
Title
Description
Category
Workload
CreatedAt
IsDeleted
```
Enrollment
```
StudentId
CourseId
Status
CreatedAt
IsDeleted
```
## 🔐 Autenticação e Segurança
ASP.NET Identity

- Gerenciamento de usuários

- Hash seguro de senha

- Controle de tentativas

- Email único

JWT

A API utiliza autenticação baseada em tokens JWT.

Cada requisição autenticada deve conter:
```
Authorization: Bearer {token}
```
## ⚙️ Configuração do Ambiente
Requisitos

- .NET SDK 8+

- SQL Server

## 🔑 Configurações Sensíveis (User Secrets)

As informações sensíveis não ficam no repositório.

Utiliza-se dotnet user-secrets.

Configurar Connection String
```
dotnet user-secrets set "ConnectionStrings:Default" "Server=localhost;Database=PlataformaCursosDb;User Id=sa;Password=SUASENHA"
```
Configurar JWT
```
dotnet user-secrets set "Jwt:Key" "SUA_CHAVE_SECRETA"
dotnet user-secrets set "Jwt:Issuer" "PlataformaCursosAPI"
dotnet user-secrets set "Jwt:Audience" "PlataformaCursosClient"
```
Verificar
```
dotnet user-secrets list
```
## 🧩 Entity Framework Core
DbContext Integrado ao Identity
```
public class ApplicationDbContext 
    : IdentityDbContext<Student>
{
    public DbSet<Course> Courses { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }
}
```
Migrations

Criar migration:
```
dotnet ef migrations add InitialCreate
```
Aplicar no banco:
```
dotnet ef database update
```
## 🔐 Políticas do Identity

Configurações aplicadas:
| Regra                | Valor |
| -------------------- | ----- |
| Tamanho mínimo senha | 8     |
| Letra maiúscula      | Sim   |
| Número               | Sim   |
| Email único          | Sim   |
| Lockout              | Sim   |

## 🚀 Execução do Projeto
Restaurar dependências
```
dotnet restore
```
Executar
```
dotnet run
```
Acessar Swagger
https://localhost:7293/swagger

## 📡 Endpoints de Autenticação
```
POST /api/auth/register
```
Body:
```
{
  "fullName": "João Silva",
  "email": "joao@email.com",
  "password": "Senha123"
}
```
Login
```
POST /api/auth/login
```
Body:
```
{
  "email": "joao@email.com",
  "password": "Senha123"
}
```
Retorno
```
{
  "token": "eyJhbGciOiJIUzI1NiIs..."
}
```
## 🔍 Usando Token no Postman

1. Faça login

2. Copie o token

3. Vá em Authorization

4. Tipo: Bearer Token

5. Cole o token

## 📦 Seed Inicial

O sistema executa seed automático para:

- Papéis:

  - Admin

  - Instructor

  - Student

- Usuário administrador
## 🧪 Testes

O projeto foi validado com:

- Swagger

- Postman

- SQL Server Management Studio

- Migrations EF Core

- Logs do Identity
  
## 📈 Status do Projeto
| Etapa                | Status |
| -------------------- | ------ |
| EF Core + SQL Server | ✅      |
| Identity             | ✅      |
| JWT                  | ✅      |
| Migrations           | ✅      |
| Services             | ✅      |
| Autenticação         | ✅      |
| Documentação         | ✅      |

## 📌 Boas Práticas Aplicadas

- Separação de camadas

- DTOs

- AutoMapper

- Soft Delete

- Filtros globais

- Dependency Injection

- Token JWT

- User Secrets

- Clean Architecture

## 👨‍💻 Autor

Hemerval Viana
Analista de Sistemas

Projeto desenvolvido para fins educacionais e portfólio profissional.

## 📄 Licença

Este projeto é livre para fins educacionais.
