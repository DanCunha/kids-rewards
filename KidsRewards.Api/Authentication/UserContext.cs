namespace KidsRewards.Api.Authentication;

public record UserContext(string UserId, string Type, string? ChildId)
{
    public bool IsResponsible => Type == "Responsible";
    public bool IsChild => Type == "Child";
}
