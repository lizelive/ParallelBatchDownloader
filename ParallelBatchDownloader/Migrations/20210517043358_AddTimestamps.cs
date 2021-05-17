using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ParallelBatchDownloader.Migrations
{
    public partial class AddTimestamps : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "Downloads",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "Finished",
                table: "Downloads",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<byte[]>(
                name: "Timestamp",
                table: "Downloads",
                type: "BLOB",
                rowVersion: true,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Created",
                table: "Downloads");

            migrationBuilder.DropColumn(
                name: "Finished",
                table: "Downloads");

            migrationBuilder.DropColumn(
                name: "Timestamp",
                table: "Downloads");
        }
    }
}
