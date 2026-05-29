using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class RemovePkTepdinhkem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TepDinhKem_DonViTuVanKeHoach_DonViTuVanKeHoachKeHoachId",
                table: "TepDinhKem");

            migrationBuilder.DropIndex(
                name: "IX_TepDinhKem_DonViTuVanKeHoachKeHoachId",
                table: "TepDinhKem");

            migrationBuilder.DropColumn(
                name: "DonViTuVanKeHoachKeHoachId",
                table: "TepDinhKem");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DonViTuVanKeHoachKeHoachId",
                table: "TepDinhKem",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TepDinhKem_DonViTuVanKeHoachKeHoachId",
                table: "TepDinhKem",
                column: "DonViTuVanKeHoachKeHoachId");

            migrationBuilder.AddForeignKey(
                name: "FK_TepDinhKem_DonViTuVanKeHoach_DonViTuVanKeHoachKeHoachId",
                table: "TepDinhKem",
                column: "DonViTuVanKeHoachKeHoachId",
                principalTable: "DonViTuVanKeHoach",
                principalColumn: "KeHoachLCNTId");
        }
    }
}
