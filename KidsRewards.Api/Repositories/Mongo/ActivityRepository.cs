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