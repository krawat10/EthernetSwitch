using Microsoft.EntityFrameworkCore.Migrations;

namespace EthernetSwitch.Data.Migrations
{
    public partial class AddedHiddenPropertyToSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HiddenInterfaces",
                table: "Settings",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HiddenInterfaces",
                table: "Settings");
        }
    }
}
