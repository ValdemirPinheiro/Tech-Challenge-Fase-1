# Arquitetura — FCG (FIAP Cloud Games) — Fase 1

## Visão geral

O sistema é um **monolito modular** em ASP.NET Core 8, organizado segundo os princípios de **Clean Architecture** e **Domain-Driven Design (DDD)**. O monolito atende ao requisito da Fase 1 e ao mesmo tempo prepara o projeto para uma futura quebra em microsserviços (matchmaking e gerenciamento de servidores) sem refatoração disruptiva: o domínio fica isolado e as fronteiras de contexto já estão delineadas.

## Camadas

```
┌────────────────────────────────────────────────────────────┐
│                          FCG.API                           │ ← Controllers, Swagger,
│      (ASP.NET Core 8 Controllers + GraphQL + Serilog)      │   middlewares, GraphQL,
└──────────────────────────┬─────────────────────────────────┘   JWT auth, DI raiz
                           │
                           ▼
┌────────────────────────────────────────────────────────────┐
│                       FCG.Application                      │ ← Use cases, DTOs,
│             (Use Cases / Application Services)             │   Result<T>, abstrações
└──────────────────────────┬─────────────────────────────────┘   (IPasswordHasher,
                           │                                     IJwtTokenService,
                           ▼                                     ICurrentUserService)
┌────────────────────────────────────────────────────────────┐
│                         FCG.Domain                         │ ← Entidades (User, Game,
│   (Entidades, VOs, Aggregate Roots, Domain Exceptions)     │   Promotion, LibraryItem),
│                                                            │   Value Objects (Email,
│                                                            │   Password), regras de
│                                                            │   negócio puras.
└──────────────────────────▲─────────────────────────────────┘
                           │ implementa interfaces de repositório
                           │
┌──────────────────────────┴─────────────────────────────────┐
│                     FCG.Infrastructure                     │ ← EF Core (SQL Server),
│  (EF Core / Mongo / BCrypt / JWT / Repositórios)           │   MongoDB driver,
│                                                            │   BCryptPasswordHasher,
└────────────────────────────────────────────────────────────┘   JwtTokenService
```

## Regra da dependência

A flecha aponta sempre para o domínio. Domain não depende de nada externo, Application depende apenas de Domain, Infrastructure implementa as interfaces de Domain/Application, e API compõe tudo.

## Bounded Contexts (Fase 1)

A FCG é grande o suficiente para se decompor em vários *bounded contexts*. Na Fase 1 implementamos três deles, dentro de um único monolito:

| Contexto | Responsabilidade | Persistência |
|----|----|----|
| **Identity & Access** | Cadastro, autenticação e autorização de usuários | SQL Server (EF) |
| **Catalog** | Jogos e promoções | SQL Server (EF) |
| **Library** | Biblioteca pessoal de jogos do usuário | MongoDB |

A escolha de MongoDB para *Library* é deliberada: o agregado `LibraryItem` é orientado a documento (lê-se quase sempre por `userId`), tem volumetria potencialmente alta e nunca participa de joins relacionais com outros agregados — o que torna NoSQL um casamento natural.

## Aggregates e Entidades

| Aggregate Root | Entidades / VOs | Invariantes principais |
|----|----|----|
| **User** | Email (VO), Password (VO) | E-mail único, válido; senha forte (8+ chars, letra+número+especial); role pertence ao enum |
| **Game** | Promotion (entidade interna) | Título obrigatório (>= 2 chars); preço >= 0; soft-delete preserva biblioteca |
| **Promotion** | — | Desconto entre 0 e 90%; data fim > data início |
| **LibraryItem** | — | Combinação `(UserId, GameId)` única; preço pago >= 0 |

## Linguagem ubíqua

| Termo | Significado dentro da FCG |
|----|----|
| **Usuário** | Conta de quem joga; possui um `Role` (User ou Admin) |
| **Administrador** | Usuário com privilégios de gestão (cria jogos, promoções, gerencia outros usuários) |
| **Jogo / Game** | Item do catálogo vendido na FCG |
| **Promoção** | Aplicação temporária de desconto sobre um jogo |
| **Biblioteca** | Conjunto de jogos adquiridos por um usuário |
| **Aquisição** | Operação que adiciona um jogo à biblioteca de um usuário |
| **Catálogo** | Conjunto de jogos ativos disponíveis para aquisição |

## Decisões arquiteturais relevantes

1. **Monolito modular**: requisito da Fase 1, mas com fronteiras já desenhadas.
2. **Result<T> em vez de exceções para fluxo esperado**: mantém *Application* limpo e o controller mapeia para HTTP de forma centralizada (`ResultExtensions`).
3. **DomainException como única exceção de domínio**: capturada pelo `ExceptionHandlingMiddleware`.
4. **Soft delete em Game**: preservar histórico de aquisições.
5. **JWT stateless**: autenticação compatível com escala horizontal futura.
6. **Migrations versionadas**: facilita reprodutibilidade e rollback.
7. **GraphQL coexistindo com REST**: REST cobre *commands* e queries simples; GraphQL atende leituras avançadas (filtros, projeções) sobre o catálogo de jogos.
