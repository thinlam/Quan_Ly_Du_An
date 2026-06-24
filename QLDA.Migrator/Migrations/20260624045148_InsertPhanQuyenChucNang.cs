using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class InsertPhanQuyenChucNang : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LoaiKeHoach",
                table: "KeHoachLuaChonNhaThau",
                type: "varchar(500)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PhanQuyenChucNang",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SuDung = table.Column<bool>(type: "bit", nullable: false),
                    MaChucNang = table.Column<string>(type: "varchar(100)", nullable: false),
                    ChucNang = table.Column<string>(type: "nvarchar(500)", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: true),
                    LevelId = table.Column<long>(type: "bigint", nullable: true),
                    NguoiDungMacDinh = table.Column<bool>(type: "bit", nullable: true),
                    NguoiDungId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhanQuyenChucNang", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PhanQuyenChucNang");

            migrationBuilder.DropColumn(
                name: "LoaiKeHoach",
                table: "KeHoachLuaChonNhaThau");
        }
    }
}
