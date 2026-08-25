using System.Security.Claims;
using JiraClone.Api.Data;
using JiraClone.Api.Models;
using JiraClone.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace JiraClone.Api.Tests;

public sealed class ProjectAuthorizationServiceTests
{
    [Theory]
    [InlineData(ProjectRole.Viewer, true, false, false)]
    [InlineData(ProjectRole.Member, true, true, false)]
    [InlineData(ProjectRole.Manager, true, true, true)]
    public async Task Project_role_maps_to_expected_permissions(ProjectRole role, bool canView, bool canEdit, bool canManage)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<JiraDbContext>().UseSqlite(connection).Options;
        await using var db = new JiraDbContext(options);
        await db.Database.EnsureCreatedAsync();
        var project = new Project { Key = "TEST", Name = "Test" };
        var user = new User { Name = "Member", Email = $"{role}@test.local", PasswordHash = "hash", Role = "User" };
        db.Projects.Add(project); db.Users.Add(user); await db.SaveChangesAsync();
        db.ProjectMembers.Add(new ProjectMember { ProjectId = project.Id, UserId = user.Id, Role = role }); await db.SaveChangesAsync();

        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), new Claim(ClaimTypes.Role, "User")], "test"));
        var accessor = new HttpContextAccessor { HttpContext = context };
        var service = new ProjectAuthorizationService(db, accessor);

        Assert.Equal(canView, await Can(() => service.EnsureCanViewAsync(project.Id, CancellationToken.None)));
        Assert.Equal(canEdit, await Can(() => service.EnsureCanEditAsync(project.Id, CancellationToken.None)));
        Assert.Equal(canManage, await Can(() => service.EnsureCanManageAsync(project.Id, CancellationToken.None)));
    }

    private static async Task<bool> Can(Func<Task> action)
    {
        try { await action(); return true; }
        catch (UnauthorizedAccessException) { return false; }
    }
}
