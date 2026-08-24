namespace JiraClone.Api.Models;

/// <summary>Project-scoped issue number allocator. The database row is the concurrency boundary.</summary>
public sealed class IssueNumberSequence
{
    public int ProjectId { get; set; }
    public int NextNumber { get; set; }
}
