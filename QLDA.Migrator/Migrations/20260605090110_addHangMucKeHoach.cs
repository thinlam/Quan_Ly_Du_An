using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class addHangMucKeHoach : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CanBoTrienKhaiHangMuc");

            migrationBuilder.DropColumn(
                name: "CanBoChuTriId",
                table: "KeHoachTrienKhaiHangMuc");

            migrationBuilder.DropColumn(
                name: "GiaiDoanId",
                table: "KeHoachTrienKhaiHangMuc");

            migrationBuilder.DropColumn(
                name: "KinhPhi",
                table: "KeHoachTrienKhaiHangMuc");

            migrationBuilder.DropColumn(
                name: "NgayBatDau",
                table: "KeHoachTrienKhaiHangMuc");

            migrationBuilder.DropColumn(
                name: "NgayKetThuc",
                table: "KeHoachTrienKhaiHangMuc");

            migrationBuilder.DropColumn(
                name: "TenHangMuc",
                table: "KeHoachTrienKhaiHangMuc");

            migrationBuilder.DropColumn(
                name: "ThoiHan",
                table: "KeHoachTrienKhaiHangMuc");

            migrationBuilder.CreateTable(
                name: "HangMucKeHoach",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    KeHoachId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GiaiDoanId = table.Column<int>(type: "int", nullable: true),
                    TenHangMuc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DonViChuTriId = table.Column<long>(type: "bigint", nullable: true),
                    DonViPhoiHopIds = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CanBoChuTriId = table.Column<long>(type: "bigint", nullable: true),
                    CanBoPhoiHopIds = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayBatDau = table.Column<DateOnly>(type: "date", nullable: true),
                    NgayKetThuc = table.Column<DateOnly>(type: "date", nullable: true),
                    ThoiHan = table.Column<DateOnly>(type: "date", nullable: true),
                    KinhPhi = table.Column<long>(type: "bigint", nullable: true),
                    TrangThaiId = table.Column<int>(type: "int", nullable: true),
                    KeHoachTrienKhaiHangMucId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HangMucKeHoach", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HangMucKeHoach_DM_DONVI_DonViChuTriId",
                        column: x => x.DonViChuTriId,
                        principalSchema: "dbo",
                        principalTable: "DM_DONVI",
                        principalColumn: "DonViID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HangMucKeHoach_DmGiaiDoan_GiaiDoanId",
                        column: x => x.GiaiDoanId,
                        principalTable: "DmGiaiDoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HangMucKeHoach_DmTrangThaiPheDuyet_TrangThaiId",
                        column: x => x.TrangThaiId,
                        principalTable: "DmTrangThaiPheDuyet",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HangMucKeHoach_KeHoachTrienKhaiHangMuc_KeHoachTrienKhaiHangMucId",
                        column: x => x.KeHoachTrienKhaiHangMucId,
                        principalTable: "KeHoachTrienKhaiHangMuc",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_HangMucKeHoach_DonViChuTriId",
                table: "HangMucKeHoach",
                column: "DonViChuTriId");

            migrationBuilder.CreateIndex(
                name: "IX_HangMucKeHoach_GiaiDoanId",
                table: "HangMucKeHoach",
                column: "GiaiDoanId");

            migrationBuilder.CreateIndex(
                name: "IX_HangMucKeHoach_Index",
                table: "HangMucKeHoach",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_HangMucKeHoach_KeHoachTrienKhaiHangMucId",
                table: "HangMucKeHoach",
                column: "KeHoachTrienKhaiHangMucId");

            migrationBuilder.CreateIndex(
                name: "IX_HangMucKeHoach_TrangThaiId",
                table: "HangMucKeHoach",
                column: "TrangThaiId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HangMucKeHoach");

            migrationBuilder.AddColumn<long>(
                name: "CanBoChuTriId",
                table: "KeHoachTrienKhaiHangMuc",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GiaiDoanId",
                table: "KeHoachTrienKhaiHangMuc",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "KinhPhi",
                table: "KeHoachTrienKhaiHangMuc",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "NgayBatDau",
                table: "KeHoachTrienKhaiHangMuc",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "NgayKetThuc",
                table: "KeHoachTrienKhaiHangMuc",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TenHangMuc",
                table: "KeHoachTrienKhaiHangMuc",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateOnly>(
                name: "ThoiHan",
                table: "KeHoachTrienKhaiHangMuc",
                type: "date",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CanBoTrienKhaiHangMuc",
                columns: table => new
                {
                    KeHoachId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CanBoId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CanBoTrienKhaiHangMuc", x => new { x.KeHoachId, x.CanBoId });
                    table.ForeignKey(
                        name: "FK_CanBoTrienKhaiHangMuc_KeHoachTrienKhaiHangMuc_KeHoachId",
                        column: x => x.KeHoachId,
                        principalTable: "KeHoachTrienKhaiHangMuc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }
    }
}
