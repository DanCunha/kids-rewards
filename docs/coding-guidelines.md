# Coding Guidelines — KidsRewards

Convenções e padrões de código para o projeto KidsRewards (backend C# .NET 8 + MongoDB, frontend Next.js + TypeScript + Tailwind). Este documento complementa a seção 3 do `prd-adr.md` e deve ser seguido em todo o código do repositório.

---

## 1. Princípios Gerais

1. **SOLID e separação rígida de responsabilidades:**
   - **Controllers** → recebem DTOs, validam entrada básica e retornam códigos HTTP. **Nenhuma regra de negócio.**
   - **Services** → centralizam regras de negócio, validações de domínio e orquestram repositórios.
   - **Repositories** → abstraem o acesso ao MongoDB (`IMongoCollection<T>`), assíncronos.
2. **DTOs estritos:** toda entrada/saída de API usa DTOs (records no C#). O domínio nunca é exposto diretamente.
3. **Programação 100% assíncrona:** `async/await` + `CancellationToken` em toda a cadeia (controller → service → repository).
4. **Histórico imutável:** toda mutação de pontos (ganho ou gasto) **deve** gravar um `PointsHistory`.
5. **Idioma:**
   - Código, identificadores, tipos, rotas e campos JSON em **inglês** (ex.: `MonthlyGoalPoints`, `pointsBalance`).
   - Mensagens exibidas ao usuário final (validações e `Message` de erros) em **português (pt-BR)**, conforme o PRD.
6. **Sem comentários desnecessários:** código deve ser autoexplicativo. Comentários apenas para explicar *porquês* não óbvios (ex.: filtro atômico de saldo).
7. **Regra de negócio no backend:** o frontend valida saldo apenas para UX; a validação real e a garantia de não-negatividade vivem no backend.

---

## 2. Backend C# (.NET 8 + MongoDB)

### 2.1 Estrutura de pastas
```
KidsRewards.Api/
├── Configuration/          # MongoDbSettings.cs
├── Controllers/
├── Domain/
│   ├── Entities/           # Child, Activity, Reward, PointsHistory
│   └── Enums/              # PointOperationType
├── DTOs/                   # ChildDTOs, ActivityDTOs, RewardDTOs, PointsHistoryDTOs
├── Repositories/
│   ├── Interfaces/
│   └── Mongo/
└── Services/
    ├── Interfaces/
    └── Implementation/
```

### 2.2 Nomenclatura
- Interfaces com prefixo `I` (`IChildService`, `IChildRepository`).
- Métodos assíncronos com sufixo `Async` (`CreateAsync`, `GetByIdAsync`, `CompleteActivityAsync`).
- Parâmetro `CancellationToken` por último, com default `default` (exceto nos DTOs de rota, onde é injetado pelo framework).
- Records DTO com sufixos `Request`/`Response` (`CreateChildRequest`, `ChildResponse`).

### 2.3 Entidades e MongoDB
- `[BsonId]` com `[BsonRepresentation(BsonType.ObjectId)]` em `Id` (tipo `string`).
- `[BsonElement("snake_case")]` mapeando o campo JSON (ex.: `pointsBalance`).
- Datas de auditoria (`CreatedAt`, `CompletedAt`) sempre em UTC.
- Enum mapeado com `[BsonRepresentation(BsonType.String)]` **opcional** — se mantiver o padrão do PRD, documente a decisão.

### 2.4 Repositórios — consistência atômica
- Saldo é alterado apenas por operadores atômicos do Mongo:
  - **Incremento (ganho):** `Update.Inc(PointsBalance, delta)` sem filtro extra.
  - **Débito (gasto):** `Update.Inc` **com filtro** `Gte(PointsBalance, Math.Abs(delta))` — impede saldo negativo. Nunca leia-modifique-escreva em duas operações.
- Conclusão de atividade: filtro de atualização inclui `Eq(IsCompleted, false)` para impedir dupla conclusão (idempotência).
- Métodos que "atualizam condicionalmente" retornam `Task<bool>` indicando se a atualização ocorreu (`ModifiedCount > 0`).

### 2.5 Services — regras de negócio
- Validar existência e regras de domínio antes de operar; lançar `InvalidOperationException` com mensagem pt-BR para casos que o Controller converte em `400`.
- Fluxos que alteram pontos seguem ordem segura:
  1. Validação (criança/recompensa/atividade existem e são válidas);
  2. Operação atômica de saldo;
  3. Gravação do `PointsHistory` (com `Type`, `Points`, `ReferenceId`, `Description`).
- Mappers privados `MapToResponse` convertem entidade → DTO; nenhuma entidade vaza do Service.

### 2.6 Controllers
- `[ApiController]`, rota `[Route("api/[controller]")]`.
- Assinaturas com `CancellationToken` no último parâmetro.
- `catch (InvalidOperationException ex) => BadRequest(new { Message = ex.Message })`.
- Respostas: `201 Created` (com `CreatedAtAction` quando houver rota GET), `200 OK`, `204 NoContent`, `400 BadRequest`, `404 NotFound`.
- Anotações `[ProducesResponseType(...)]` documentando os contratos.

### 2.7 DTOs e validação
- `record` para todos os DTOs.
- `System.ComponentModel.DataAnnotations` com mensagens pt-BR:
  - `[Required]`, `[StringLength(min, max)]`, `[Range(min, max)]`.
- Exemplos (do PRD):
  - Nome da criança: 2–100 caracteres, obrigatório.
  - Idade: 1–18.
  - Pontos de atividade: `> 0` (max 10.000).
  - `RequiredPoints` de recompensa: `> 0` (max 100.000).

### 2.8 Configuração e DI
- MongoDB em `Configuration/MongoDbSettings.cs` (`DatabaseName`, `ConnectionString`) lidos de `appsettings.json`.
- `IMongoClient` singleton; `IMongoDatabase` scoped; repositórios e services **scoped**.
- CORS policy `AllowNextJs` apenas para `http://localhost:3000`.
- Swagger apenas em desenvolvimento.

### 2.9 Tratamento de erros
- Erros de regra de negócio → `InvalidOperationException` → `400` com `{ "message": "..." }`.
- Não encontrado → `404` com `{ "message": "..." }`.
- Nenhuma exception não tratada deve vazar ao cliente (use middleware/`ProblemDetails` se necessário).

---

## 3. Frontend (Next.js + TypeScript + Tailwind)

### 3.1 Estrutura
- **App Router** (`src/app/`); componentes interativos com `"use client"` no topo.
- **`src/lib/types.ts`** — interfaces que espelham os DTOs do backend (camelCase):
  ```ts
  interface Child {
    id: string;
    name: string;
    age: number;
    pointsBalance: number;
    monthlyGoalPoints: number;
    monthlyGoalRewardId?: string | null;
    createdAt: string;
  }
  ```
- **`src/lib/api.ts`** — único ponto de acesso ao backend (wrapper `fetch`), com:
  - `baseURL` de `NEXT_PUBLIC_API_URL`;
  - cabeçalho `Content-Type: application/json`;
  - tratamento de erro desnormalizado (`{ message }`);
  - helpers por recurso (`getChildren`, `createActivity`, `completeActivity`, `redeemReward`, ...).
- **`src/components/`** — UI reutilizável (cards, badges, tabs, botões, spinners). Nomes PascalCase (`RewardCard.tsx`).

### 3.2 Tipagem
- Nunca usar `any`. Tipos derivados da API devem seguir os DTOs do backend.
- Estados tipados com unions (ex.: `type Tab = "activities" | "rewards" | "history"`).
- Feedback: `loading` e `error` explícitos em cada tela que consome API.

### 3.3 Estilo (Tailwind — Mobile First)
- Paleta vibrante, botões grandes e áreas de toque ≥ 44px (foco infantil).
- Gradientes para ações primárias; contraste alto para crianças.
- Layout mobile (`max-w-md`) para o dashboard da criança; layout desktop (sidebar) para o admin.
- Ícones via `lucide-react`. Paleta/design system no `tailwind.config.ts`.

### 3.4 Regras de UX/segurança
- **Resgate no cliente:** desabilitar botão quando `pointsBalance < requiredPoints` — é **UX**, nunca a única proteção.
- Feedback imediato (toast/alert) para sucesso e erro de API.
- Progresso da meta mensal: usar o **acumulado `Earned` do mês** (ADR-005), não o saldo corrente.

---

## 4. Padrões de API e HTTP

- Usar os métodos/códigos do contrato (§5 do `planejamento.md`).
- Erros sempre no formato `{ "message": "..." }`.
- Nunca logar ou expor segredos (connection strings, chaves). Usar `.env`/`.gitignore`.

---

## 5. Git e Fluxo de Trabalho

- **Commits pequenos e atômicos**, com mensagens no padrão:
  ```
  feat(api): cria endpoint de conclusão de atividade
  fix(web): trata saldo insuficiente no resgate
  chore(docker): adiciona docker-compose do MongoDB
  ```
- Prefixos: `feat`, `fix`, `refactor`, `chore`, `docs`, `test`.
- Não commitar `bin/`, `obj/`, `node_modules/`, `.next/`, `.env*`.
- Uma mudança por commit; revisar `git status`/`git diff` antes de commitar.

---

## 6. Critérios de Revisão (Checklist)

**Backend**
- [ ] Controller sem regra de negócio.
- [ ] Todo fluxo assíncrono com `CancellationToken`.
- [ ] Saldo alterado apenas com operação atômica (nunca leitura-escrita).
- [ ] Toda mutação de pontos grava `PointsHistory`.
- [ ] DTOs validados com mensagens pt-BR.

**Frontend**
- [ ] Tipos espelham os DTOs; sem `any`.
- [ ] API consumida apenas via `src/lib/api.ts`.
- [ ] Estados de loading/erro tratados.
- [ ] Mobile First e acessível (alvos de toque, contraste).

**Geral**
- [ ] `dotnet build` e `npm run build` sem erros.
- [ ] `npm run lint` limpo.
- [ ] Sem segredos no código ou no histórico.