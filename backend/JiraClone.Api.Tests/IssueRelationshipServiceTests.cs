using JiraClone.Api.Data;
using JiraClone.Api.Models;
using JiraClone.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JiraClone.Api.Tests;

public sealed class IssueRelationshipServiceTests
{
    [Fact]
    public async Task Create_RejectsSelfRelationship()
    {
        await using var db = CreateDb();
        await SeedProjectAsync(db, 1, 1, ProjectRole.Member, Issue(1, 1, "APP-1"));
        var service = CreateService(db, 1);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(1, 1, IssueRelationshipType.Blocks, default));
    }

    [Fact]
    public async Task Create_RejectsCrossProjectRelationship()
    {
        await using var db = CreateDb();
        await SeedProjectAsync(db, 1, 1, ProjectRole.Member, Issue(1, 1, "A-1"));
        await SeedProjectAsync(db, 2, 1, ProjectRole.Member, Issue(2, 2, "B-1"));
        var service = CreateService(db, 1);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(1, 2, IssueRelationshipType.RelatesTo, default));
    }

    [Fact]
    public async Task Create_RelatesTo_IsSymmetricAndIdempotent()
    {
        await using var db = CreateDb();
        await SeedProjectAsync(db, 1, 1, ProjectRole.Member, Issue(1, 1, "APP-1"), Issue(2, 1, "APP-2"));
        var service = CreateService(db, 1);
        var first = await service.CreateAsync(1, 2, IssueRelationshipType.RelatesTo, default);
        var second = await service.CreateAsync(2, 1, IssueRelationshipType.RelatesTo, default);
        Assert.Equal(first.Id, second.Id);
        Assert.Equal(1, await db.IssueRelationships.CountAsync());
    }

    [Fact]
    public async Task Create_Blocks_IsDirectional()
    {
        await using var db = CreateDb();
        await SeedProjectAsync(db, 1, 1, ProjectRole.Member, Issue(1, 1, "APP-1"), Issue(2, 1, "APP-2"));
        var service = CreateService(db, 1);
        await service.CreateAsync(1, 2, IssueRelationshipType.Blocks, default);
        await service.CreateAsync(2, 1, IssueRelationshipType.Blocks, default);
        Assert.Equal(2, await db.IssueRelationships.CountAsync());
    }

    [Fact]
    public async Task Viewer_CannotCreateRelationship()
    {
        await using var db = CreateDb();
        await SeedProjectAsync(db, 1, 1, ProjectRole.Viewer, Issue(1, 1, "APP-1"), Issue(2, 1, "APP-2"));
        var service = CreateService(db, 1);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.CreateAsync(1, 2, IssueRelationshipType.Blocks, default));
    }

    [Fact]
    public async Task Member_CanCreateRelationship()
    {
        await using var db = CreateDb();
        await SeedProjectAsync(db, 1, 1, ProjectRole.Member, Issue(1, 1, "APP-1"), Issue(2, 1, "APP-2"));
        var service = CreateService(db, 1);
        var result = await service.CreateAsync(1, 2, IssueRelationshipType.Blocks, default);
        Assert.Equal(IssueRelationshipType.Blocks, result.Type);
    }

    private static Issue Issue(int id, int projectId, string key) => new() { Id=id, ProjectId=projectId, Key=key, Title=key, Description="", Status=IssueStatus.Todo, Priority=IssuePriority.Medium, Type=IssueType.Task, StoryPoints=1, CreatedAt=DateTime.UtcNow, UpdatedAt=DateTime.UtcNow };

    private static async Task SeedProjectAsync(JiraDbContext db, int projectId, int userId, ProjectRole role, params Issue[] issues)
    {
        if (!await db.Projects.AnyAsync(x => x.Id == projectId)) db.Projects.Add(new Project { Id=projectId, Key=$"P{projectId}", Name=$"Project {projectId}" });
        if (!await db.Users.AnyAsync(x => x.Id == userId)) db.Users.Add(new User { Id=userId, Name=$"User {userId}", Email=$"user{userId}@test.local" });
        db.ProjectMembers.Add(new ProjectMember { ProjectId=projectId, UserId=userId, Role=role });
        db.Issues.AddRange(issues);
        await db.SaveChangesAsync();
    }

    private static JiraDbContext CreateDb() { var db = new JiraDbContext(new DbContextOptionsBuilder<JiraDbContext>().UseSqlite("DataSource=:memory:").Options); db.Database.OpenConnection(); db.Database.EnsureCreated(); return db; }
    private static IssueRelationshipService CreateService(JiraDbContext db, int userId) { var accessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId.ToString())], "test")) } }; return new IssueRelationshipService(db, accessor, new ProjectAuthorizationService(db, accessor)); }
}
