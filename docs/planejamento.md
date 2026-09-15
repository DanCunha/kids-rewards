# Planejamento do Projeto — KidsRewards

Gerenciador de atividades e recompensas infantis. Este documento traduz o PRD/ADR (`prd-adr.md`) em um plano de execução por fases (Gates), com entregáveis, critérios de aceite e decisões de arquitetura.

---

## 1. Visão Geral

O **KidsRewards** transforma a rotina infantil em uma jornada gamificada: a criança conclui tarefas, acumula pontos e troca por recompensas cadastradas pelos pais. O sistema tem **duas interfaces**:

| Interface | Público | Dispositivo | Inspiração |
| :--- | :--- | :--- | :--- |
| **Painel Admin** | Pais / Responsáveis | Desktop | `.inspo/index.html` (sidebar, KPIs, quick actions) |
| **Dashboard da Criança** | Crianças (4–12 anos) | Mobile First | PRD §5.1 (gamificado, botões grandes) |

## 2. Decisões Confirmadas

| # | Decisão | Escolha |
| :--- | :--- | :--- |
| 1 | Escopo do frontend | Painel Admin **+** Dashboard da Criança |
| 2 | Autenticação | **Sem auth** no MVP (família única) |
| 3 | Estrutura do repositório | **Monorepo raiz** |
| 4 | MongoDB local | **Docker Compose** (porta 27017) |
| 5 | Meta mensal | Acumulado de `Earned` no mês vigente (ver ADR-005) |

## 3. Stack Tecnológica

| Camada | Tecnologia | Versão alvo |
| :--- | :--- | :--- |
| Backend | C# / .NET Core Web API | .NET 8 |
| Banco | MongoDB (`MongoDB.Driver`) | 7.x |
| Frontend | Next.js (App Router) + TypeScript | 14+ |
| Estilo | Tailwind CSS | 3.x |
| Ícones | lucide-react | latest |
| Infra local | Docker Compose | — |
| Testes backend | xUnit | — |

## 4. Estrutura do Monorepo

```
kids-rewards/
├── docker-compose.yml                 # MongoDB local (porta 27017)
├── .gitignore
├── prd-adr.md
├── docs/
│   ├── planejamento.md                # este documento
│   ├── coding-guidelines.md
│   └── adr/
│       └── ADR-005-meta-mensal.md
├── KidsRewards.Api/                   # Backend C# (.NET 8)
│   ├── KidsRewards.Api.csproj
│   ├── Program.cs
│   ├── appsettings.json
│   ├── Configuration/                 # MongoDbSettings.cs
│   ├── Controllers/                   # Children, Activities, Rewards, PointsHistory
│   ├── Domain/
│   │   ├── Entities/                  # Child, Activity, Reward, PointsHistory
│   │   └── Enums/                     # PointOperationType
│   ├── DTOs/                          # ChildDTOs, ActivityDTOs, RewardDTOs, PointsHistoryDTOs
│   ├── Repositories/
│   │   ├── Interfaces/
│   │   └── Mongo/
│   └── Services/
│       ├── Interfaces/
│       └── Implementation/
└── frontend/                          # Next.js 14+ (TS + Tailwind)
    ├── src/
    │   ├── app/
    │   │   ├── page.tsx                # Dashboard da Criança (mobile)
    │   │   └── admin/
    │   │       ├── page.tsx            # Visão geral (KPIs)
    │   │       ├── criancas/page.tsx
    │   │       ├── atividades/page.tsx
    │   │       ├── recompensas/page.tsx
    │   │       └── historico/page.tsx
    │   ├── components/                 # UI reutilizável
    │   └── lib/                        # api.ts, types.ts
    ├── tailwind.config.ts
    └── package.json
```

## 5. Contrato da API (REST)

Base URL de desenvolvimento: `http://localhost:5000` (ou porta do `launchSettings.json`). CORS liberado apenas para `http://localhost:3000`.

### 5.1 Children
| Método | Rota | Descrição |
| :--- | :--- | :--- |
| POST | `/api/children` | Cadastra criança |
| GET | `/api/children` | Lista crianças |
| GET | `/api/children/{id}` | Consulta criança e saldo |
| PUT | `/api/children/{id}/goal` | Atualiza meta mensal |

### 5.2 Activities
| Método | Rota | Descrição |
| :--- | :--- | :--- |
| POST | `/api/activities` | Cria atividade para a criança |
| GET | `/api/activities/child/{childId}` | Lista atividades da criança |
| PATCH | `/api/activities/{id}/complete` | Conclui atividade (soma pontos + histórico) |

### 5.3 Rewards
| Método | Rota | Descrição |
| :--- | :--- | :--- |
| POST | `/api/rewards` | Cria recompensa |
| GET | `/api/rewards` | Lista recompensas ativas |
| POST | `/api/rewards/redeem` | Resgata recompensa (valida saldo + histórico) |
| PATCH | `/api/rewards/{id}` | **Novo:** ativa/desativa recompensa (necessário ao admin) |

### 5.4 PointsHistory
| Método | Rota | Descrição |
| :--- | :--- | :--- |
| GET | `/api/pointshistory/child/{childId}` | Extrato imutável de pontos |

### 5.5 Padrão de Erro
```json
{ "message": "Descrição legível do erro." }
```
Códigos: `200 OK`, `201 Created`, `204 NoContent`, `400 BadRequest`, `404 NotFound`.

## 6. Fases de Execução (Gates)

### Gate 0 — Infraestrutura e Repositório
**Entregáveis**
- `docker-compose.yml` com MongoDB 7 (`kidsrewards-mongo`), volume persistente e `MONGO_INITDB_DATABASE=KidsRewardsDb`.
- `git init`, `.gitignore` (`bin/`, `obj/`, `node_modules/`, `.next/`, `.env*`).
- Solução `KidsRewards.sln` com projeto `KidsRewards.Api`.

**Aceite**
- `docker compose up -d` sobe o banco; `docker ps` mostra o container saudável.
- `dotnet build` compila sem erros.

---

### Gate 1 — Fundamentos de Crianças e Pontuação Base
**Entregáveis**
- `Configuration/MongoDbSettings.cs` + registro de `IMongoClient`/`IMongoDatabase` no DI.
- Entidades `Child`, `Activity`, `Reward`, `PointsHistory` e enum `PointOperationType`.
- DTOs `ChildDTOs` (`CreateChildRequest`, `UpdateChildGoalRequest`, `ChildResponse`).
- `IChildRepository` / `ChildRepository`, `IPointsHistoryRepository` / `PointsHistoryRepository`.
- `IChildService` / `ChildService` e `ChildrenController`.
- Swagger habilitado em desenvolvimento.

**Aceite**
- `POST /api/children` cria criança com saldo `0` e retorna `201`.
- `GET /api/children/{id}` retorna `404` para id inexistente.
- `PUT /api/children/{id}/goal` persiste meta mensal.

---

### Gate 2 — Gestão de Atividades e Acúmulo de Pontos
**Entregáveis**
- `ActivityRepository` com `MarkAsCompletedAsync` (filtro `IsCompleted == false`).
- `ActivityService.CompleteActivityAsync`:
  1. marca atividade como concluída;
  2. incrementa saldo (`Update.Inc` — soma atômica);
  3. grava `PointsHistory` com `Type = Earned`.
- `ActivitiesController` (`POST`, `GET child/{childId}`, `PATCH {id}/complete`).

**Aceite**
- Concluir atividade **não concluída** soma os pontos ao saldo.
- Concluir atividade **já concluída** retorna `400` e não altera saldo.
- Cada conclusão gera **exatamente um** registro de histórico.

---

### Gate 3 — Gestão e Resgate de Recompensas
**Entregáveis**
- `RewardRepository` (`GetAllActiveAsync`, `GetByIdAsync`, `CreateAsync`, `SetActiveAsync`).
- `RewardService.RedeemRewardAsync`:
  1. valida criança e recompensa ativa;
  2. valida `PointsBalance >= RequiredPoints`;
  3. débito **atômico** com filtro `Gte` (impede saldo negativo);
  4. grava `PointsHistory` com `Type = Spent`.
- `RewardsController` (`POST`, `GET`, `POST redeem`, `PATCH {id}`).

**Aceite**
- Resgate com saldo insuficiente retorna `400` e **não** debita.
- Resgate válido debita o saldo e registra histórico.
- **Invariante:** `PointsBalance` nunca fica negativo (garantido na query).

---

### Gate 4 — Frontend Next.js

#### 4a — Scaffold e Camada de Dados
**Entregáveis**
- `create-next-app` com TypeScript + Tailwind + App Router.
- `src/lib/types.ts` espelhando os DTOs (`Child`, `Activity`, `Reward`, `HistoryItem`).
- `src/lib/api.ts` — wrapper `fetch` centralizado, `baseURL` via `NEXT_PUBLIC_API_URL`.
- Configuração de fontes (Inter) e paleta de marca no `tailwind.config.ts`.

**Aceite**
- `npm run dev` sobe a aplicação e o wrapper consome a API real.

#### 4b — Painel Admin (Pais)
**Entregáveis**
- Layout com **sidebar** + **header** + banner de boas-vindas e cards de KPI, inspirado no `.inspo/index.html`.
- `admin/page.tsx`: visão geral (total de crianças, atividades pendentes, pontos em circulação).
- `admin/criancas`: CRUD e definição de meta mensal.
- `admin/atividades`: criação e listagem por criança.
- `admin/recompensas`: catálogo e ativar/desativar.
- `admin/historico`: extrato consolidado por criança.

**Aceite**
- CRUD funcional ponta a ponta contra a API.
- Ativar/desativar recompensa reflete no catálogo da criança.

#### 4c — Dashboard da Criança (Mobile First)
**Entregáveis**
- Header com gradiente, avatar, nome/idade e saldo animado.
- Barra de progresso da **meta mensal** (acumulado `Earned` do mês — ver ADR-005).
- Abas **Tarefas / Prêmios / Histórico** (PRD §5.1) com dados reais.
- Botões grandes, feedback visual imediato e estados de carregamento/erro.

**Aceite**
- Concluir tarefa atualiza saldo e extrato na tela.
- Botão "Resgatar" desabilitado quando saldo é insuficiente (UX) e erro tratado se a API recusar.

---

### Gate 5 — Qualidade e Documentação
**Entregáveis**
- Testes xUnit dos Services: conclusão de atividade, resgate com/sem saldo, idempotência.
- `README.md` com instruções de execução (Docker, backend, frontend).
- `npm run lint` e `npm run build` sem erros; `dotnet build` limpo.

**Aceite**
- `dotnet test` verde.
- Build de produção do Next.js conclui com sucesso.

## 7. Regras de Negócio Invariantes

1. Saldo inicial de toda criança é `0`.
2. Apenas atividades ativas e não concluídas geram pontos.
3. Resgate exige `saldo >= RequiredPoints`; **saldo negativo é proibido**.
4. Toda mutação de pontos gera registro **imutável** em `PointsHistory`.
5. A meta mensal considera os pontos **ganhos no mês vigente**.

## 8. Comandos de Verificação

| Ação | Comando |
| :--- | :--- |
| Subir banco | `docker compose up -d` |
| Backend | `dotnet run --project KidsRewards.Api` |
| Testes backend | `dotnet test` |
| Frontend (dev) | `npm run dev` (em `frontend/`) |
| Lint frontend | `npm run lint` |
| Build frontend | `npm run build` |

## 9. Riscos e Mitigações

| Risco | Mitigação |
| :--- | :--- |
| Saldo negativo por corrida de concorrência | Débito atômico com filtro `Gte` no Mongo |
| Histórico inconsistente com saldo | Gravação de histórico sempre no mesmo fluxo do Service |
| Divergência de contrato API/Frontend | Tipos em `src/lib/types.ts` espelhando os DTOs |
| Duplicidade ao concluir atividade | Filtro `IsCompleted == false` na atualização |

## 10. ADRs Complementares

- **ADR-002:** Monorepo raiz (backend + frontend versionados juntos).
- **ADR-003:** Duas interfaces (admin desktop + criança mobile) na mesma app Next.js.
- **ADR-004:** Sem autenticação no MVP (família única, uso local).
- **ADR-005:** Meta mensal calculada pelo acumulado de `Earned` no mês vigente — ver `docs/adr/ADR-005-meta-mensal.md`.
