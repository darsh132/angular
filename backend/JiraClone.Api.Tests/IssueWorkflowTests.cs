using JiraClone.Api.Domain;
using JiraClone.Api.Models;

namespace JiraClone.Api.Tests;

public sealed class IssueWorkflowTests
{
    [Theory]
    [InlineData(IssueStatus.Backlog, IssueStatus.Todo)]
    [InlineData(IssueStatus.Todo, IssueStatus.InProgress)]
    [InlineData(IssueStatus.InProgress, IssueStatus.InReview)]
    [InlineData(IssueStatus.InReview, IssueStatus.Done)]
    [InlineData(IssueStatus.InReview, IssueStatus.InProgress)]
    public void Allowed_transitions_are_accepted(IssueStatus from, IssueStatus to)
        => Assert.True(IssueWorkflow.CanTransition(from, to));

    [Theory]
    [InlineData(IssueStatus.Backlog, IssueStatus.Done)]
    [InlineData(IssueStatus.Backlog, IssueStatus.InReview)]
    [InlineData(IssueStatus.Done, IssueStatus.Backlog)]
    public void Invalid_transitions_are_rejected(IssueStatus from, IssueStatus to)
        => Assert.False(IssueWorkflow.CanTransition(from, to));

    [Fact]
    public void Ensure_can_transition_throws_for_invalid_transition()
        => Assert.Throws<InvalidOperationException>(() => IssueWorkflow.EnsureCanTransition(IssueStatus.Backlog, IssueStatus.Done));
}
