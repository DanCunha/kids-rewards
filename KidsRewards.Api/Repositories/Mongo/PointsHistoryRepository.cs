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