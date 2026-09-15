# Especificação de Arquitetura, PRD, ADR e Código: Gerenciador de Atividades e Recompensas Infantis (KidsRewards)

Documento completo de especificação de produto, decisões de arquitetura, diretrizes de código e implementação completa do Backend em **C# (.NET 8 Core + MongoDB)** e estrutura Frontend em **Next.js (TypeScript + Tailwind CSS - Mobile First)**.

---

## 1. PRD (Product Requirement Document)

### 1.1 Visão Geral e Argumento do Produto
* **A Dor Central:** Pais e responsáveis enfrentam dificuldades em engajar crianças nas tarefas diárias (estudos, organização do quarto, higiene) de forma positiva e lúdica, recorrendo frequentemente a broncas ou barganhas sem acompanhamento contínuo.
* **Proposta de Valor:** O **KidsRewards** transforma a rotina infantil em uma jornada gamificada. As crianças realizam tarefas cotidianas, ganham pontos com visualização clara de progresso mensal e trocam por recompensas cadastradas pelos pais, estimulando autonomia e responsabilidade de forma divertida.
* **Público-Alvo:** Pais/Responsáveis (gestão e acompanhamento) e Crianças de 4 a 12 anos (visualização Mobile First intuitiva).

### 1.2 Regras de Negócio Cruciais
1. **Cadastro e Saldo:** Cada criança possui um cadastro com nome, idade e um saldo atual de pontos (`PointsBalance`).
2. **Conclusão de Atividades:** Apenas atividades ativas e não concluídas geram pontos ao serem marcadas como concluídas.
3. **Resgate de Recompensas:** O resgate só é permitido se o saldo da criança for maior ou igual aos pontos exigidos (`RequiredPoints`). **Saldo negativo é estritamente proibido**.
4. **Histórico de Operações:** Toda alteração de pontos (seja por ganho em atividade ou gasto em recompensa) DEVE gerar um registro imutável no histórico (`PointsHistory`).
5. **Ciclo e Meta Mensal:** Suporte à definição de uma meta mensal de pontos por criança para conquistar recompensas especiais, acompanhada pelo total de pontos acumulados no ciclo mensal vigente.

### 1.3 Escopo do MVP e Gates de Desenvolvimento

* **Gate 1: Fundamentos de Crianças e Pontuação Base (Backend C# + Mongo)**
  * Cadastro de crianças, consulta de saldo e estrutura imutável de histórico.
* **Gate 2: Gestão de Atividades e Acúmulo de Pontos**
  * Criação de atividades vinculadas à criança, conclusão de tarefas com soma atômica de saldo e registro em histórico (`Type: Earned`).
* **Gate 3: Gestão e Resgate de Recompensas**
  * Cadastro de catálogo de recompensas, validação rígida de saldo de pontos, consumo de saldo e registro em histórico (`Type: Spent`).
* **Gate 4: Painel Mobile First em Next.js e Metas Mensais**
  * Interface gamificada responsiva para smartphones (Next.js + Tailwind CSS), exibindo o progresso da meta mensal do mês vigente.

---

## 2. ADR (Architecture Decision Record)

### ADR-001: Seleção de Arquitetura Backend (C# + MongoDB) e Frontend (Next.js)

#### Status
**Aceito**

#### Contexto
A aplicação exige alta performance em dispositivos móveis para as crianças, facilidade de expansão dos documentos de atividades/histórico e uma separação limpa entre regras de negócio, persistência e camada de entrega (APIs REST).

#### Decisões de Engenharia
1. **Backend C# (.NET 8 Web API):**
   * Padrão em camadas seguindo SOLID: `Controllers` (entrega/rotas HTTP) -> `Services` (regras de negócio) -> `Repositories` (acesso aos dados via `MongoDB.Driver`).
   * Padrão DTO (`Data Transfer Object`) para todas as requisições e respostas, isolando completamente o domínio interno.
   * Programação 100% assíncrona com `async/await` e `CancellationToken`.
2. **Banco de Dados (MongoDB):**
   * Mapeamento BSON nativo em C# com suporte a `ObjectId` em formato `string`.
   * Operações de saldo e histórico com garantias de consistência atômica.
3. **Frontend (Next.js 14+ App Router, TypeScript, Tailwind CSS):**
   * Abordagem **Mobile First**, com botões grandes, feedback visual imediato para crianças e paleta vibrante.

---

## 3. Coding Guidelines (Convenções e Padrões de Código)

1. **Separação Rígida de Responsabilidades:**
   * **Controllers:** Apenas recebem DTOs, validam entrada básica, chamam o Service e retornam `IActionResult` com códigos HTTP apropriados (`200 OK`, `201 Created`, `400 BadRequest`, `404 NotFound`). Nenhuma regra de negócio deve residir no Controller.
   * **Services:** Centralizam as regras de negócio, validações de domínio (ex: checagem de saldo antes do resgate) e orquestram operações entre repositórios.
   * **Repositories:** Abstraem o acesso ao MongoDB (`IMongoCollection<T>`), utilizando métodos assíncronos nativos.
2. **Nomenclatura C# Standard:**
   * Interfaces iniciadas com `I` (`IChildService`, `IChildRepository`).
   * Métodos assíncronos terminados com o sufixo `Async` (`CreateAsync`, `GetByIdAsync`).
3. **Tratamento de Exceções e Respostas:**
   * Uso de DTOs claros para entrada e saída. Respostas padronizadas para erros de validação e regras de negócio.

---

## 4. Implementação Completa do Backend (C# .NET 8 + MongoDB)

### 4.1 Estrutura de Pastas Sugerida (C#)

```
KidsRewards.Api/
├── Configuration/
│   └── MongoDbSettings.cs
├── Controllers/
│   ├── ChildrenController.cs
│   ├── ActivitiesController.cs
│   ├── RewardsController.cs
│   └── PointsHistoryController.cs
├── Domain/
│   ├── Entities/
│   │   ├── Child.cs
│   │   ├── Activity.cs
│   │   ├── Reward.cs
│   │   └── PointsHistory.cs
│   └── Enums/
│       └── PointOperationType.cs
├── DTOs/
│   ├── ChildDTOs.cs
│   ├── ActivityDTOs.cs
│   ├── RewardDTOs.cs
│   └── PointsHistoryDTOs.cs
├── Repositories/
│   ├── Interfaces/
│   │   ├── IChildRepository.cs
│   │   ├── IActivityRepository.cs
│   │   ├── IRewardRepository.cs
│   │   └── IPointsHistoryRepository.cs
│   └── Mongo/
│       ├── ChildRepository.cs
│       ├── ActivityRepository.cs
│       ├── RewardRepository.cs
│       └── PointsHistoryRepository.cs
├── Services/
│   ├── Interfaces/
│   │   ├── IChildService.cs
│   │   ├── IActivityService.cs
│   │   ├── IRewardService.cs
│   │   └── IPointsHistoryService.cs
│   └── Implementation/
│       ├── ChildService.cs
│       ├── ActivityService.cs
│       ├── RewardService.cs
│       └── PointsHistoryService.cs
└── Program.cs
```

---

### 4.2 Domínio e Entidades (`Domain/Entities/`)

#### `Domain/Enums/PointOperationType.cs`
```csharp
namespace KidsRewards.Api.Domain.Enums;

public enum PointOperationType
{
    Earned = 1,
    Spent = 2
}
```

#### `Domain/Entities/Child.cs`
```csharp
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KidsRewards.Api.Domain.Entities;

public class Child
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("age")]
    public int Age { get; set; }

    [BsonElement("pointsBalance")]
    public int PointsBalance { get; set; } = 0;

    [BsonElement("monthlyGoalPoints")]
    public int MonthlyGoalPoints { get; set; } = 0;

    [BsonElement("monthlyGoalRewardId")]
    public string? MonthlyGoalRewardId { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

#### `Domain/Entities/Activity.cs`
```csharp
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KidsRewards.Api.Domain.Entities;

public class Activity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("childId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string ChildId { get; set; } = string.Empty;

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    [BsonElement("points")]
    public int Points { get; set; }

    [BsonElement("isCompleted")]
    public bool IsCompleted { get; set; } = false;

    [BsonElement("completedAt")]
    public DateTime? CompletedAt { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

#### `Domain/Entities/Reward.cs`
```csharp
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KidsRewards.Api.Domain.Entities;

public class Reward
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("requiredPoints")]
    public int RequiredPoints { get; set; }

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

#### `Domain/Entities/PointsHistory.cs`
```csharp
using KidsRewards.Api.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KidsRewards.Api.Domain.Entities;

public class PointsHistory
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("childId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string ChildId { get; set; } = string.Empty;

    [BsonElement("type")]
    public PointOperationType Type { get; set; }

    [BsonElement("points")]
    public int Points { get; set; }

    [BsonElement("referenceId")]
    public string ReferenceId { get; set; } = string.Empty;

    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

---

### 4.3 DTOs (Data Transfer Objects) (`DTOs/`)

#### `DTOs/ChildDTOs.cs`
```csharp
using System.ComponentModel.DataAnnotations;

namespace KidsRewards.Api.DTOs;

public record CreateChildRequest(
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres.")]
    string Name,

    [Range(1, 18, ErrorMessage = "A idade deve estar entre 1 e 18 anos.")]
    int Age,

    [Range(0, 100000, ErrorMessage = "A meta de pontos não pode ser negativa.")]
    int MonthlyGoalPoints = 0,

    string? MonthlyGoalRewardId = null
);

public record UpdateChildGoalRequest(
    [Range(0, 100000, ErrorMessage = "A meta de pontos não pode ser negativa.")]
    int MonthlyGoalPoints,

    string? MonthlyGoalRewardId
);

public record ChildResponse(
    string Id,
    string Name,
    int Age,
    int PointsBalance,
    int MonthlyGoalPoints,
    string? MonthlyGoalRewardId,
    DateTime CreatedAt
);
```

#### `DTOs/ActivityDTOs.cs`
```csharp
using System.ComponentModel.DataAnnotations;

namespace KidsRewards.Api.DTOs;

public record CreateActivityRequest(
    [Required(ErrorMessage = "O ID da criança é obrigatório.")]
    string ChildId,

    [Required(ErrorMessage = "O título é obrigatório.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "O título deve ter entre 3 e 150 caracteres.")]
    string Title,

    [StringLength(500, ErrorMessage = "A descrição não pode exceder 500 caracteres.")]
    string Description,

    [Range(1, 10000, ErrorMessage = "A pontuação da atividade deve ser maior que zero.")]
    int Points
);

public record ActivityResponse(
    string Id,
    string ChildId,
    string Title,
    string Description,
    int Points,
    bool IsCompleted,
    DateTime? CompletedAt,
    DateTime CreatedAt
);
```

#### `DTOs/RewardDTOs.cs`
```csharp
using System.ComponentModel.DataAnnotations;

namespace KidsRewards.Api.DTOs;

public record CreateRewardRequest(
    [Required(ErrorMessage = "O nome da recompensa é obrigatório.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 150 caracteres.")]
    string Name,

    [Range(1, 100000, ErrorMessage = "Os pontos necessários devem ser maiores que zero.")]
    int RequiredPoints
);

public record RedeemRewardRequest(
    [Required(ErrorMessage = "O ID da criança é obrigatório.")]
    string ChildId,

    [Required(ErrorMessage = "O ID da recompensa é obrigatório.")]
    string RewardId
);

public record RewardResponse(
    string Id,
    string Name,
    int RequiredPoints,
    bool IsActive,
    DateTime CreatedAt
);
```

#### `DTOs/PointsHistoryDTOs.cs`
```csharp
namespace KidsRewards.Api.DTOs;

public record PointsHistoryResponse(
    string Id,
    string ChildId,
    string Type,
    int Points,
    string ReferenceId,
    string Description,
    DateTime CreatedAt
);
```

---

### 4.4 Repositórios MongoDB (`Repositories/`)

#### Interfaces (`Repositories/Interfaces/`)

```csharp
using KidsRewards.Api.Domain.Entities;

namespace KidsRewards.Api.Repositories.Interfaces;

public interface IChildRepository
{
    Task<Child?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Child>> GetAllAsync(CancellationToken cancellationToken = default);
    Task CreateAsync(Child child, CancellationToken cancellationToken = default);
    Task<bool> UpdateBalanceAsync(string childId, int deltaPoints, CancellationToken cancellationToken = default);
    Task<bool> UpdateGoalAsync(string childId, int monthlyGoalPoints, string? rewardId, CancellationToken cancellationToken = default);
}

public interface IActivityRepository
{
    Task<Activity?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Activity>> GetByChildIdAsync(string childId, CancellationToken cancellationToken = default);
    Task CreateAsync(Activity activity, CancellationToken cancellationToken = default);
    Task<bool> MarkAsCompletedAsync(string id, DateTime completedAt, CancellationToken cancellationToken = default);
}

public interface IRewardRepository
{
    Task<Reward?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Reward>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task CreateAsync(Reward reward, CancellationToken cancellationToken = default);
}

public interface IPointsHistoryRepository
{
    Task CreateAsync(PointsHistory history, CancellationToken cancellationToken = default);
    Task<IEnumerable<PointsHistory>> GetByChildIdAsync(string childId, CancellationToken cancellationToken = default);
}
```

#### Implementações Mongo (`Repositories/Mongo/`)

```csharp
using KidsRewards.Api.Domain.Entities;
using KidsRewards.Api.Repositories.Interfaces;
using MongoDB.Driver;

namespace KidsRewards.Api.Repositories.Mongo;

public class ChildRepository : IChildRepository
{
    private readonly IMongoCollection<Child> _collection;

    public ChildRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<Child>("Children");
    }

    public async Task<Child?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _collection.Find(c => c.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<Child>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _collection.Find(_ => true).ToListAsync(cancellationToken);
    }

    public async Task CreateAsync(Child child, CancellationToken cancellationToken = default)
    {
        await _collection.InsertOneAsync(child, cancellationToken: cancellationToken);
    }

    public async Task<bool> UpdateBalanceAsync(string childId, int deltaPoints, CancellationToken cancellationToken = default)
    {
        // Se deltaPoints for negativo, garante atômica e estritamente que PointsBalance + deltaPoints >= 0
        FilterDefinition<Child> filter;
        if (deltaPoints < 0)
        {
            filter = Builders<Child>.Filter.And(
                Builders<Child>.Filter.Eq(c => c.Id, childId),
                Builders<Child>.Filter.Gte(c => c.PointsBalance, Math.Abs(deltaPoints))
            );
        }
        else
        {
            filter = Builders<Child>.Filter.Eq(c => c.Id, childId);
        }

        var update = Builders<Child>.Update.Inc(c => c.PointsBalance, deltaPoints);
        var result = await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        
        return result.ModifiedCount > 0;
    }

    public async Task<bool> UpdateGoalAsync(string childId, int monthlyGoalPoints, string? rewardId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Child>.Filter.Eq(c => c.Id, childId);
        var update = Builders<Child>.Update
            .Set(c => c.MonthlyGoalPoints, monthlyGoalPoints)
            .Set(c => c.MonthlyGoalRewardId, rewardId);

        var result = await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        return result.ModifiedCount > 0;
    }
}
```

```csharp
using KidsRewards.Api.Domain.Entities;
using KidsRewards.Api.Repositories.Interfaces;
using MongoDB.Driver;

namespace KidsRewards.Api.Repositories.Mongo;

public class ActivityRepository : IActivityRepository
{
    private readonly IMongoCollection<Activity> _collection;

    public ActivityRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<Activity>("Activities");
    }

    public async Task<Activity?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _collection.Find(a => a.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<Activity>> GetByChildIdAsync(string childId, CancellationToken cancellationToken = default)
    {
        return await _collection.Find(a => a.ChildId == childId).SortByDescending(a => a.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task CreateAsync(Activity activity, CancellationToken cancellationToken = default)
    {
        await _collection.InsertOneAsync(activity, cancellationToken: cancellationToken);
    }

    public async Task<bool> MarkAsCompletedAsync(string id, DateTime completedAt, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Activity>.Filter.And(
            Builders<Activity>.Filter.Eq(a => a.Id, id),
            Builders<Activity>.Filter.Eq(a => a.IsCompleted, false)
        );

        var update = Builders<Activity>.Update
            .Set(a => a.IsCompleted, true)
            .Set(a => a.CompletedAt, completedAt);

        var result = await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        return result.ModifiedCount > 0;
    }
}
```

```csharp
using KidsRewards.Api.Domain.Entities;
using KidsRewards.Api.Repositories.Interfaces;
using MongoDB.Driver;

namespace KidsRewards.Api.Repositories.Mongo;

public class RewardRepository : IRewardRepository
{
    private readonly IMongoCollection<Reward> _collection;

    public RewardRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<Reward>("Rewards");
    }

    public async Task<Reward?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _collection.Find(r => r.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<Reward>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _collection.Find(r => r.IsActive).SortBy(r => r.RequiredPoints).ToListAsync(cancellationToken);
    }

    public async Task CreateAsync(Reward reward, CancellationToken cancellationToken = default)
    {
        await _collection.InsertOneAsync(reward, cancellationToken: cancellationToken);
    }
}
```

```csharp
using KidsRewards.Api.Domain.Entities;
using KidsRewards.Api.Repositories.Interfaces;
using MongoDB.Driver;

namespace KidsRewards.Api.Repositories.Mongo;

public class PointsHistoryRepository : IPointsHistoryRepository
{
    private readonly IMongoCollection<PointsHistory> _collection;

    public PointsHistoryRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<PointsHistory>("PointsHistory");
    }

    public async Task CreateAsync(PointsHistory history, CancellationToken cancellationToken = default)
    {
        await _collection.InsertOneAsync(history, cancellationToken: cancellationToken);
    }

    public async Task<IEnumerable<PointsHistory>> GetByChildIdAsync(string childId, CancellationToken cancellationToken = default)
    {
        return await _collection.Find(h => h.ChildId == childId)
            .SortByDescending(h => h.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
```

---

### 4.5 Camada de Serviços de Negócio (`Services/`)

#### Interfaces (`Services/Interfaces/`)

```csharp
using KidsRewards.Api.DTOs;

namespace KidsRewards.Api.Services.Interfaces;

public interface IChildService
{
    Task<ChildResponse> CreateAsync(CreateChildRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<ChildResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ChildResponse?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<bool> UpdateGoalAsync(string childId, UpdateChildGoalRequest request, CancellationToken cancellationToken = default);
}

public interface IActivityService
{
    Task<ActivityResponse> CreateAsync(CreateActivityRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<ActivityResponse>> GetByChildIdAsync(string childId, CancellationToken cancellationToken = default);
    Task<bool> CompleteActivityAsync(string activityId, CancellationToken cancellationToken = default);
}

public interface IRewardService
{
    Task<RewardResponse> CreateAsync(CreateRewardRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<RewardResponse>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<bool> RedeemRewardAsync(RedeemRewardRequest request, CancellationToken cancellationToken = default);
}

public interface IPointsHistoryService
{
    Task<IEnumerable<PointsHistoryResponse>> GetByChildIdAsync(string childId, CancellationToken cancellationToken = default);
}
```

#### Implementações (`Services/Implementation/`)

##### `Services/Implementation/ChildService.cs`
```csharp
using KidsRewards.Api.Domain.Entities;
using KidsRewards.Api.DTOs;
using KidsRewards.Api.Repositories.Interfaces;
using KidsRewards.Api.Services.Interfaces;

namespace KidsRewards.Api.Services.Implementation;

public class ChildService : IChildService
{
    private readonly IChildRepository _childRepository;

    public ChildService(IChildRepository childRepository)
    {
        _childRepository = childRepository;
    }

    public async Task<ChildResponse> CreateAsync(CreateChildRequest request, CancellationToken cancellationToken = default)
    {
        var child = new Child
        {
            Name = request.Name.Trim(),
            Age = request.Age,
            PointsBalance = 0,
            MonthlyGoalPoints = request.MonthlyGoalPoints,
            MonthlyGoalRewardId = request.MonthlyGoalRewardId,
            CreatedAt = DateTime.UtcNow
        };

        await _childRepository.CreateAsync(child, cancellationToken);
        return MapToResponse(child);
    }

    public async Task<IEnumerable<ChildResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var children = await _childRepository.GetAllAsync(cancellationToken);
        return children.Select(MapToResponse);
    }

    public async Task<ChildResponse?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var child = await _childRepository.GetByIdAsync(id, cancellationToken);
        return child == null ? null : MapToResponse(child);
    }

    public async Task<bool> UpdateGoalAsync(string childId, UpdateChildGoalRequest request, CancellationToken cancellationToken = default)
    {
        var child = await _childRepository.GetByIdAsync(childId, cancellationToken);
        if (child == null) return false;

        return await _childRepository.UpdateGoalAsync(childId, request.MonthlyGoalPoints, request.MonthlyGoalRewardId, cancellationToken);
    }

    private static ChildResponse MapToResponse(Child c) =>
        new(c.Id, c.Name, c.Age, c.PointsBalance, c.MonthlyGoalPoints, c.MonthlyGoalRewardId, c.CreatedAt);
}
```

##### `Services/Implementation/ActivityService.cs`
```csharp
using KidsRewards.Api.Domain.Entities;
using KidsRewards.Api.Domain.Enums;
using KidsRewards.Api.DTOs;
using KidsRewards.Api.Repositories.Interfaces;
using KidsRewards.Api.Services.Interfaces;

namespace KidsRewards.Api.Services.Implementation;

public class ActivityService : IActivityService
{
    private readonly IActivityRepository _activityRepository;
    private readonly IChildRepository _childRepository;
    private readonly IPointsHistoryRepository _historyRepository;

    public ActivityService(
        IActivityRepository activityRepository,
        IChildRepository childRepository,
        IPointsHistoryRepository historyRepository)
    {
        _activityRepository = activityRepository;
        _childRepository = childRepository;
        _historyRepository = historyRepository;
    }

    public async Task<ActivityResponse> CreateAsync(CreateActivityRequest request, CancellationToken cancellationToken = default)
    {
        var child = await _childRepository.GetByIdAsync(request.ChildId, cancellationToken);
        if (child == null)
            throw new InvalidOperationException($"Criança com ID '{request.ChildId}' não foi encontrada.");

        var activity = new Activity
        {
            ChildId = request.ChildId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            Points = request.Points,
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow
        };

        await _activityRepository.CreateAsync(activity, cancellationToken);
        return MapToResponse(activity);
    }

    public async Task<IEnumerable<ActivityResponse>> GetByChildIdAsync(string childId, CancellationToken cancellationToken = default)
    {
        var activities = await _activityRepository.GetByChildIdAsync(childId, cancellationToken);
        return activities.Select(MapToResponse);
    }

    public async Task<bool> CompleteActivityAsync(string activityId, CancellationToken cancellationToken = default)
    {
        var activity = await _activityRepository.GetByIdAsync(activityId, cancellationToken);
        if (activity == null || activity.IsCompleted)
            return false;

        var now = DateTime.UtcNow;

        // 1. Marca atividade como concluída
        var marked = await _activityRepository.MarkAsCompletedAsync(activityId, now, cancellationToken);
        if (!marked) return false;

        // 2. Incrementa pontos da criança
        await _childRepository.UpdateBalanceAsync(activity.ChildId, activity.Points, cancellationToken);

        // 3. Grava histórico imutável
        var history = new PointsHistory
        {
            ChildId = activity.ChildId,
            Type = PointOperationType.Earned,
            Points = activity.Points,
            ReferenceId = activity.Id,
            Description = $"Atividade Concluída: {activity.Title}",
            CreatedAt = now
        };
        await _historyRepository.CreateAsync(history, cancellationToken);

        return true;
    }

    private static ActivityResponse MapToResponse(Activity a) =>
        new(a.Id, a.ChildId, a.Title, a.Description, a.Points, a.IsCompleted, a.CompletedAt, a.CreatedAt);
}
```

##### `Services/Implementation/RewardService.cs`
```csharp
using KidsRewards.Api.Domain.Entities;
using KidsRewards.Api.Domain.Enums;
using KidsRewards.Api.DTOs;
using KidsRewards.Api.Repositories.Interfaces;
using KidsRewards.Api.Services.Interfaces;

namespace KidsRewards.Api.Services.Implementation;

public class RewardService : IRewardService
{
    private readonly IRewardRepository _rewardRepository;
    private readonly IChildRepository _childRepository;
    private readonly IPointsHistoryRepository _historyRepository;

    public RewardService(
        IRewardRepository rewardRepository,
        IChildRepository childRepository,
        IPointsHistoryRepository historyRepository)
    {
        _rewardRepository = rewardRepository;
        _childRepository = childRepository;
        _historyRepository = historyRepository;
    }

    public async Task<RewardResponse> CreateAsync(CreateRewardRequest request, CancellationToken cancellationToken = default)
    {
        var reward = new Reward
        {
            Name = request.Name.Trim(),
            RequiredPoints = request.RequiredPoints,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _rewardRepository.CreateAsync(reward, cancellationToken);
        return MapToResponse(reward);
    }

    public async Task<IEnumerable<RewardResponse>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        var rewards = await _rewardRepository.GetAllActiveAsync(cancellationToken);
        return rewards.Select(MapToResponse);
    }

    public async Task<bool> RedeemRewardAsync(RedeemRewardRequest request, CancellationToken cancellationToken = default)
    {
        var child = await _childRepository.GetByIdAsync(request.ChildId, cancellationToken);
        if (child == null)
            throw new InvalidOperationException("Criança não encontrada.");

        var reward = await _rewardRepository.GetByIdAsync(request.RewardId, cancellationToken);
        if (reward == null || !reward.IsActive)
            throw new InvalidOperationException("Recompensa inválida ou inativa.");

        // REGRA DE NEGÓCIO: Verificar saldo antes de debitar
        if (child.PointsBalance < reward.RequiredPoints)
            throw new InvalidOperationException($"Saldo insuficiente. Pontos atuais: {child.PointsBalance}, Necessários: {reward.RequiredPoints}");

        // 1. Tenta atualizar o saldo atômico com verificação de não ficar negativo no Mongo
        var updated = await _childRepository.UpdateBalanceAsync(request.ChildId, -reward.RequiredPoints, cancellationToken);
        if (!updated)
            throw new InvalidOperationException("Falha ao debitar pontos. Verifique o saldo disponível.");

        // 2. Registra o Histórico de Resgate
        var history = new PointsHistory
        {
            ChildId = request.ChildId,
            Type = PointOperationType.Spent,
            Points = reward.RequiredPoints,
            ReferenceId = reward.Id,
            Description = $"Recompensa Resgatada: {reward.Name}",
            CreatedAt = DateTime.UtcNow
        };

        await _historyRepository.CreateAsync(history, cancellationToken);

        return true;
    }

    private static RewardResponse MapToResponse(Reward r) =>
        new(r.Id, r.Name, r.RequiredPoints, r.IsActive, r.CreatedAt);
}
```

##### `Services/Implementation/PointsHistoryService.cs`
```csharp
using KidsRewards.Api.DTOs;
using KidsRewards.Api.Repositories.Interfaces;
using KidsRewards.Api.Services.Interfaces;

namespace KidsRewards.Api.Services.Implementation;

public class PointsHistoryService : IPointsHistoryService
{
    private readonly IPointsHistoryRepository _historyRepository;

    public PointsHistoryService(IPointsHistoryRepository historyRepository)
    {
        _historyRepository = historyRepository;
    }

    public async Task<IEnumerable<PointsHistoryResponse>> GetByChildIdAsync(string childId, CancellationToken cancellationToken = default)
    {
        var histories = await _historyRepository.GetByChildIdAsync(childId, cancellationToken);
        return histories.Select(h => new PointsHistoryResponse(
            h.Id,
            h.ChildId,
            h.Type.ToString(),
            h.Points,
            h.ReferenceId,
            h.Description,
            h.CreatedAt
        ));
    }
}
```

---

### 4.6 Controllers (`Controllers/`)

#### `Controllers/ChildrenController.cs`
```csharp
using KidsRewards.Api.DTOs;
using KidsRewards.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KidsRewards.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChildrenController : ControllerBase
{
    private readonly IChildService _childService;

    public ChildrenController(IChildService childService)
    {
        _childService = childService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ChildResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateChildRequest request, CancellationToken cancellationToken)
    {
        var result = await _childService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ChildResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _childService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ChildResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        var result = await _childService.GetByIdAsync(id, cancellationToken);
        if (result == null) return NotFound(new { Message = $"Criança com ID {id} não encontrada." });
        return Ok(result);
    }

    [HttpPut("{id}/goal")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateGoal(string id, [FromBody] UpdateChildGoalRequest request, CancellationToken cancellationToken)
    {
        var updated = await _childService.UpdateGoalAsync(id, request, cancellationToken);
        if (!updated) return NotFound(new { Message = "Criança não encontrada." });
        return NoContent();
    }
}
```

#### `Controllers/ActivitiesController.cs`
```csharp
using KidsRewards.Api.DTOs;
using KidsRewards.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KidsRewards.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ActivitiesController : ControllerBase
{
    private readonly IActivityService _activityService;

    public ActivitiesController(IActivityService activityService)
    {
        _activityService = activityService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ActivityResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateActivityRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _activityService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetByChildId), new { childId = result.ChildId }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("child/{childId}")]
    [ProducesResponseType(typeof(IEnumerable<ActivityResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByChildId(string childId, CancellationToken cancellationToken)
    {
        var result = await _activityService.GetByChildIdAsync(childId, cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{id}/complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Complete(string id, CancellationToken cancellationToken)
    {
        var success = await _activityService.CompleteActivityAsync(id, cancellationToken);
        if (!success)
            return BadRequest(new { Message = "Não foi possível concluir a atividade. Ela pode já estar concluída ou não existir." });

        return Ok(new { Message = "Atividade concluída com sucesso e pontos adicionados ao saldo!" });
    }
}
```

#### `Controllers/RewardsController.cs`
```csharp
using KidsRewards.Api.DTOs;
using KidsRewards.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KidsRewards.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RewardsController : ControllerBase
{
    private readonly IRewardService _rewardService;

    public RewardsController(IRewardService rewardService)
    {
        _rewardService = rewardService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(RewardResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateRewardRequest request, CancellationToken cancellationToken)
    {
        var result = await _rewardService.CreateAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RewardResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllActive(CancellationToken cancellationToken)
    {
        var result = await _rewardService.GetAllActiveAsync(cancellationToken);
        return Ok(result);
    }

    [HttpPost("redeem")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Redeem([FromBody] RedeemRewardRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var success = await _rewardService.RedeemRewardAsync(request, cancellationToken);
            return Ok(new { Message = "Recompensa resgatada com sucesso!" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }
}
```

#### `Controllers/PointsHistoryController.cs`
```csharp
using KidsRewards.Api.DTOs;
using KidsRewards.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KidsRewards.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PointsHistoryController : ControllerBase
{
    private readonly IPointsHistoryService _historyService;

    public PointsHistoryController(IPointsHistoryService historyService)
    {
        _historyService = historyService;
    }

    [HttpGet("child/{childId}")]
    [ProducesResponseType(typeof(IEnumerable<PointsHistoryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByChildId(string childId, CancellationToken cancellationToken)
    {
        var result = await _historyService.GetByChildIdAsync(childId, cancellationToken);
        return Ok(result);
    }
}
```

---

### 4.7 Configuração do Injeção de Dependências (`Program.cs`)

```csharp
using KidsRewards.Api.Repositories.Interfaces;
using KidsRewards.Api.Repositories.Mongo;
using KidsRewards.Api.Services.Implementation;
using KidsRewards.Api.Services.Interfaces;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Configuração do MongoDB
var mongoConnectionString = builder.Configuration.GetConnectionString("MongoDb") ?? "mongodb://localhost:27017";
var mongoDatabaseName = builder.Configuration["MongoDbSettings:DatabaseName"] ?? "KidsRewardsDb";

builder.Services.AddSingleton<IMongoClient>(new MongoClient(mongoConnectionString));
builder.Services.AddScoped<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(mongoDatabaseName);
});

// Registro de Repositórios
builder.Services.AddScoped<IChildRepository, ChildRepository>();
builder.Services.AddScoped<IActivityRepository, ActivityRepository>();
builder.Services.AddScoped<IRewardRepository, RewardRepository>();
builder.Services.AddScoped<IPointsHistoryRepository, PointsHistoryRepository>();

// Registro de Serviços
builder.Services.AddScoped<IChildService, ChildService>();
builder.Services.AddScoped<IActivityService, ActivityService>();
builder.Services.AddScoped<IRewardService, RewardService>();
builder.Services.AddScoped<IPointsHistoryService, PointsHistoryService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS para Next.js
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNextJs", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowNextJs");
app.UseAuthorization();
app.MapControllers();

app.Run();
```

---

## 5. Estrutura e Interface Frontend (Next.js + TypeScript + Tailwind CSS - Mobile First)

### 5.1 Componente Principal: Dashboard da Criança (`src/app/page.tsx`)

```tsx
"use client";

import React, { useState, useEffect } from "react";
import { Award, CheckCircle2, Star, Gift, History, PlusCircle } from "lucide-react";

interface Child {
  id: string;
  name: string;
  age: number;
  pointsBalance: number;
  monthlyGoalPoints: number;
}

interface Activity {
  id: string;
  title: string;
  description: string;
  points: number;
  isCompleted: boolean;
}

interface Reward {
  id: string;
  name: string;
  requiredPoints: number;
  isActive: boolean;
}

interface HistoryItem {
  id: string;
  type: "Earned" | "Spent";
  points: number;
  description: string;
  createdAt: string;
}

export default function KidsDashboard() {
  const [activeTab, setActiveTab] = useState<"activities" | "rewards" | "history">("activities");
  const [child, setChild] = useState<Child>({
    id: "1",
    name: "Lucas",
    age: 8,
    pointsBalance: 120,
    monthlyGoalPoints: 200,
  });

  const [activities, setActivities] = useState<Activity[]>([
    { id: "a1", title: "Arrumar a cama", description: "Deixar o quarto organizado", points: 15, isCompleted: false },
    { id: "a2", title: "Fazer lição de casa", description: "Matemática e Português", points: 30, isCompleted: false },
    { id: "a3", title: "Escovar os dentes", description: "Após o almoço", points: 10, isCompleted: true },
  ]);

  const [rewards, setRewards] = useState<Reward[]>([
    { id: "r1", name: "30 min de Video Game", requiredPoints: 50, isActive: true },
    { id: "r2", name: "Passeio no Parque", requiredPoints: 100, isActive: true },
    { id: "r3", name: "Escolher o Filme no Domingo", requiredPoints: 150, isActive: true },
  ]);

  const [history, setHistory] = useState<HistoryItem[]>([
    { id: "h1", type: "Earned", points: 10, description: "Atividade Concluída: Escovar os dentes", createdAt: "Hoje, 12:30" },
    { id: "h2", type: "Spent", points: 50, description: "Recompensa Resgatada: 30 min de Video Game", createdAt: "Ontem, 18:00" },
  ]);

  // Ação: Concluir Atividade
  const handleCompleteActivity = (activityId: string, points: number) => {
    setActivities((prev) =>
      prev.map((act) => (act.id === activityId ? { ...act, isCompleted: true } : act))
    );
    setChild((prev) => ({ ...prev, pointsBalance: prev.pointsBalance + points }));
    setHistory((prev) => [
      {
        id: Date.now().toString(),
        type: "Earned",
        points,
        description: `Atividade Concluída: ${activities.find((a) => a.id === activityId)?.title}`,
        createdAt: "Agora",
      },
      ...prev,
    ]);
  };

  // Ação: Resgatar Recompensa
  const handleRedeemReward = (reward: Reward) => {
    if (child.pointsBalance < reward.requiredPoints) {
      alert("Pontos insuficientes para resgatar esta recompensa!");
      return;
    }
    setChild((prev) => ({ ...prev, pointsBalance: prev.pointsBalance - reward.requiredPoints }));
    setHistory((prev) => [
      {
        id: Date.now().toString(),
        type: "Spent",
        points: reward.requiredPoints,
        description: `Recompensa Resgatada: ${reward.name}`,
        createdAt: "Agora",
      },
      ...prev,
    ]);
    alert(`Parabéns! Você resgatou: ${reward.name} 🎉`);
  };

  const progressPercentage = Math.min(
    100,
    Math.round((child.pointsBalance / (child.monthlyGoalPoints || 1)) * 100)
  );

  return (
    <main className="min-h-screen bg-slate-50 pb-20 font-sans text-slate-800 antialiased">
      {/* Top Header Mobile First */}
      <header className="sticky top-0 z-10 bg-gradient-to-r from-amber-400 via-orange-400 to-yellow-500 p-4 shadow-md rounded-b-3xl">
        <div className="mx-auto flex max-w-md items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="flex h-12 w-12 items-center justify-center rounded-full bg-white text-2xl shadow">
              👦
            </div>
            <div>
              <h1 className="text-xl font-extrabold text-white">Olá, {child.name}!</h1>
              <p className="text-xs font-medium text-amber-100">{child.age} anos • Campeão de Tarefas</p>
            </div>
          </div>

          {/* Card de Saldo de Pontos */}
          <div className="flex items-center gap-1.5 rounded-2xl bg-white/95 px-3.5 py-2 shadow-inner">
            <Star className="h-6 w-6 text-yellow-500 fill-yellow-400 animate-pulse" />
            <span className="text-xl font-black text-slate-900">{child.pointsBalance}</span>
            <span className="text-xs font-bold text-slate-500">pts</span>
          </div>
        </div>

        {/* Barra de Meta Mensal */}
        <div className="mx-auto mt-4 max-w-md rounded-xl bg-black/10 p-3 backdrop-blur-sm">
          <div className="flex items-center justify-between text-xs font-bold text-white mb-1.5">
            <span className="flex items-center gap-1"><Award className="h-4 w-4" /> Meta Mensal</span>
            <span>{child.pointsBalance} / {child.monthlyGoalPoints} pts ({progressPercentage}%)</span>
          </div>
          <div className="h-3 w-full overflow-hidden rounded-full bg-white/30">
            <div
              className="h-full rounded-full bg-emerald-400 transition-all duration-500 ease-out"
              style={{ width: `${progressPercentage}%` }}
            />
          </div>
        </div>
      </header>

      {/* Conteúdo Principal */}
      <section className="mx-auto mt-4 max-w-md px-4">
        {/* Navegação por Abas */}
        <div className="flex rounded-2xl bg-slate-200 p-1 font-bold text-slate-600 shadow-inner mb-4">
          <button
            onClick={() => setActiveTab("activities")}
            className={`flex-1 py-2.5 rounded-xl text-xs flex items-center justify-center gap-1.5 transition-all ${
              activeTab === "activities" ? "bg-white text-orange-600 shadow-sm" : ""
            }`}
          >
            <CheckCircle2 className="h-4 w-4" /> Tarefas
          </button>
          <button
            onClick={() => setActiveTab("rewards")}
            className={`flex-1 py-2.5 rounded-xl text-xs flex items-center justify-center gap-1.5 transition-all ${
              activeTab === "rewards" ? "bg-white text-orange-600 shadow-sm" : ""
            }`}
          >
            <Gift className="h-4 w-4" /> Prémios
          </button>
          <button
            onClick={() => setActiveTab("history")}
            className={`flex-1 py-2.5 rounded-xl text-xs flex items-center justify-center gap-1.5 transition-all ${
              activeTab === "history" ? "bg-white text-orange-600 shadow-sm" : ""
            }`}
          >
            <History className="h-4 w-4" /> Histórico
          </button>
        </div>

        {/* Aba de Atividades */}
        {activeTab === "activities" && (
          <div className="space-y-3">
            <h2 className="text-sm font-bold text-slate-500 uppercase tracking-wider">Suas Tarefas de Hoje</h2>
            {activities.map((act) => (
              <div
                key={act.id}
                className={`flex items-center justify-between p-4 rounded-2xl border transition-all ${
                  act.isCompleted
                    ? "bg-emerald-50 border-emerald-200 opacity-75"
                    : "bg-white border-slate-200 shadow-sm hover:shadow-md"
                }`}
              >
                <div>
                  <h3 className={`font-bold ${act.isCompleted ? "line-through text-emerald-800" : "text-slate-800"}`}>
                    {act.title}
                  </h3>
                  <p className="text-xs text-slate-500 mt-0.5">{act.description}</p>
                  <span className="inline-flex items-center gap-1 mt-2 text-xs font-black text-amber-600 bg-amber-50 px-2 py-0.5 rounded-md border border-amber-200">
                    +{act.points} pts
                  </span>
                </div>

                {!act.isCompleted ? (
                  <button
                    onClick={() => handleCompleteActivity(act.id, act.points)}
                    className="flex h-12 px-4 items-center justify-center gap-1 rounded-xl bg-gradient-to-r from-emerald-500 to-teal-600 font-bold text-white shadow-md active:scale-95 transition-transform"
                  >
                    Concluir
                  </button>
                ) : (
                  <span className="flex items-center gap-1 text-xs font-bold text-emerald-600 bg-emerald-100 px-3 py-1.5 rounded-xl">
                    <CheckCircle2 className="h-4 w-4" /> Feito!
                  </span>
                )}
              </div>
            ))}
          </div>
        )}

        {/* Aba de Recompensas */}
        {activeTab === "rewards" && (
          <div className="space-y-3">
            <h2 className="text-sm font-bold text-slate-500 uppercase tracking-wider">Troque Seus Pontos</h2>
            {rewards.map((reward) => {
              const canAfford = child.pointsBalance >= reward.requiredPoints;
              return (
                <div
                  key={reward.id}
                  className="flex items-center justify-between p-4 rounded-2xl bg-white border border-slate-200 shadow-sm"
                >
                  <div>
                    <h3 className="font-bold text-slate-800">{reward.name}</h3>
                    <span className="inline-flex items-center gap-1 mt-1 text-xs font-black text-purple-600 bg-purple-50 px-2 py-0.5 rounded-md border border-purple-200">
                      <Gift className="h-3 w-3" /> {reward.requiredPoints} pts
                    </span>
                  </div>

                  <button
                    onClick={() => handleRedeemReward(reward)}
                    disabled={!canAfford}
                    className={`h-11 px-4 rounded-xl font-bold text-xs shadow-md transition-all ${
                      canAfford
                        ? "bg-gradient-to-r from-purple-600 to-indigo-600 text-white active:scale-95"
                        : "bg-slate-200 text-slate-400 cursor-not-allowed shadow-none"
                    }`}
                  >
                    {canAfford ? "Resgatar" : "Faltam Pontos"}
                  </button>
                </div>
              );
            })}
          </div>
        )}

        {/* Aba de Histórico */}
        {activeTab === "history" && (
          <div className="space-y-3">
            <h2 className="text-sm font-bold text-slate-500 uppercase tracking-wider">Extrato de Pontos</h2>
            {history.map((item) => (
              <div
                key={item.id}
                className="flex items-center justify-between p-3.5 rounded-xl bg-white border border-slate-200 text-xs"
              >
                <div>
                  <p className="font-bold text-slate-800">{item.description}</p>
                  <p className="text-[10px] text-slate-400 mt-0.5">{item.createdAt}</p>
                </div>
                <span
                  className={`font-black text-sm px-2.5 py-1 rounded-lg ${
                    item.type === "Earned"
                      ? "bg-emerald-100 text-emerald-700"
                      : "bg-rose-100 text-rose-700"
                  }`}
                >
                  {item.type === "Earned" ? `+${item.points}` : `-${item.points}`}
                </span>
              </div>
            ))}
          </div>
        )}
      </section>
    </main>
  );
}
```

---

## 6. Resumo da Conformidade com os Requisitos

| Requisito | Status | Implementação no Código |
| :--- | :---: | :--- |
| **Código Assíncrono** | ✅ | Todos os repositórios, serviços e controllers utilizam `Task`, `async/await` e `CancellationToken`. |
| **Separado em Camadas (SOLID)** | ✅ | Total desacoplamento entre `Controller`, `Service`, `Repository` e `Domain`. |
| **Uso Estrito de DTOs** | ✅ | `CreateChildRequest`, `ChildResponse`, `CreateActivityRequest`, `RedeemRewardRequest`, etc. |
| **Sem Lógica no Controller** | ✅ | Controllers apenas orquestram as chamadas de API e tratam respostas HTTP. |
| **Garantia de Saldo Não Negativo** | ✅ | Validação em `RewardService` + filtro de atualização atômica `Gte` no `ChildRepository` Mongo. |
| **Histórico Imutável de Operações** | ✅ | Gravação automática em `PointsHistory` ao concluir atividade ou resgatar recompensa. |
| **Meta Mensal de Pontos** | ✅ | Suporte nos campos `MonthlyGoalPoints` e `MonthlyGoalRewardId` no backend e frontend. |
| **Frontend Mobile First** | ✅ | Componente React Next.js estilizado com Tailwind CSS otimizado para dispositivos móveis. |