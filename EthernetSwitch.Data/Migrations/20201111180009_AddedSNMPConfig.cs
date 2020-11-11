using Microsoft.EntityFrameworkCore.Migrations;

namespace EthernetSwitch.Data.Migrations
{
    public partial class AddedSNMPConfig : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SNMPConfiguration_AgentAddresses",
                table: "Settings",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SNMPConfiguration_SysContact",
                table: "Settings",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SNMPConfiguration_SysLocation",
                table: "Settings",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SNMPConfiguration_TrapRecieverIpAddress",
                table: "Settings",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SNMPConfiguration_TrapRecieverPort",
                table: "Settings",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SNMPUsers",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserName = table.Column<string>(nullable: true),
                    Password = table.Column<string>(nullable: true),
                    Encryption = table.Column<string>(nullable: true),
                    EncryptionType = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SNMPUsers", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SNMPUsers");

            migrationBuilder.DropColumn(
                name: "SNMPConfiguration_AgentAddresses",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "SNMPConfiguration_SysContact",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "SNMPConfiguration_SysLocation",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "SNMPConfiguration_TrapRecieverIpAddress",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "SNMPConfiguration_TrapRecieverPort",
                table: "Settings");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "Settings",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(long))
                .Annotation("Sqlite:Autoincrement", true);
        }
    }
}
