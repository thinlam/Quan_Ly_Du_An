using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class updateKeHoachTrienKhai2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        

        

            migrationBuilder.CreateIndex(
                name: "IX_HangMucKeHoach_KeHoachId",
                table: "HangMucKeHoach",
                column: "KeHoachId");

            migrationBuilder.AddForeignKey(
                name: "FK_HangMucKeHoach_KeHoachTrienKhaiHangMuc_KeHoachId",
                table: "HangMucKeHoach",
                column: "KeHoachId",
                principalTable: "KeHoachTrienKhaiHangMuc",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_HangMucKeHoach_KeHoachTrienKhaiHangMucId",
                table: "HangMucKeHoach",
                column: "KeHoachId");

            migrationBuilder.AddForeignKey(
                name: "FK_HangMucKeHoach_KeHoachTrienKhaiHangMuc_KeHoachTrienKhaiHangMucId",
                table: "HangMucKeHoach",
                column: "KeHoachId",
                principalTable: "KeHoachTrienKhaiHangMuc",
                principalColumn: "Id");
        }
    }
}
