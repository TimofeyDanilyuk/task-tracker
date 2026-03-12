using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToStages1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                INSERT INTO ""Stages"" (""Name"", ""Color"", ""UserId"")
                SELECT s.""Name"", s.""Color"", u.""Id""
                FROM ""Stages"" s
                CROSS JOIN ""Users"" u
                WHERE s.""UserId"" IS NULL;

                DELETE FROM ""Stages"" WHERE ""UserId"" IS NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
