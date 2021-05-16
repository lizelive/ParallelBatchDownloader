using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ParallelBatchDownloader.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Downloads",
                columns: table => new
                {
                    Uri = table.Column<string>(type: "TEXT", nullable: false),
                    Path = table.Column<string>(type: "TEXT", nullable: true),
                    Hash = table.Column<string>(type: "TEXT", nullable: true),
                    Data = table.Column<byte[]>(type: "BLOB", nullable: true),
                    State = table.Column<int>(type: "INTEGER", nullable: false),
                    Size = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Downloads", x => x.Uri);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Downloads");
        }
    }
}
