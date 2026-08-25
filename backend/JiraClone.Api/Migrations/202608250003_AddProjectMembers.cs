using Microsoft.EntityFrameworkCore.Migrations;
#nullable disable
namespace JiraClone.Api.Migrations;
public partial class AddProjectMembers : Migration
{
 protected override void Up(MigrationBuilder migrationBuilder) { migrationBuilder.CreateTable(name: "ProjectMembers", columns: table => new { ProjectId = table.Column<int>(type: "INTEGER", nullable: false), UserId = table.Column<int>(type: "INTEGER", nullable: false), Role = table.Column<string>(type: "TEXT", nullable: false) }, constraints: table => { table.PrimaryKey("PK_ProjectMembers", x => new { x.ProjectId, x.UserId }); table.ForeignKey("FK_ProjectMembers_Projects_ProjectId", x => x.ProjectId, "Projects", "Id", onDelete: ReferentialAction.Cascade); table.ForeignKey("FK_ProjectMembers_Users_UserId", x => x.UserId, "Users", "Id", onDelete: ReferentialAction.Cascade); }); }
 protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable(name: "ProjectMembers");
}
