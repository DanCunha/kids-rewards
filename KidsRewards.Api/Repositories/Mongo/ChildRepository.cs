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