using System.Security.Claims;
using JiraClone.Api.Data;
using JiraClone.Api.Domain;
using JiraClone.Api.Models;
using JiraClone.Api.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JiraClone.Api.Tests;

public sealed class IssueTransitionIntegrationTests
{
    [Fact]
    public async Task Authorized_member_can_transition_and_audit_change()
    {
        await using var fixture = await Fixture.CreateAsync();
        var issue = await fixture.CreateIssueAsync(IssueStatus.Todo);
        fixture.UserId = fixture.Member.Id;

        await fixture.Service.MoveAsync(issue.Id, IssueStatus.InProgress, CancellationToken.None);

        var saved = await fixture.Db.Issues.SingleAsync(x => x.Id == issue.Id);
        var activity = await fixture.Db.IssueActivities.SingleAsync(x => x.IssueId == issue.Id && x.Type == IssueActivityType.StatusChanged);
        Assert.Equal(IssueStatus.InProgress, saved.Status);
        Assert.Equal(IssueStatus.Todo.ToString(), activity.OldValue);
        Assert.Equal(IssueStatus.InProgress.ToString(), activity.NewValue);
    }

    [Fact]
    public async Task Invalid_transition_does_not_mutate_issue_or_create_audit_event()
    {
        await using var fixture = await Fixture.CreateAsync();
        var issue = await fixture.CreateIssueAsync(IssueStatus.Backlog);
        fixture.UserId = fixture.Member.Id;

        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Service.MoveAsync(issue.Id, IssueStatus.Done, CancellationToken.None));

        var saved = await fixture.Db.Issues.SingleAsync(x => x.Id == issue.Id);
        var statusChanges = await fixture.Db.IssueActivities.CountAsync(x => x.IssueId == issue.Id && x.Type == IssueActivityType.StatusChanged);
        Assert.Equal(IssueStatus.Backlog, saved.Status);
        Assert.Equal(0, statusChanges);
    }

    private sealed class Fixture : IAsyncDisposable
    {
        private Fixture(SqliteConnection connection, JiraDbContext db, IssueApplicationService service, Project project, User member)
        { Connection = connection; Db = db; Service = service; Project = project; Member = member; }
        public SqliteConnection Connection { get; }
        public JiraDbContext Db { get; }
        public IssueApplicationService Service { get; }
        public Project Project { get; }
        public User Member { get; }
        public int UserId { get; set; }

        public static async Task<Fixture> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var options = new DbContextOptionsBuilder<JiraDbContext>().UseSqlite(connection).Options;
            var db = new JiraDbContext(options);
            await db.Database.EnsureCreatedAsync();
            var project = new Project { Key = "TEST", Name = "Test" };
            var member = new User { Name = "Member", Email = "member@test.local", PasswordHash = "test" };
            db.Projects.Add(project); db.Users.Add(member); await db.SaveChangesAsync();
            db.ProjectMembers.Add(new ProjectMember { ProjectId = project.Id, UserId = member.Id, Role = ProjectRole.Member });
            await db.SaveChangesAsync();
            var service = new IssueApplicationService(db, new TestHttpContextAccessor(() => member.Id));
            return new Fixture(connection, db, service, project, member) { UserId = member.Id };
        }

        public async Task<Issue> CreateIssueAsync(IssueStatus status)
        {
            var issue = new Issue { ProjectId = Project.Id, Number = 1, Title = "Workflow test", Description = "", Status = status, Priority = IssuePriority.Medium, Type = IssueType.Task, StoryPoints = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            Db.Issues.Add(issue); await Db.SaveChangesAsync(); return issue;
        }

        public async ValueTask DisposeAsync() { await Db.DisposeAsync(); await Connection.DisposeAsync(); }
    }

    private sealed class TestHttpContextAccessor(Func<int> userId) : IHttpContextAccessor
    {
        public Microsoft.AspNetCore.Http.HttpContext? HttpContext { get; set; } = CreateContext(userId);
        private static Microsoft.AspNetCore.Http.HttpContext CreateContext(Func<int> getUserId)
        {
            var context = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            context.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, getUserId().ToString())], "test"));
            return context;
        }
    }
}
