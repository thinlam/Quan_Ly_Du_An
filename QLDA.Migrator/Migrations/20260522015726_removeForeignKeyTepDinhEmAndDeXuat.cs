using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class removeForeignKeyTepDinhEmAndDeXuat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
         
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DeXuatChuTruongMoiId",
                table: "TepDinhKem",
                type: "uniqueidentifier",
                nullable: true);

        

            migrationBuilder.CreateIndex(
                name: "IX_TepDinhKem_DeXuatChuTruongMoiId",
                table: "TepDinhKem",
                column: "DeXuatChuTruongMoiId");

        }
    }
}
