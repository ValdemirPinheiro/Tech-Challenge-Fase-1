# Domain Storytelling — FCG

Domain Storytelling captura cenários reais de uso da plataforma na linguagem de negócio. Cada história representa uma interação ponta-a-ponta entre **atores**, **ações** e **objetos do domínio**.

---

## História 1 — *"O aluno cria sua conta na FCG"*

```mermaid
sequenceDiagram
    autonumber
    actor Aluno as Aluno (visitante)
    participant API as FCG.API
    participant Auth as RegisterUserUseCase
    participant DB as Identity (SQL)
    participant Hash as BCryptPasswordHasher

    Aluno->>API: POST /api/auth/register {name, email, password}
    API->>Auth: ExecuteAsync(request)
    Auth->>Auth: Email.Create(...) e Password.Create(...)
    Auth->>DB: EmailExistsAsync? → false
    Auth->>Hash: Hash(password) → "$2a$..."
    Auth->>DB: AddAsync(User) + SaveChanges
    DB-->>Auth: Persistido
    Auth-->>API: Result<UserResponse>.Success
    API-->>Aluno: 201 Created (id, email, role: User)
```

**Pano de fundo:** o aluno entra no portal pela primeira vez, fornece nome, e-mail acadêmico e uma senha forte. O sistema valida, garante que o e-mail é único e cria a conta com role *User*.

---

## História 2 — *"O aluno faz login e adquire seu primeiro jogo"*

```mermaid
sequenceDiagram
    autonumber
    actor Aluno as Aluno
    participant API as FCG.API
    participant Login as LoginUseCase
    participant Acquire as AcquireGameUseCase
    participant SQL as Catalog (SQL)
    participant Mongo as Library (Mongo)
    participant JWT as JwtTokenService

    Aluno->>API: POST /api/auth/login {email, password}
    API->>Login: ExecuteAsync
    Login->>SQL: GetByEmailAsync → User
    Login->>JWT: GenerateToken(user) → token
    Login-->>API: AuthResponse
    API-->>Aluno: 200 OK + JWT

    Aluno->>API: POST /api/games/{id}/acquire (Authorization: Bearer ...)
    API->>Acquire: ExecuteAsync(userId, gameId)
    Acquire->>SQL: GetByIdAsync(gameId) → Game (ativo)
    Acquire->>Mongo: UserOwnsGameAsync? → false
    Acquire->>Mongo: AddAsync(LibraryItem)
    Acquire-->>API: AcquireGameResponse
    API-->>Aluno: 201 Created (LibraryItemId, GameTitle, PricePaid)
```

**Pano de fundo:** após autenticar, o aluno escolhe um jogo do catálogo e finaliza a aquisição. O sistema impede aquisições duplicadas pelo índice único `(UserId, GameId)` no MongoDB.

---

## História 3 — *"O administrador cadastra um jogo e cria uma promoção de lançamento"*

```mermaid
sequenceDiagram
    autonumber
    actor Admin as Administrador
    participant API as FCG.API
    participant Catalog as CreateGameUseCase
    participant Promo as CreatePromotionUseCase
    participant SQL as Catalog (SQL)

    Admin->>API: POST /api/games (com JWT de Admin)
    API->>Catalog: ExecuteAsync(title, price, ...)
    Catalog->>SQL: TitleExistsAsync? → false
    Catalog->>SQL: AddAsync(Game) + SaveChanges
    Catalog-->>API: GameResponse
    API-->>Admin: 201 Created

    Admin->>API: POST /api/promotions { gameId, 30%, hoje, +7d }
    API->>Promo: ExecuteAsync
    Promo->>SQL: GetByIdAsync(gameId) → Game
    Promo->>SQL: AddAsync(Promotion) + SaveChanges
    Promo-->>API: PromotionResponse
    API-->>Admin: 201 Created
```

**Pano de fundo:** apenas usuários com `Role=Admin` conseguem chegar nestes endpoints (forçado por `[Authorize(Roles=Admin)]`). O preço corrente do jogo passa a refletir automaticamente o desconto durante a vigência da promoção.

---

## História 4 — *"O aluno quer ver promoções e filtrar jogos por gênero"*

O aluno abre a vitrine, observa as promoções ativas e filtra apenas RPGs com preço inferior a R$ 100. Para a vitrine REST simples ele consome `GET /api/promotions`. Para uma vitrine mais sofisticada, o frontend usa GraphQL:

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

GraphQL (HotChocolate) é exposto em `POST /graphql` e Banana Cake Pop fica em `GET /graphql/`. Filtros, ordenações e projeções são gerados automaticamente em cima do `IQueryable<Game>` exposto pelo repositório.
