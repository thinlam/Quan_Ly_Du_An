using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class UpateTableKetQuaThamDinhNhaThau : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GoiThauId",
                table: "KetQuaThamDinhNhaThau",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_KetQuaThamDinhNhaThau_GoiThauId",
                table: "KetQuaThamDinhNhaThau",
                column: "GoiThauId");

            migrationBuilder.AddForeignKey(
                name: "FK_KetQuaThamDinhNhaThau_GoiThau_GoiThauId",
                table: "KetQuaThamDinhNhaThau",
                column: "GoiThauId",
                principalTable: "GoiThau",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KetQuaThamDinhNhaThau_GoiThau_GoiThauId",
                table: "KetQuaThamDinhNhaThau");

            migrationBuilder.DropIndex(
                name: "IX_KetQuaThamDinhNhaThau_GoiThauId",
                table: "KetQuaThamDinhNhaThau");

            migrationBuilder.DropColumn(
                name: "GoiThauId",
                table: "KetQuaThamDinhNhaThau");
        }
    }
}
