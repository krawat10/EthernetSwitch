using Microsoft.EntityFrameworkCore.Migrations;

namespace EthernetSwitch.Data.Migrations
{
    public partial class AddedSNMPMessages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrapMessages",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Type = table.Column<int>(nullable: false),
                    Version = table.Column<int>(nullable: false),
                    TimeStamp = table.Column<uint>(nullable: false),
                    ContextName = table.Column<string>(nullable: true),
                    MessageId = table.Column<int>(nullable: false),
                    Enterprise = table.Column<string>(nullable: true),
                    UserName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrapMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SNMPMessageVariable",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TrapMessageId = table.Column<long>(nullable: false),
                    VariableId = table.Column<string>(nullable: true),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SNMPMessageVariable", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SNMPMessageVariable_TrapMessages_TrapMessageId",
                        column: x => x.TrapMessageId,
                        principalTable: "TrapMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SNMPMessageVariable_TrapMessageId",
                table: "SNMPMessageVariable",
                column: "TrapMessageId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SNMPMessageVariable");

            migrationBuilder.DropTable(
                name: "TrapMessages");
        }
    }
}
