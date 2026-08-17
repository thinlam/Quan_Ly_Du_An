using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class Issue179_RemoveLegacyToTrinhThamDinhNhaThauFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DaThamDinh",
                table: "ToTrinhThamDinhNhaThau");

            migrationBuilder.DropColumn(
                name: "NgayTrinh",
                table: "ToTrinhThamDinhNhaThau");

            migrationBuilder.DropColumn(
                name: "So",
                table: "ToTrinhThamDinhNhaThau");

            migrationBuilder.DropColumn(
                name: "TrichYeu",
                table: "ToTrinhThamDinhNhaThau");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DaThamDinh",
                table: "ToTrinhThamDinhNhaThau",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "NgayTrinh",
                table: "ToTrinhThamDinhNhaThau",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "So",
                table: "ToTrinhThamDinhNhaThau",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TrichYeu",
                table: "ToTrinhThamDinhNhaThau",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);
        }
    }
}
