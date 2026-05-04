using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class ChangeKeHoachVonNguonVonIdToInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "NguonVonId",
                table: "KeHoachVon",
                type: "int",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_KeHoachVon_NguonVonId",
                table: "KeHoachVon",
                column: "NguonVonId");

            migrationBuilder.AddForeignKey(
                name: "FK_KeHoachVon_DmNguonVon_NguonVonId",
                table: "KeHoachVon",
                column: "NguonVonId",
                principalTable: "DmNguonVon",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KeHoachVon_DmNguonVon_NguonVonId",
                table: "KeHoachVon");

            migrationBuilder.DropIndex(
                name: "IX_KeHoachVon_NguonVonId",
                table: "KeHoachVon");

            migrationBuilder.AlterColumn<Guid>(
                name: "NguonVonId",
                table: "KeHoachVon",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
