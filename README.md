# Plataforma de Cursos - API

## Objetivo
API REST em .NET 8 para gerenciamento de cursos, estudantes e matrículas, com autenticação JWT e controle de acesso por papéis.

## Tecnologias
- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- ASP.NET Identity
- JWT
- SQLite

## Requisitos
- .NET SDK 8+

## Como Executar

```bash
dotnet restore
dotnet run
```
## Acesse Swagger:
```bash
https://localhost:7293/swagger
```

## Configurações

Variáveis sensíveis via User Secrets ou environment variables (ex.: ConnectionStrings:DefaultConnection, JWT:SecretKey).

## Diagrama das Entidades

O diagrama abaixo representa as principais entidades da API de cursos, seus relacionamentos e campos principais:

### Diagrama Visual (Imagem)

![Diagrama de Entidades](https://raw.githubusercontent.com/hemervalviana/plataforma-cursos-api/main/A_diagram_in_the_form_of_an_Entity-Relationship_Di.png)

**Observações da imagem:**

- **Students**: cada aluno referencia um usuário do Identity via `UserId`.  
- **Enrollments**: vincula alunos e cursos, com índice único `(StudentId, CourseId)` para evitar duplicação de matrícula.  
- **Soft delete**: implementado em `Courses`, `Students` e `Enrollments` através do campo `IsDeleted`.  
- **CreatedAt**: inicializado automaticamente em todas as entidades do domínio.  
- **AspNetUsers**: mantém autenticação e e-mail único do Identity.

classDiagram

    class Course {
        +Guid Id
        +string Title
        +string Description
        +string Category
        +int Workload
        +DateTime CreatedAt
        +bool IsDeleted
    }

    class Student {
        +Guid Id
        +string FullName
        +string UserId
        +DateTime CreatedAt
        +bool IsActive
        +bool IsDeleted
    }

    class Enrollment {
        +Guid Id
        +Guid CourseId
        +string StudentId
        +string Status
        +DateTime CreatedAt
        +bool IsDeleted
    }

    class AspNetUser {
        +string Id
        +string Email
        +string UserName
        +string PasswordHash
    }

    Course "1" -- "0..*" Enrollment : "Enrollments"
    Student "1" -- "0..*" Enrollment : "Enrollments"
    Student "1" -- "1" AspNetUser : "UserId → Id"

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
