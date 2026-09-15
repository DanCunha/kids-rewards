# KidsRewards

Gerenciador de atividades e recompensas infantis. As crianças concluem tarefas, acumulam pontos e trocam por recompensas cadastradas pelos pais — tudo de forma gamificada.

## Stack

| Camada | Tecnologia |
| :--- | :--- |
| Backend | C# (.NET 8 Web API) + MongoDB |
| Frontend | Next.js (App Router) + TypeScript + Tailwind CSS (Mobile First) |
| Banco local | MongoDB 7 via Docker Compose |
| Testes | xUnit + Moq |

## Estrutura

```
kids-rewards/
├── docker-compose.yml          # MongoDB local (porta 27017)
├── KidsRewards.Api/            # Backend (Controllers → Services → Repositories)
├── KidsRewards.Api.Tests/      # Testes unitários dos Services
├── frontend/                   # Next.js (painel admin + dashboard da criança)
└── docs/                       # planejamento.md, coding-guidelines.md, ADRs
```

## Como executar

### 1. Banco de dados (Docker)

```bash
docker compose up -d
```

Sobe o MongoDB 7 no container `kidsrewards-mongo` (db `KidsRewardsDb`, porta `27017`).

### 2. Backend (API)

```bash
dotnet run --project KidsRewards.Api
```

- API em `http://localhost:5183` (configuração em `Properties/launchSettings.json`).
- Swagger em `http://localhost:5183/swagger` (apenas em desenvolvimento).
- Configuração do Mongo em `appsettings.json` → `MongoDbSettings`.

### 3. Frontend

```bash
cd frontend
npm install
npm run dev
```

- Abra `http://localhost:3000` — **dashboard da criança** (mobile-first, com seletor de criança).
- Abra `http://localhost:3000/admin` — **painel dos pais** (sidebar, KPIs, CRUD).

> A API é consumida via `NEXT_PUBLIC_API_URL` (default `http://localhost:5183/api`). CORS liberado apenas para `http://localhost:3000`.

## Testes

```bash
dotnet test
```

## Scripts úteis

| Comando | Descrição |
| :--- | :--- |
| `dotnet build` | Compila a solução |
| `dotnet test` | Roda os testes xUnit |
| `npm run lint` | Lint do frontend |
| `npm run build` | Build de produção do frontend |

## Endpoints principais (API)

| Recurso | Endpoints |
| :--- | :--- |
| Crianças | `POST/GET /api/children`, `GET /api/children/{id}`, `PUT /api/children/{id}/goal` |
| Atividades | `POST /api/activities`, `GET /api/activities/child/{childId}`, `PATCH /api/activities/{id}/complete` |
| Recompensas | `POST/GET /api/rewards`, `GET /api/rewards/all`, `POST /api/rewards/redeem`, `PATCH /api/rewards/{id}` |
| Histórico | `GET /api/pointshistory/child/{childId}` |

## Regras de negócio

1. Saldo inicial de toda criança é `0`.
2. Apenas atividades ativas e não concluídas geram pontos.
3. Resgate exige `saldo >= RequiredPoints`; **saldo negativo é proibido** (débito atômico no Mongo).
4. Toda mutação de pontos gera registro **imutável** em `PointsHistory`.
5. A meta mensal considera os pontos **ganhos no mês vigente** (acumulado `Earned`).

## Documentação

- `docs/planejamento.md` — planejamento por Gates e contrato da API.
- `docs/coding-guidelines.md` — convenções de código (C#, Next.js, Git).
- `docs/adr/` — decisões de arquitetura (ADR-005: meta mensal por acúmulo mensal).