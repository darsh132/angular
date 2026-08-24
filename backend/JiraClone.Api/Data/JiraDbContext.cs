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
    public DbSet<IssueActivity> IssueActivities => Set<IssueActivity>();
    public DbSet<IssueNumberSequence> IssueNumberSequences => Set<IssueNumberSequence>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<Project>().HasIndex(x => x.Key).IsUnique();
        modelBuilder.Entity<Issue>().HasIndex(x => new { x.ProjectId, x.Number }).IsUnique();
        modelBuilder.Entity<Issue>().Property(x => x.Status).HasConversion<string>();
        modelBuilder.Entity<Issue>().Property(x => x.Priority).HasConversion<string>();
        modelBuilder.Entity<Issue>().Property(x => x.Type).HasConversion<string>();
        modelBuilder.Entity<Sprint>().Property(x => x.Status).HasConversion<string>();
        modelBuilder.Entity<IssueActivity>().Property(x => x.Type).HasConversion<string>();
        modelBuilder.Entity<User>().Property(x => x.Role).HasMaxLength(32).IsRequired();

        modelBuilder.Entity<IssueNumberSequence>().HasKey(x => x.ProjectId);
        modelBuilder.Entity<IssueNumberSequence>().Property(x => x.NextNumber).IsRequired();
        modelBuilder.Entity<IssueNumberSequence>().HasOne<Project>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Issue>().HasOne(x => x.Project).WithMany(x => x.Issues).HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Sprint>().HasOne(x => x.Project).WithMany(x => x.Sprints).HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Issue>().HasOne(x => x.Assignee).WithMany().HasForeignKey(x => x.AssigneeId).OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<Issue>().HasOne(x => x.Sprint).WithMany(x => x.Issues).HasForeignKey(x => x.SprintId).OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<IssueComment>().HasOne(x => x.Issue).WithMany(x => x.Comments).HasForeignKey(x => x.IssueId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<IssueComment>().HasOne(x => x.Author).WithMany().HasForeignKey(x => x.AuthorId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<IssueActivity>().HasOne(x => x.Issue).WithMany(x => x.Activities).HasForeignKey(x => x.IssueId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<IssueActivity>().HasOne(x => x.Actor).WithMany().HasForeignKey(x => x.ActorId).OnDelete(DeleteBehavior.Restrict);
    }
}
