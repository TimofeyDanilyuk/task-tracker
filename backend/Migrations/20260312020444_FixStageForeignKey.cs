using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class FixStageForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stages_Users_UserId1",
                table: "Stages");

            migrationBuilder.DropIndex(
                name: "IX_Stages_UserId1",
                table: "Stages");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Stages");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId1",
                table: "Stages",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stages_UserId1",
                table: "Stages",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Stages_Users_UserId1",
                table: "Stages",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
