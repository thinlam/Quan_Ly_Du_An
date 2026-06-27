using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class AddTableDanhMucLoaiCongViec : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LoaiCongViecId",
                table: "GoiThau",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DanhMucLoaiCongViec",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Stt = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
                    Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Used = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhMucLoaiCongViec", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GoiThau_LoaiCongViecId",
                table: "GoiThau",
                column: "LoaiCongViecId");

            migrationBuilder.CreateIndex(
                name: "IX_DanhMucLoaiCongViec_Index",
                table: "DanhMucLoaiCongViec",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_DanhMucLoaiCongViec_Ma",
                table: "DanhMucLoaiCongViec",
                column: "Ma",
                unique: true,
                filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

            migrationBuilder.AddForeignKey(
                name: "FK_GoiThau_DanhMucLoaiCongViec_LoaiCongViecId",
                table: "GoiThau",
                column: "LoaiCongViecId",
                principalTable: "DanhMucLoaiCongViec",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GoiThau_DanhMucLoaiCongViec_LoaiCongViecId",
                table: "GoiThau");

            migrationBuilder.DropTable(
                name: "DanhMucLoaiCongViec");

            migrationBuilder.DropIndex(
                name: "IX_GoiThau_LoaiCongViecId",
                table: "GoiThau");

            migrationBuilder.DropColumn(
                name: "LoaiCongViecId",
                table: "GoiThau");
        }
    }
}
