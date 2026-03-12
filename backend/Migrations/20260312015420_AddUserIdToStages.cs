using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToStages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Stages",
                type: "text",
                nullable: true);

            migrationBuilder.Sql(@"
                INSERT INTO ""Stages"" (""Name"", ""Color"", ""UserId"")
                SELECT s.""Name"", s.""Color"", u.""Id""
                FROM ""Stages"" s
                CROSS JOIN ""Users"" u
                WHERE s.""UserId"" IS NULL AND u.""Id"" IS NOT NULL;
            ");

            migrationBuilder.Sql(@"
                DELETE FROM ""Stages"" WHERE ""UserId"" IS NULL;
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Stages_UserId",
                table: "Stages",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Stages_Users_UserId",
                table: "Stages",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stages_Users_UserId1",
                table: "Stages");

            migrationBuilder.DropIndex(
                name: "IX_Stages_UserId1",
                table: "Stages");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Stages");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Stages");
        }
    }
}
