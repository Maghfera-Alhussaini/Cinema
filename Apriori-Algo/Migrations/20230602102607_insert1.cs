using Microsoft.EntityFrameworkCore.Migrations;

namespace Apriori_Algo.Migrations
{
    public partial class insert1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Views",
                table: "Views");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Views",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Views",
                table: "Views",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Views",
                table: "Views");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Views");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Views",
                table: "Views",
                columns: new[] { "UserId", "MovieId" });
        }
    }
}
