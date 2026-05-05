# Setup local — primeiros passos

Este guia complementa o `README.md` com os comandos exatos que você (ou qualquer outra pessoa) precisa rodar uma única vez para colocar o FCG no ar.

## 1. Limpar a pasta `.git/` parcial e inicializar o repositório

Há uma pasta `.git/` parcial que ficou no projeto (resíduo de uma tentativa de inicialização). Pelo PowerShell elevado (Windows), execute na raiz `C:\Users\USUARIO\Dev\Tech Challenge Fase 1`:

```powershell
# Apaga a .git parcial (use Remove-Item via Explorer se preferir)
Remove-Item -Recurse -Force .\.git

# Inicializa Git limpo e faz o primeiro commit
git init -b main
git add .
git commit -m "feat: tech challenge fase 1 — FCG API REST (Clean Architecture + DDD)"
```

Se ainda for criar o repositório no GitHub:

```powershell
# Após criar o repo vazio em https://github.com/<seu-usuario>/<seu-repo>
git remote add origin https://github.com/<seu-usuario>/<seu-repo>.git
git push -u origin main
```

## 2. Pré-requisitos

| Ferramenta | Versão | Como instalar |
|----|----|----|
| .NET 8 SDK | 8.0.x | <https://dotnet.microsoft.com/download/dotnet/8.0> |
| SQL Server LocalDB | 2019+ | já vem com Visual Studio 2022 |
| Docker (opcional) | — | <https://www.docker.com/products/docker-desktop> |
| MongoDB | 7.x | container ou instalação nativa |

Para checar:

```powershell
dotnet --version           # deve mostrar 8.0.x
sqllocaldb info            # deve listar instâncias (MSSQLLocalDB)
docker --version           # opcional
```

## 3. Subir o MongoDB (necessário para biblioteca / aquisição de jogos)

### Via Docker (recomendado)

```powershell
docker run -d --name fcg-mongo -p 27017:27017 mongo:7
```

Para parar / iniciar:
```powershell
docker stop fcg-mongo
docker start fcg-mongo
```

### Sem Docker

Baixe o MongoDB Community Server, instale como serviço Windows e garanta que esteja rodando na porta 27017.

## 4. Restaurar, compilar e rodar

```powershell
cd "C:\Users\USUARIO\Dev\Tech Challenge Fase 1"
dotnet restore
dotnet build
dotnet run --project src/FCG.API
```

Em **Development**, o `Program.cs` automaticamente:
1. Cria o banco SQL via `db.Database.Migrate()`.
2. Cadastra um administrador padrão (`admin@fcg.com` / `Admin@123`).

A API sobe em:
- HTTP: <http://localhost:5080>
- HTTPS: <https://localhost:7080>
- Swagger: <https://localhost:7080/swagger>
- GraphQL (Banana Cake Pop): <https://localhost:7080/graphql/>

## 5. Rodar os testes

```powershell
dotnet test
```

## 6. Criar novas migrations (caso modifique o domínio)

```powershell
dotnet tool install --global dotnet-ef          # apenas uma vez
dotnet ef migrations add <NomeDaMigration> `
  --project src/FCG.Infrastructure `
  --startup-project src/FCG.API
dotnet ef database update `
  --project src/FCG.Infrastructure `
  --startup-project src/FCG.API
```

## 7. Roteiro rápido pelo Swagger (smoke test)

1. `POST /api/auth/login` com `admin@fcg.com` / `Admin@123` → copie o token.
2. Clique em **Authorize** no Swagger e cole `Bearer <token>`.
3. `POST /api/games` para cadastrar um jogo.
4. `POST /api/promotions` (opcional) para criar uma promoção.
5. `POST /api/auth/register` para criar um usuário comum.
6. Faça login com o usuário comum e clique em **Authorize** novamente com o token dele.
7. `POST /api/games/{id}/acquire` para adquirir um jogo.
8. `GET /api/library/me` para ver a biblioteca.

Pronto — todos os requisitos da Fase 1 demonstrados em ~3 minutos.
