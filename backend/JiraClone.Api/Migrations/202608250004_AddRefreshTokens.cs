using Microsoft.EntityFrameworkCore.Migrations;
#nullable disable
namespace JiraClone.Api.Migrations;
public partial class AddRefreshTokens : Migration
{
 protected override void Up(MigrationBuilder migrationBuilder)
 {
  migrationBuilder.CreateTable(name: "RefreshTokens", columns: table => new { Id = table.Column<long>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true), UserId = table.Column<int>(type: "INTEGER", nullable: false), TokenHash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false), CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false), ExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: false), RevokedAt = table.Column<DateTime>(type: "TEXT", nullable: true), ReplacedByTokenHash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true) }, constraints: table => { table.PrimaryKey("PK_RefreshTokens", x => x.Id); table.ForeignKey("FK_RefreshTokens_Users_UserId", x => x.UserId, "Users", "Id", onDelete: ReferentialAction.Cascade); });
  migrationBuilder.CreateIndex(name: "IX_RefreshTokens_TokenHash", table: "RefreshTokens", column: "TokenHash", unique: true);
  migrationBuilder.CreateIndex(name: "IX_RefreshTokens_UserId", table: "RefreshTokens", column: "UserId");
 }
 protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable(name: "RefreshTokens");
}
