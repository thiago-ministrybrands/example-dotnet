# example-dotnet

A minimal ASP.NET Core API backed by Postgres (EF Core) and Redis, used as a
scratch target for trying out `unity-swarm-reviewer` functionality. Not a real
application.

## Run the infrastructure

```sh
docker compose up -d postgres redis
```

Starts Postgres on `5432` and Redis on `6379` with credentials matching
`src/Api/appsettings.json`.

## Run the API

```sh
dotnet run --project src/Api
```

Endpoints:

- `GET /health`
- `GET /items`
- `POST /items` — body `{ "name": "widget" }`
- `GET /items/count` — count cached in Redis for 30s

## Tests

```sh
dotnet test
```

Unit tests cover `ItemService` using EF Core's in-memory provider and a mocked
`ICacheService`, so they run without Postgres/Redis.

## Lint

```sh
./scripts/lint.sh
```

Runs `dotnet format --verify-no-changes` plus an analyzer-enforced build
(`Directory.Build.props` turns on `EnableNETAnalyzers` / code-style
enforcement).

## unity-swarm-reviewer

- `Dockerfile` / `docker-compose.yml`: the full stack (Postgres, Redis, the
  API on `8080`, and an idle `tests` toolchain container). The reviewer's
  Sandbox Runner boots it with `docker compose up --wait`.
- `.swarm-reviewer.yaml`: the Functional-Verifier's checks (unit tests, lint,
  HTTP probes and a page visit), read from the PR head.
- `openspec/specs/`: the spec corpus the Spec-Compliance reviewer checks PRs against.

Try it locally:

```sh
docker compose up --build --wait
docker compose exec tests dotnet test example-dotnet.slnx --no-restore
curl http://localhost:8080/health
```
