using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UnivForm.Migrations
{
    /// <inheritdoc />
    public partial class AddViewCountToForumThread : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastActivity",
                table: "ForumThreads",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "ForumThreads",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastActivity",
                table: "ForumThreads");

            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "ForumThreads");
        }
    }
}
