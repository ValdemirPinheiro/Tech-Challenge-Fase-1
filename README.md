# FCG — FIAP Cloud Games (Tech Challenge — Fase 1)

API REST em **.NET 8** para cadastro de usuários, gestão de catálogo de jogos, promoções e biblioteca pessoal de jogos adquiridos. Esta é a **Fase 1** do Tech Challenge da pós-graduação FIAP — o MVP em monolito que serve de base para fases futuras (matchmaking e gestão de servidores).

## Sumário

- [Stack](#stack)
- [Arquitetura](#arquitetura)
- [Funcionalidades implementadas](#funcionalidades-implementadas)
- [Pré-requisitos](#pré-requisitos)
- [Como executar localmente](#como-executar-localmente)
- [Endpoints REST principais](#endpoints-rest-principais)
- [GraphQL](#graphql)
- [Testes](#testes)
- [Estrutura do projeto](#estrutura-do-projeto)
- [Documentação DDD](#documentação-ddd)

## Stack

| Camada | Tecnologia |
|----|----|
| Runtime | .NET 8 |
| Web API | ASP.NET Core 8 (Controllers MVC) |
| ORM | Entity Framework Core 8 (SQL Server LocalDB) |
| NoSQL | MongoDB 7 (driver oficial 2.28) |
| Autenticação | JWT Bearer (`System.IdentityModel.Tokens.Jwt`) |
| Hash de senha | BCrypt.Net-Next (work factor 12) |
| GraphQL | HotChocolate 13 (filtering, sorting, projection) |
| Logs estruturados | Serilog (console + arquivo rolling diário) |
| Documentação | Swashbuckle (Swagger) com suporte a JWT |
| Testes | xUnit + FluentAssertions + Moq |

## Arquitetura

Clean Architecture + DDD com 4 projetos:

```
src/
├── FCG.Domain          → entidades, VOs, regras puras (zero dependências externas)
├── FCG.Application     → use cases, DTOs, abstrações de serviços
├── FCG.Infrastructure  → EF Core, MongoDB, BCrypt, JWT, repositórios
└── FCG.API             → controllers, middlewares, Swagger, GraphQL, DI raiz
```

A regra da dependência aponta sempre para o domínio. Detalhes em [`docs/arquitetura.md`](docs/arquitetura.md).

## Funcionalidades implementadas

### Obrigatórias
- ✅ Cadastro de usuário (nome, e-mail, senha) com **validação de e-mail** (regex) e **senha forte** (>= 8 caracteres com letra, número e caractere especial).
- ✅ **Autenticação JWT** com 2 níveis de acesso: `User` e `Admin`.
- ✅ Cadastro / atualização / desativação de **jogos** (Admin).
- ✅ Criação de **promoções** com vigência e desconto (Admin).
- ✅ Aquisição de jogo e listagem de **biblioteca pessoal** (autenticado).
- ✅ Administração de usuários — listar, alterar role, remover (Admin).
- ✅ **Entity Framework Core** com **migrations** versionadas.
- ✅ **Middleware** central de erros + logs estruturados (Serilog).
- ✅ **Swagger** com integração JWT.
- ✅ **Monolito modular** (Clean Architecture).
- ✅ **Testes unitários** com xUnit, FluentAssertions e Moq (BDD-style em Auth).

### Opcionais entregues
- ✅ **MongoDB** para persistir a biblioteca de jogos do usuário.
- ✅ **GraphQL** (HotChocolate) para consultas dinâmicas no catálogo.
- ✅ **Domain Storytelling** (documentação adicional em `docs/domain-storytelling.md`).

## Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- **SQL Server LocalDB** (vem com Visual Studio 2022 ou via "SQL Server Express LocalDB")
- **MongoDB local** (porta 27017) — opcional, necessário para a biblioteca:
  - Forma rápida via Docker: `docker run -d --name fcg-mongo -p 27017:27017 mongo:7`
  - Ou instalação nativa: <https://www.mongodb.com/try/download/community>

## Como executar localmente

### 1. Restaurar e compilar

```bash
dotnet restore
dotnet build
```

### 2. Aplicar migrations (criar o banco SQL)

A aplicação roda `db.Database.Migrate()` automaticamente em **Development**, então apenas iniciar a API já cria o banco. Se preferir rodar manualmente:

```bash
dotnet tool install --global dotnet-ef    # uma única vez
dotnet ef database update \
  --project src/FCG.Infrastructure \
  --startup-project src/FCG.API
```

### 3. Subir o MongoDB (opcional, para biblioteca/aquisições)

```bash
docker run -d --name fcg-mongo -p 27017:27017 mongo:7
```

### 4. Executar a API

```bash
dotnet run --project src/FCG.API
```

A API sobe em `http://localhost:5080` (HTTP) e/ou `https://localhost:7080` (HTTPS).
- Swagger UI: `https://localhost:7080/swagger`
- GraphQL (Banana Cake Pop): `https://localhost:7080/graphql`

### 5. Usuário administrador semeado automaticamente

Em ambiente Development, na primeira execução o sistema cria um administrador de demonstração:

| Campo | Valor |
|----|----|
| E-mail | `admin@fcg.com` |
| Senha | `Admin@123` |
| Role | Admin |

Use-o para gerar um token via `POST /api/auth/login` e exercitar os endpoints administrativos.

## Endpoints REST principais

### Autenticação (público)
| Método | Rota | Descrição |
|----|----|----|
| POST | `/api/auth/register` | Cadastra novo usuário (role User) |
| POST | `/api/auth/login` | Retorna JWT |

### Usuários (Admin)
| Método | Rota | Descrição |
|----|----|----|
| GET | `/api/users` | Lista usuários |
| POST | `/api/users/admin` | Cria outro administrador |
| PUT | `/api/users/{id}/role` | Altera role (`User`/`Admin`) |
| DELETE | `/api/users/{id}` | Remove usuário |

### Jogos
| Método | Rota | Acesso | Descrição |
|----|----|----|----|
| GET | `/api/games` | público | Lista catálogo ativo (com preço corrente) |
| POST | `/api/games` | Admin | Cadastra jogo |
| PUT | `/api/games/{id}` | Admin | Atualiza jogo |
| DELETE | `/api/games/{id}` | Admin | Soft-delete (desativa) |
| POST | `/api/games/{id}/acquire` | autenticado | Adquire jogo (vai para biblioteca) |

### Promoções
| Método | Rota | Acesso | Descrição |
|----|----|----|----|
| GET | `/api/promotions` | público | Lista promoções vigentes |
| POST | `/api/promotions` | Admin | Cria promoção |

### Biblioteca
| Método | Rota | Acesso | Descrição |
|----|----|----|----|
| GET | `/api/library/me` | autenticado | Biblioteca do usuário logado (Mongo) |

## GraphQL

Endpoint: `POST /graphql`. Banana Cake Pop em `GET /graphql/`.

Exemplo de consulta com filtros, ordenação e projeção:

```graphql
query {
  games(
    where: {
      genre: { eq: "RPG" }
      price: { lte: 100 }
      isActive: { eq: true }
    }
    order: [{ price: ASC }]
  ) {
    id
    title
    price
    releaseDate
  }
}
```

## Testes

```bash
dotnet test
```

Estão organizados em três projetos:
- `tests/FCG.Domain.Tests` — entidades e value objects.
- `tests/FCG.Application.Tests` — use cases (BDD-style nos módulos de Auth).
- `tests/FCG.API.Tests` — mapeamento `Result → IActionResult`.

## Estrutura do projeto

```
Tech Challenge Fase 1/
├── FCG.sln
├── README.md
├── .gitignore
├── docs/
│   ├── arquitetura.md
│   ├── event-storming-usuarios.md
│   ├── event-storming-jogos.md
│   ├── domain-storytelling.md
│   └── relatorio-entrega.md
├── src/
│   ├── FCG.Domain/
│   ├── FCG.Application/
│   ├── FCG.Infrastructure/
│   └── FCG.API/
└── tests/
    ├── FCG.Domain.Tests/
    ├── FCG.Application.Tests/
    └── FCG.API.Tests/
```

## Documentação DDD

- [`docs/arquitetura.md`](docs/arquitetura.md) — visão geral, bounded contexts, agregados, linguagem ubíqua.
- [`docs/event-storming-usuarios.md`](docs/event-storming-usuarios.md) — Event Storming do fluxo de usuários (cadastro, login, mudança de role).
- [`docs/event-storming-jogos.md`](docs/event-storming-jogos.md) — Event Storming do fluxo de jogos (cadastro, promoção, aquisição).
- [`docs/domain-storytelling.md`](docs/domain-storytelling.md) — histórias de uso ponta-a-ponta.

Os diagramas estão em **Mermaid**, renderizam direto no GitHub. Para visualizar localmente, qualquer extensão Mermaid em VS Code resolve.

## Licença

Projeto educacional — pós-graduação FIAP / Tech Challenge.
