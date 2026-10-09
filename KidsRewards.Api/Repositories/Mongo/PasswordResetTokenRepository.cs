using KidsRewards.Api.Domain.Entities;
using KidsRewards.Api.Repositories.Interfaces;
using MongoDB.Driver;

namespace KidsRewards.Api.Repositories.Mongo;

public class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly IMongoCollection<PasswordResetToken> _collection;

    public PasswordResetTokenRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<PasswordResetToken>("PasswordResetTokens");
    }

    public async Task CreateAsync(PasswordResetToken token, CancellationToken cancellationToken = default)
    {
        await _collection.InsertOneAsync(token, cancellationToken: cancellationToken);
    }

    public async Task<PasswordResetToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        return await _collection.Find(t => t.TokenHash == tokenHash).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> MarkUsedAsync(string tokenId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<PasswordResetToken>.Filter.Eq(t => t.Id, tokenId);
        var update = Builders<PasswordResetToken>.Update.Set(t => t.UsedAt, DateTime.UtcNow);
        var result = await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        return result.ModifiedCount > 0;
    }

    public async Task DeleteByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        await _collection.DeleteManyAsync(t => t.UserId == userId, cancellationToken);
    }
}
