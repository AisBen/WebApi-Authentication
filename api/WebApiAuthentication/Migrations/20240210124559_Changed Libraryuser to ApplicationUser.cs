using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApiAuthentication.Migrations
{
    /// <inheritdoc />
    public partial class ChangedLibraryusertoApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookReviews_AspNetUsers_LibraryUserId",
                table: "BookReviews");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "LibraryUserId",
                table: "BookReviews",
                newName: "ApplicationUserId");

            migrationBuilder.RenameIndex(
                name: "IX_BookReviews_LibraryUserId",
                table: "BookReviews",
                newName: "IX_BookReviews_ApplicationUserId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RefreshTokenExpiry",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "RatingsAllowed",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BookReviews_AspNetUsers_ApplicationUserId",
                table: "BookReviews",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookReviews_AspNetUsers_ApplicationUserId",
                table: "BookReviews");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserId",
                table: "BookReviews",
                newName: "LibraryUserId");

            migrationBuilder.RenameIndex(
                name: "IX_BookReviews_ApplicationUserId",
                table: "BookReviews",
                newName: "IX_BookReviews_LibraryUserId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RefreshTokenExpiry",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<bool>(
                name: "RatingsAllowed",
                table: "AspNetUsers",
                type: "bit",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_BookReviews_AspNetUsers_LibraryUserId",
                table: "BookReviews",
                column: "LibraryUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
