using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TrexLock.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Locks",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    State = table.Column<int>(nullable: false),
                    Timeout = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PinLogs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Pin = table.Column<int>(nullable: false),
                    Reason = table.Column<string>(nullable: true),
                    PinState = table.Column<int>(nullable: false),
                    Time = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PinLogs", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Locks");

            migrationBuilder.DropTable(
                name: "PinLogs");
        }
    }
}
