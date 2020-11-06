using Microsoft.EntityFrameworkCore.Migrations;

namespace EthernetSwitch.Data.Migrations
{
    public partial class AddedTrapUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrapUsers",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserName = table.Column<string>(nullable: true),
                    Port = table.Column<int>(nullable: false),
                    Password = table.Column<string>(nullable: true),
                    Encryption = table.Column<string>(nullable: true),
                    EngineId = table.Column<string>(nullable: true),
                    EncryptionType = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrapUsers", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrapUsers");
        }
    }
}
