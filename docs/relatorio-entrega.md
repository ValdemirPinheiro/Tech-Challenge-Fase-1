# Relatório de Entrega — Tech Challenge Fase 1

> Documento de submissão da Fase 1 do Tech Challenge da pós-graduação FIAP.
> Substitua os campos `<PREENCHER>` pelos dados finais antes da entrega.

## 1. Identificação

| Campo | Valor |
|----|----|
| **Nome do grupo** | `<PREENCHER — ex.: NETT — Time 01>` |
| **Modalidade** | Individual |
| **Data de entrega** | `<PREENCHER — DD/MM/AAAA>` |

## 2. Participantes

| Nome | E-mail | Username Discord |
|----|----|----|
| José Pinheiro | pinheiroone@gmail.com | `<PREENCHER>` |

## 3. Links da entrega

| Item | URL |
|----|----|
| Repositório do código-fonte | `<PREENCHER — ex.: https://github.com/usuario/fcg-fase1>` |
| Documentação DDD (Miro / equivalente) | `<PREENCHER — ou referenciar `docs/event-storming-*.md` se for entregue como markdown>` |
| Vídeo de demonstração (até 15 min) | `<PREENCHER — ex.: https://youtu.be/...>` |

## 4. Escopo entregue

### Funcionalidades obrigatórias
- ✅ Cadastro de usuários (nome, e-mail, senha) com validação de e-mail e senha forte (mín. 8 caracteres com letra, número e caractere especial).
- ✅ Autenticação JWT.
- ✅ Dois níveis de acesso: `User` e `Admin`.
- ✅ Cadastro de jogos pelo administrador, biblioteca pessoal do usuário e promoções.
- ✅ Arquitetura monolítica (Clean Architecture + DDD).

### Requisitos técnicos obrigatórios
- ✅ Entity Framework Core 8 + migrations versionadas (SQL Server LocalDB).
- ✅ API em ASP.NET Core 8 com Controllers MVC.
- ✅ Middleware de tratamento de erros + logs estruturados (Serilog em arquivo + console).
- ✅ Documentação Swagger com integração JWT.
- ✅ Testes unitários (xUnit + FluentAssertions + Moq) cobrindo Domain, Application e mapeamento HTTP.
- ✅ Aplicação de **BDD-style (Given/When/Then)** nos use cases de autenticação (`RegisterUserUseCaseTests`, `LoginUseCaseTests`).
- ✅ Modelagem DDD com **Event Storming** dos fluxos de usuários e jogos (em `docs/`).
- ✅ Princípios de DDD na organização (agregados, value objects, domain exceptions, repositórios na camada Domain).

### Itens opcionais entregues
- ✅ **MongoDB** para a biblioteca de jogos do usuário (estrutura de coleção `library_items` com índice único `(UserId, GameId)`).
- ✅ **GraphQL** com HotChocolate em `/graphql`, expondo filtros, ordenação e projeção sobre o catálogo de jogos.
- ✅ **Domain Storytelling** com 4 histórias de uso ponta-a-ponta em `docs/domain-storytelling.md`.

## 5. Como executar

Instruções completas no `README.md` na raiz do repositório. Resumindo:

```bash
# Pré-requisitos: .NET 8 SDK, SQL Server LocalDB, MongoDB (ou Docker)
docker run -d --name fcg-mongo -p 27017:27017 mongo:7   # opcional p/ biblioteca
dotnet restore
dotnet run --project src/FCG.API
# Swagger: https://localhost:7080/swagger
# Admin demo: admin@fcg.com / Admin@123
```

Para rodar os testes:

```bash
dotnet test
```

## 6. Estrutura de pastas

```
Tech Challenge Fase 1/
├── FCG.sln
├── README.md
├── .gitignore
├── docs/                   ← documentação DDD e este relatório
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

## 7. Roteiro sugerido para o vídeo (15 min)

1. **0:00–1:00** — Apresentação do problema (FCG / Tech Challenge Fase 1) e do escopo entregue.
2. **1:00–3:00** — Tour pela arquitetura: Clean Architecture + DDD, projetos da solução, regras de dependência.
3. **3:00–5:30** — Demonstração do **fluxo de usuário** (Event Storming `docs/event-storming-usuarios.md`): cadastro via Swagger → erro de e-mail/senha fraca → login OK → JWT.
4. **5:30–8:30** — Demonstração do **fluxo de jogos** (Event Storming `docs/event-storming-jogos.md`): admin cria jogo → cria promoção → usuário comum lista jogos com preço corrente.
5. **8:30–10:30** — Aquisição: usuário adquire jogo → consulta biblioteca → mostra estrutura no MongoDB Compass / `db.library_items.find()`.
6. **10:30–12:00** — GraphQL: consulta com filtro por gênero e preço.
7. **12:00–13:30** — Testes (`dotnet test`) e cobertura dos módulos.
8. **13:30–15:00** — Logs estruturados, Swagger, fechamento.

## 8. Referências

- [Documentação Microsoft .NET 8 / EF Core](https://learn.microsoft.com/dotnet)
- [HotChocolate GraphQL](https://chillicream.com/docs/hotchocolate)
- [BCrypt.Net-Next](https://github.com/BcryptNet/bcrypt.net)
- [Serilog](https://serilog.net/)
- DDD: Eric Evans, *Domain-Driven Design*; Vaughn Vernon, *Implementing DDD*.
