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
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Stages",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql(@"
        CREATE TEMP TABLE stage_mapping AS
        SELECT s.""Id"" AS old_id, s.""Name"", s.""Color"", u.""Id"" AS user_id
        FROM ""Stages"" s
        CROSS JOIN ""Users"" u
        WHERE s.""UserId"" IS NULL;

        INSERT INTO ""Stages"" (""Name"", ""Color"", ""UserId"")
        SELECT ""Name"", ""Color"", user_id FROM stage_mapping;

        CREATE TEMP TABLE stage_id_map AS
        SELECT m.old_id, m.user_id, ns.""Id"" AS new_id
        FROM stage_mapping m
        JOIN ""Stages"" ns ON ns.""Name"" = m.""Name"" 
            AND ns.""Color"" = m.""Color"" 
            AND ns.""UserId"" = m.user_id;

        UPDATE ""Tasks"" t
        SET ""StageId"" = sm.new_id
        FROM stage_id_map sm
        WHERE t.""StageId"" = sm.old_id
          AND t.""UserId"" = sm.user_id;

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
