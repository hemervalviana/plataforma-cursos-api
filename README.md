# Plataforma de Cursos - API
## Objetivo

API REST desenvolvida em .NET 8 para gerenciamento de cursos, estudantes e matrículas, com autenticação JWT e controle de acesso por papéis.

## Tecnologias

- .NET 8

- ASP.NET Core Web API

- Entity Framework Core

- ASP.NET Identity

- JWT

- SQL Server (local para desenvolvimento)

## Requisitos

- .NET SDK 8+

- SQL Server local (ou remoto)

## Como Executar
```bash
dotnet restore
dotnet run
```
## Acesse:
https://localhost:7293/swagger

## Configurações

As configurações sensíveis (connection string, JWT key) devem ser definidas via variáveis de ambiente ou User Secrets:
```bash
dotnet user-secrets set "ConnectionStrings:Default" "Server=localhost;Database=PlataformaCursosDb;User Id=sa;Password=SuaSenha"
dotnet user-secrets set "Jwt:Key" "sua-chave-secreta"
dotnet user-secrets set "Jwt:Issuer" "PlataformaCursosAPI"
dotnet user-secrets set "Jwt:Audience" "PlataformaCursosClient"
```
## Estrutura do Banco de Dados
Principais Entidades

- Courses: cursos oferecidos

- Students: alunos, integrados ao Identity (AspNetUsers)

- Enrollments: matrículas de alunos em cursos

Campos comuns:

- CreatedAt → inicializado automaticamente

- IsDeleted → soft delete

Relacionamentos:

- Enrollment.StudentId → AspNetUsers.Id

- Enrollment.CourseId → Courses.Id

- Índice único em (StudentId, CourseId) para evitar duplicação de matrícula

- UserName e Email únicos em AspNetUsers

## Diagrama das Entidades

O diagrama abaixo representa as principais entidades da API de cursos, seus relacionamentos e campos principais:

### Diagrama Visual (Imagem)

![Diagrama de Entidades](https://raw.githubusercontent.com/hemervalviana/plataforma-cursos-api/main/A_diagram_in_the_form_of_an_Entity-Relationship_Di.png)

Diagrama de Entidades (Mermaid)
```erDiagram
    %% =====================
    %% Entidades do domínio
    %% =====================
    COURSES {
        GUID Id PK "Chave primária"
        string Title "Único, obrigatório"
        string Description
        string Category
        int Workload
        datetime CreatedAt
        bool IsDeleted
    }

    ENROLLMENTS {
        GUID Id PK
        GUID CourseId FK
        string StudentId FK
        string Status
        datetime CreatedAt
        bool IsDeleted
    }

    %% =====================
    %% Identity
    %% =====================
    ASPNETUSERS {
        string Id PK "IdentityUser"
        string UserName
        string NormalizedUserName
        string Email
        string NormalizedEmail
        string FullName
        datetime CreatedAt
        bool IsActive
        bool IsDeleted
        string PasswordHash
        string SecurityStamp
        string ConcurrencyStamp
        bool EmailConfirmed
        bool LockoutEnabled
        int AccessFailedCount
    }

    ASPNETROLES {
        string Id PK
        string Name
        string NormalizedName
        string ConcurrencyStamp
    }

    ASPNETUSERROLES {
        string UserId FK
        string RoleId FK
    }

    %% =====================
    %% Relacionamentos
    %% =====================
    STUDENTS ||--|| ASPNETUSERS : "Id"
    ENROLLMENTS }|--|| COURSES : "CourseId"
    ENROLLMENTS }|--|| ASPNETUSERS : "StudentId"
    COURSES ||--|{ ENROLLMENTS : "Enrollments"
    ASPNETUSERS ||--|{ ENROLLMENTS : "Enrollments"
    ASPNETUSERS }|--|{ ASPNETUSERROLES : "UserRoles"
    ASPNETROLES }|--|{ ASPNETUSERROLES : "UserRoles"
```
## Observações

- Soft delete: implementado via IsDeleted em Courses, Students e Enrollments.

- FK compatíveis: tipos corretos para Identity (string) e cursos (Guid).

- Seed inicial: você pode criar papéis (Admin, Instructor, Student) e um usuário admin via seeder.

- Segurança: JWT configurado via User Secrets ou variáveis de ambiente, sem dados sensíveis no repositório.

- Migration inicial: dotnet ef migrations add InitialCreate e dotnet ef database update para criar o banco.



## Observações:

- Cada Student referencia um usuário do Identity via UserId.

- Um Enrollment conecta um aluno a um curso; índice único (StudentId + CourseId) evita duplicação.

- Soft delete implementado em Courses, Students e Enrollments (IsDeleted).

- CreatedAt inicializa automaticamente.

- Course.Title e Student.UserId possuem restrições únicas via EF Core.

## Estrutura de Pastas
```bash
/PlataformaCursos.API
|-- /Domain/Entities
|    |-- Course.cs
|    |-- Student.cs
|    |-- Enrollment.cs
|-- /Infrastructure/Data
|    |-- ApplicationDbContext.cs
|    |-- /Configurations
|         |-- CourseConfiguration.cs
|         |-- StudentConfiguration.cs
|         |-- EnrollmentConfiguration.cs
|-- /Controllers
|-- Program.cs
|-- appsettings.json
|-- A_diagram_in_the_form_of_an_Entity-Relationship_Di.png
```
