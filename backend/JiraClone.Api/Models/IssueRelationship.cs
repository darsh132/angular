namespace JiraClone.Api.Models;

public enum IssueRelationshipType
{
    Blocks,
    RelatesTo,
    Duplicates
}

public sealed class IssueRelationship
{
    public long Id { get; set; }
    public int SourceIssueId { get; set; }
    public Issue SourceIssue { get; set; } = null!;
    public int TargetIssueId { get; set; }
    public Issue TargetIssue { get; set; } = null!;
    public IssueRelationshipType Type { get; set; }
    public DateTime CreatedAt { get; set; }
    public int CreatedById { get; set; }
    public User CreatedBy { get; set; } = null!;
}
