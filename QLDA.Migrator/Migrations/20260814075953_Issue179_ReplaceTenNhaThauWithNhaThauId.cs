using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class Issue179_ReplaceTenNhaThauWithNhaThauId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TenNhaThau",
                table: "ToTrinhThamDinhNhaThau");

            migrationBuilder.AddColumn<Guid>(
                name: "NhaThauId",
                table: "ToTrinhThamDinhNhaThau",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ToTrinhThamDinhNhaThau_NhaThauId",
                table: "ToTrinhThamDinhNhaThau",
                column: "NhaThauId");

            migrationBuilder.AddForeignKey(
                name: "FK_ToTrinhThamDinhNhaThau_DmNhaThau_NhaThauId",
                table: "ToTrinhThamDinhNhaThau",
                column: "NhaThauId",
                principalTable: "DmNhaThau",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ToTrinhThamDinhNhaThau_DmNhaThau_NhaThauId",
                table: "ToTrinhThamDinhNhaThau");

            migrationBuilder.DropIndex(
                name: "IX_ToTrinhThamDinhNhaThau_NhaThauId",
                table: "ToTrinhThamDinhNhaThau");

            migrationBuilder.DropColumn(
                name: "NhaThauId",
                table: "ToTrinhThamDinhNhaThau");

            migrationBuilder.AddColumn<string>(
                name: "TenNhaThau",
                table: "ToTrinhThamDinhNhaThau",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
