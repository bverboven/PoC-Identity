using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Identity.Web.Migrations
{
    public partial class AddLoginEntry : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LoginEntries",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(maxLength: 450, nullable: false),
                    IPAddressBytes = table.Column<byte[]>(maxLength: 16, nullable: false),
                    Status = table.Column<string>(maxLength: 128, nullable: false),
                    Created = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoginEntries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LoginEntries_UserId",
                table: "LoginEntries",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LoginEntries");
        }
    }
}
