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