using JiraClone.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace JiraClone.Api.Data;

public sealed class JiraDbContext(DbContextOptions<JiraDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Sprint> Sprints => Set<Sprint>();
    public DbSet<Issue> Issues => Set<Issue>();
    public DbSet<IssueComment> IssueComments => Set<IssueComment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<Project>().HasIndex(x => x.Key).IsUnique();
        modelBuilder.Entity<Issue>().HasIndex(x => new { x.ProjectId, x.Number }).IsUnique();

        modelBuilder.Entity<Issue>()
            .HasOne(x => x.Project).WithMany(x => x.Issues)
            .HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Issue>()
            .HasOne(x => x.Assignee).WithMany()
            .HasForeignKey(x => x.AssigneeId).OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Issue>()
            .HasOne(x => x.Sprint).WithMany(x => x.Issues)
            .HasForeignKey(x => x.SprintId).OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<IssueComment>()
            .HasOne(x => x.Issue).WithMany(x => x.Comments)
            .HasForeignKey(x => x.IssueId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<IssueComment>()
            .HasOne(x => x.Author).WithMany()
            .HasForeignKey(x => x.AuthorId).OnDelete(DeleteBehavior.Restrict);
    }
}
