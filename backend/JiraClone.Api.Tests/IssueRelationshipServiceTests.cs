using JiraClone.Api.Data;
using JiraClone.Api.Models;
using JiraClone.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace JiraClone.Api.Tests;

public sealed class IssueRelationshipServiceTests
{
    [Fact]
    public async Task Create_RejectsSelfRelationship()
    {
        await using var db = CreateDb();
        var project = new Project { Id = 1, Key = "APP", Name = "App" };
        var issue = new Issue { Id = 1, ProjectId = 1, Key = "APP-1", Title = "One", Description = "", Status = IssueStatus.Todo, Priority = IssuePriority.Medium, Type = IssueType.Task, StoryPoints = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        db.Projects.Add(project); db.Issues.Add(issue); await db.SaveChangesAsync();
        var service = CreateService(db);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(1, 1, IssueRelationshipType.Blocks, default));
    }

    [Fact]
    public async Task Create_RejectsCrossProjectRelationship()
    {
        await using var db = CreateDb();
        db.Projects.AddRange(new Project { Id = 1, Key = "A", Name = "A" }, new Project { Id = 2, Key = "B", Name = "B" });
        db.Issues.AddRange(Issue(1, 1, "A-1"), Issue(2, 2, "B-1")); await db.SaveChangesAsync();
        var service = CreateService(db);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(1, 2, IssueRelationshipType.RelatesTo, default));
    }

    [Fact]
    public async Task Create_RelatesTo_IsSymmetricAndIdempotent()
    {
        await using var db = CreateDb();
        db.Projects.Add(new Project { Id = 1, Key = "APP", Name = "App" }); db.Issues.AddRange(Issue(1,1,"APP-1"), Issue(2,1,"APP-2")); await db.SaveChangesAsync();
        var service = CreateService(db);
        var first = await service.CreateAsync(1, 2, IssueRelationshipType.RelatesTo, default);
        var second = await service.CreateAsync(2, 1, IssueRelationshipType.RelatesTo, default);
        Assert.Equal(first.Id, second.Id); Assert.Equal(1, await db.IssueRelationships.CountAsync());
    }

    [Fact]
    public async Task Create_Blocks_IsDirectional()
    {
        await using var db = CreateDb();
        db.Projects.Add(new Project { Id = 1, Key = "APP", Name = "App" }); db.Issues.AddRange(Issue(1,1,"APP-1"), Issue(2,1,"APP-2")); await db.SaveChangesAsync();
        var service = CreateService(db);
        await service.CreateAsync(1, 2, IssueRelationshipType.Blocks, default);
        await service.CreateAsync(2, 1, IssueRelationshipType.Blocks, default);
        Assert.Equal(2, await db.IssueRelationships.CountAsync());
    }

    private static Issue Issue(int id, int projectId, string key) => new() { Id=id, ProjectId=projectId, Key=key, Title=key, Description="", Status=IssueStatus.Todo, Priority=IssuePriority.Medium, Type=IssueType.Task, StoryPoints=1, CreatedAt=DateTime.UtcNow, UpdatedAt=DateTime.UtcNow };
    private static JiraDbContext CreateDb() => new(new DbContextOptionsBuilder<JiraDbContext>().UseSqlite("DataSource=:memory:").Options);
    private static IssueRelationshipService CreateService(JiraDbContext db)
    {
        db.Database.OpenConnection(); db.Database.EnsureCreated();
        var accessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }, "test")) } };
        return new IssueRelationshipService(db, accessor, new ProjectAuthorizationService(db, accessor));
    }
}
