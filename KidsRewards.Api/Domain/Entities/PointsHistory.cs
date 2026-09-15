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