using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UnivForm.Migrations
{
    /// <inheritdoc />
    public partial class AddEmojiGifAndModeratorRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GifUrl",
                table: "ForumThreads",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SelectedEmoji",
                table: "ForumThreads",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GifUrl",
                table: "ForumThreads");

            migrationBuilder.DropColumn(
                name: "SelectedEmoji",
                table: "ForumThreads");
        }
    }
}
