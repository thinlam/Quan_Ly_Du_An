using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class Update1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NhaThauId",
                table: "KetQuaThamDinhNhaThau");

            migrationBuilder.AddColumn<Guid>(
                name: "NhaThauId_temp",
                table: "KetQuaThamDinhNhaThau",
                type: "uniqueidentifier",
                nullable: false);

            migrationBuilder.RenameColumn(
                name: "NhaThauId_temp",
                table: "KetQuaThamDinhNhaThau",
                newName: "NhaThauId");

            migrationBuilder.CreateIndex(
                name: "IX_KetQuaThamDinhNhaThau_NhaThauId",
                table: "KetQuaThamDinhNhaThau",
                column: "NhaThauId");

            migrationBuilder.AddForeignKey(
                name: "FK_KetQuaThamDinhNhaThau_DmNhaThau_NhaThauId",
                table: "KetQuaThamDinhNhaThau",
                column: "NhaThauId",
                principalTable: "DmNhaThau",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KetQuaThamDinhNhaThau_DmNhaThau_NhaThauId",
                table: "KetQuaThamDinhNhaThau");

            migrationBuilder.DropIndex(
                name: "IX_KetQuaThamDinhNhaThau_NhaThauId",
                table: "KetQuaThamDinhNhaThau");

            migrationBuilder.DropColumn(
                name: "NhaThauId",
                table: "KetQuaThamDinhNhaThau");

            migrationBuilder.AddColumn<int>(
                name: "NhaThauId_temp",
                table: "KetQuaThamDinhNhaThau",
                type: "int",
                nullable: false);

            migrationBuilder.RenameColumn(
                name: "NhaThauId_temp",
                table: "KetQuaThamDinhNhaThau",
                newName: "NhaThauId");
        }
    }
}
