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

    public async Task<IEnumerable<Reward>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _collection.Find(_ => true).SortBy(r => r.RequiredPoints).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Reward>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _collection.Find(r => r.IsActive).SortBy(r => r.RequiredPoints).ToListAsync(cancellationToken);
    }

    public async Task CreateAsync(Reward reward, CancellationToken cancellationToken = default)
    {
        await _collection.InsertOneAsync(reward, cancellationToken: cancellationToken);
    }

    public async Task<bool> SetActiveAsync(string id, bool isActive, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Reward>.Filter.Eq(r => r.Id, id);
        var update = Builders<Reward>.Update.Set(r => r.IsActive, isActive);

        var result = await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        return result.ModifiedCount > 0;
    }
}