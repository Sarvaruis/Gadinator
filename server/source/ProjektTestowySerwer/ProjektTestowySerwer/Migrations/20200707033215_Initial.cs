using Microsoft.EntityFrameworkCore.Migrations;

namespace ProjektTestowySerwer.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Areas",
                columns: table => new
                {
                    AreasId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AreasName = table.Column<string>(maxLength: 50, nullable: true),
                    AreasX = table.Column<int>(nullable: false),
                    AreasY = table.Column<int>(nullable: false),
                    AreasWidth = table.Column<int>(nullable: false),
                    AreasHeight = table.Column<int>(nullable: false),
                    ParentAreaId = table.Column<int>(nullable: true),
                    ProjectId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Areas", x => x.AreasId);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    CategoriesId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CategoriesName = table.Column<string>(maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CategoriesId);
                });

            migrationBuilder.CreateTable(
                name: "Instances",
                columns: table => new
                {
                    InstancesId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    InstancesX = table.Column<int>(nullable: false),
                    InstancesY = table.Column<int>(nullable: false),
                    ObjectId = table.Column<int>(nullable: false),
                    AreaId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instances", x => x.InstancesId);
                });

            migrationBuilder.CreateTable(
                name: "Objects",
                columns: table => new
                {
                    ObjectsId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ObjectsName = table.Column<string>(maxLength: 100, nullable: false),
                    ObjectsWidth = table.Column<int>(nullable: false),
                    ObjectsHeight = table.Column<int>(nullable: false),
                    ObjectsImagePath = table.Column<string>(maxLength: 100, nullable: true),
                    CategoryId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Objects", x => x.ObjectsId);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    ProjectsId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProjectsName = table.Column<string>(maxLength: 50, nullable: false),
                    ProjectsBackgroundFilePath = table.Column<string>(maxLength: 100, nullable: true),
                    ProjectsGridWidth = table.Column<int>(nullable: false),
                    ProjectsGridHeight = table.Column<int>(nullable: false),
                    UserId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.ProjectsId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UsersId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UsersLogin = table.Column<string>(maxLength: 50, nullable: false),
                    UsersPassword = table.Column<string>(maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UsersId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Areas");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Instances");

            migrationBuilder.DropTable(
                name: "Objects");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
