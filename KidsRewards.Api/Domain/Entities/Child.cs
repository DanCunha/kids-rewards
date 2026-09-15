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