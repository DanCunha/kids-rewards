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