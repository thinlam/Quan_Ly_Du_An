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
            // SQL Server cannot ALTER COLUMN from uniqueidentifier to int directly.
            // Workflow: add new int column → drop old guid column → rename new to old name

            migrationBuilder.AddColumn<int>(
                name: "NguonVonId_New",
                table: "KeHoachVon",
                type: "int",
                nullable: true);

            migrationBuilder.DropColumn(
                name: "NguonVonId",
                table: "KeHoachVon");

            migrationBuilder.RenameColumn(
                name: "NguonVonId_New",
                table: "KeHoachVon",
                newName: "NguonVonId");

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

            // Reverse: add guid column → drop int column → rename back
            migrationBuilder.AddColumn<Guid>(
                name: "NguonVonId_Old",
                table: "KeHoachVon",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.DropColumn(
                name: "NguonVonId",
                table: "KeHoachVon");

            migrationBuilder.RenameColumn(
                name: "NguonVonId_Old",
                table: "KeHoachVon",
                newName: "NguonVonId");
        }
    }
}
