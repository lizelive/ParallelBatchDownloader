using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ParallelBatchDownloader.Migrations
{
    public partial class RemoveData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Downloads",
                table: "Downloads");

            migrationBuilder.DropColumn(
                name: "Data",
                table: "Downloads");

            migrationBuilder.AlterColumn<string>(
                name: "Uri",
                table: "Downloads",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Downloads",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0)
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Downloads",
                table: "Downloads",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Downloads",
                table: "Downloads");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Downloads");

            migrationBuilder.AlterColumn<string>(
                name: "Uri",
                table: "Downloads",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Data",
                table: "Downloads",
                type: "BLOB",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Downloads",
                table: "Downloads",
                column: "Uri");
        }
    }
}
