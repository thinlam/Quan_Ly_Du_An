using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class UpdateConfigPheDuyetDuToan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DuToanId",
                table: "PheDuyetDuToan",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_PheDuyetDuToan_DuToanId",
                table: "PheDuyetDuToan",
                column: "DuToanId",
                unique: true,
                filter: "[DuToanId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_PheDuyetDuToan_DuToanDauTu_DuToanId",
                table: "PheDuyetDuToan",
                column: "DuToanId",
                principalTable: "DuToanDauTu",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PheDuyetDuToan_DuToanDauTu_DuToanId",
                table: "PheDuyetDuToan");

            migrationBuilder.DropIndex(
                name: "IX_PheDuyetDuToan_DuToanId",
                table: "PheDuyetDuToan");

            migrationBuilder.DropColumn(
                name: "DuToanId",
                table: "PheDuyetDuToan");
        }
    }
}
