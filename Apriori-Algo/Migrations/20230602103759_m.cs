using Microsoft.EntityFrameworkCore.Migrations;

namespace Apriori_Algo.Migrations
{
    public partial class m : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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
                columns: new[] { "MovieId", "UserId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Views",
                table: "Views");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Views",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Views",
                table: "Views",
                column: "Id");
        }
    }
}
