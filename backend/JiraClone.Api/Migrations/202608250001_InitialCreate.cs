using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JiraClone.Api.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(name: "Projects", columns: table => new
        {
            Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
            Key = table.Column<string>(type: "TEXT", nullable: false),
            Name = table.Column<string>(type: "TEXT", nullable: false),
            Description = table.Column<string>(type: "TEXT", nullable: false)
        }, constraints: table => table.PrimaryKey("PK_Projects", x => x.Id));

        migrationBuilder.CreateTable(name: "Users", columns: table => new
        {
            Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
            Name = table.Column<string>(type: "TEXT", nullable: false),
            Email = table.Column<string>(type: "TEXT", nullable: false),
            Avatar = table.Column<string>(type: "TEXT", nullable: false),
            PasswordHash = table.Column<string>(type: "TEXT", nullable: false)
        }, constraints: table => table.PrimaryKey("PK_Users", x => x.Id));

        migrationBuilder.CreateTable(name: "Sprints", columns: table => new
        {
            Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
            Name = table.Column<string>(type: "TEXT", nullable: false),
            Goal = table.Column<string>(type: "TEXT", nullable: true),
            Status = table.Column<string>(type: "TEXT", nullable: false),
            ProjectId = table.Column<int>(type: "INTEGER", nullable: false),
            StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
            EndDate = table.Column<DateTime>(type: "TEXT", nullable: false)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_Sprints", x => x.Id);
            table.ForeignKey("FK_Sprints_Projects_ProjectId", x => x.ProjectId, "Projects", "Id", onDelete: ReferentialAction.Cascade);
        });

        migrationBuilder.CreateTable(name: "IssueNumberSequences", columns: table => new
        {
            ProjectId = table.Column<int>(type: "INTEGER", nullable: false),
            NextNumber = table.Column<int>(type: "INTEGER", nullable: false)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_IssueNumberSequences", x => x.ProjectId);
            table.ForeignKey("FK_IssueNumberSequences_Projects_ProjectId", x => x.ProjectId, "Projects", "Id", onDelete: ReferentialAction.Cascade);
        });

        migrationBuilder.CreateTable(name: "Issues", columns: table => new
        {
            Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
            Number = table.Column<int>(type: "INTEGER", nullable: false),
            Title = table.Column<string>(type: "TEXT", nullable: false),
            Description = table.Column<string>(type: "TEXT", nullable: false),
            Status = table.Column<string>(type: "TEXT", nullable: false),
            Priority = table.Column<string>(type: "TEXT", nullable: false),
            Type = table.Column<string>(type: "TEXT", nullable: false),
            StoryPoints = table.Column<int>(type: "INTEGER", nullable: false),
            ProjectId = table.Column<int>(type: "INTEGER", nullable: false),
            AssigneeId = table.Column<int>(type: "INTEGER", nullable: true),
            SprintId = table.Column<int>(type: "INTEGER", nullable: true),
            CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
            UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_Issues", x => x.Id);
            table.ForeignKey("FK_Issues_Projects_ProjectId", x => x.ProjectId, "Projects", "Id", onDelete: ReferentialAction.Cascade);
            table.ForeignKey("FK_Issues_Sprints_SprintId", x => x.SprintId, "Sprints", "Id", onDelete: ReferentialAction.SetNull);
            table.ForeignKey("FK_Issues_Users_AssigneeId", x => x.AssigneeId, "Users", "Id", onDelete: ReferentialAction.SetNull);
        });

        migrationBuilder.CreateTable(name: "IssueActivities", columns: table => new
        {
            Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
            IssueId = table.Column<int>(type: "INTEGER", nullable: false),
            ActorId = table.Column<int>(type: "INTEGER", nullable: false),
            Type = table.Column<string>(type: "TEXT", nullable: false),
            OldValue = table.Column<string>(type: "TEXT", nullable: true),
            NewValue = table.Column<string>(type: "TEXT", nullable: true),
            CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_IssueActivities", x => x.Id);
            table.ForeignKey("FK_IssueActivities_Issues_IssueId", x => x.IssueId, "Issues", "Id", onDelete: ReferentialAction.Cascade);
            table.ForeignKey("FK_IssueActivities_Users_ActorId", x => x.ActorId, "Users", "Id", onDelete: ReferentialAction.Restrict);
        });

        migrationBuilder.CreateTable(name: "IssueComments", columns: table => new
        {
            Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
            IssueId = table.Column<int>(type: "INTEGER", nullable: false),
            AuthorId = table.Column<int>(type: "INTEGER", nullable: false),
            Body = table.Column<string>(type: "TEXT", nullable: false),
            CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_IssueComments", x => x.Id);
            table.ForeignKey("FK_IssueComments_Issues_IssueId", x => x.IssueId, "Issues", "Id", onDelete: ReferentialAction.Cascade);
            table.ForeignKey("FK_IssueComments_Users_AuthorId", x => x.AuthorId, "Users", "Id", onDelete: ReferentialAction.Restrict);
        });

        migrationBuilder.CreateIndex(name: "IX_IssueActivities_ActorId", table: "IssueActivities", column: "ActorId");
        migrationBuilder.CreateIndex(name: "IX_IssueActivities_IssueId", table: "IssueActivities", column: "IssueId");
        migrationBuilder.CreateIndex(name: "IX_IssueComments_AuthorId", table: "IssueComments", column: "AuthorId");
        migrationBuilder.CreateIndex(name: "IX_IssueComments_IssueId", table: "IssueComments", column: "IssueId");
        migrationBuilder.CreateIndex(name: "IX_Issues_AssigneeId", table: "Issues", column: "AssigneeId");
        migrationBuilder.CreateIndex(name: "IX_Issues_ProjectId_Number", table: "Issues", columns: new[] { "ProjectId", "Number" }, unique: true);
        migrationBuilder.CreateIndex(name: "IX_Issues_SprintId", table: "Issues", column: "SprintId");
        migrationBuilder.CreateIndex(name: "IX_Projects_Key", table: "Projects", column: "Key", unique: true);
        migrationBuilder.CreateIndex(name: "IX_Sprints_ProjectId", table: "Sprints", column: "ProjectId");
        migrationBuilder.CreateIndex(name: "IX_Users_Email", table: "Users", column: "Email", unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "IssueActivities");
        migrationBuilder.DropTable(name: "IssueComments");
        migrationBuilder.DropTable(name: "IssueNumberSequences");
        migrationBuilder.DropTable(name: "Issues");
        migrationBuilder.DropTable(name: "Sprints");
        migrationBuilder.DropTable(name: "Users");
        migrationBuilder.DropTable(name: "Projects");
    }
}
