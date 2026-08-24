using JiraClone.Api.Models;

namespace JiraClone.Api.Domain;

public static class IssueWorkflow
{
    private static readonly IReadOnlyDictionary<IssueStatus, IssueStatus[]> Allowed =
        new Dictionary<IssueStatus, IssueStatus[]>
        {
            [IssueStatus.Backlog] = [IssueStatus.Todo],
            [IssueStatus.Todo] = [IssueStatus.InProgress, IssueStatus.Backlog],
            [IssueStatus.InProgress] = [IssueStatus.InReview, IssueStatus.Todo],
            [IssueStatus.InReview] = [IssueStatus.Done, IssueStatus.InProgress],
            [IssueStatus.Done] = [IssueStatus.InProgress]
        };

    public static bool CanTransition(IssueStatus from, IssueStatus to) =>
        from == to || (Allowed.TryGetValue(from, out var targets) && targets.Contains(to));

    public static void EnsureCanTransition(IssueStatus from, IssueStatus to)
    {
        if (!CanTransition(from, to))
            throw new InvalidOperationException($"Transition from {from} to {to} is not allowed.");
    }
}
