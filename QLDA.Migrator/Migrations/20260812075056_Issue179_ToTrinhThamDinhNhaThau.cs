using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class Issue179_ToTrinhThamDinhNhaThau : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ToTrinhQuyetDinh_HoSoMoiThauDienTu_HoSoMoiThauQuyetDinhId",
                table: "ToTrinhQuyetDinh");

            migrationBuilder.DropForeignKey(
                name: "FK_ToTrinhQuyetDinh_HoSoMoiThauDienTu_HoSoMoiThauToTrinhId",
                table: "ToTrinhQuyetDinh");

            migrationBuilder.DropIndex(
                name: "IX_ToTrinhQuyetDinh_HoSoMoiThauQuyetDinhId",
                table: "ToTrinhQuyetDinh");

            migrationBuilder.DropIndex(
                name: "IX_ToTrinhQuyetDinh_HoSoMoiThauToTrinhId",
                table: "ToTrinhQuyetDinh");

            // Issue #179 — Thêm Loai TRƯỚC khi drop/rename 2 FK cũ để backfill đúng dữ liệu hiện có
            // (HoSoMoiThauToTrinhId → Loai=1, HoSoMoiThauQuyetDinhId → Loai=2), tránh mất dữ liệu cũ.
            migrationBuilder.AddColumn<int>(
                name: "Loai",
                table: "ToTrinhQuyetDinh",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"
                UPDATE ToTrinhQuyetDinh SET Loai = 1 WHERE HoSoMoiThauToTrinhId IS NOT NULL;
                UPDATE ToTrinhQuyetDinh SET Loai = 2 WHERE HoSoMoiThauQuyetDinhId IS NOT NULL;
            ");

            // Gộp giá trị HoSoMoiThauQuyetDinhId (nếu có) vào HoSoMoiThauToTrinhId trước khi cột này
            // được đổi tên thành EntityId — không để mất liên kết của các bản ghi QuyetDinh cũ.
            migrationBuilder.Sql(@"
                UPDATE ToTrinhQuyetDinh SET HoSoMoiThauToTrinhId = HoSoMoiThauQuyetDinhId
                WHERE HoSoMoiThauToTrinhId IS NULL AND HoSoMoiThauQuyetDinhId IS NOT NULL;
            ");

            migrationBuilder.DropColumn(
                name: "HoSoMoiThauQuyetDinhId",
                table: "ToTrinhQuyetDinh");

            migrationBuilder.RenameColumn(
                name: "HoSoMoiThauToTrinhId",
                table: "ToTrinhQuyetDinh",
                newName: "EntityId");

            migrationBuilder.AddColumn<int>(
                name: "NguoiKyChucVuId",
                table: "VanBanQuyetDinh",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TrangThaiDuyetId",
                table: "VanBanQuyetDinh",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GoiThauId",
                table: "ToTrinhThamDinhNhaThau",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "NgayKetThucDanhGia",
                table: "ToTrinhThamDinhNhaThau",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TenNhaThau",
                table: "ToTrinhThamDinhNhaThau",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ToTrinhThamDinhBuocXuLy",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ToTrinhId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    So = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Ngay = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Loai = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToTrinhThamDinhBuocXuLy", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ToTrinhThamDinhBuocXuLy_ToTrinhThamDinhNhaThau_ToTrinhId",
                        column: x => x.ToTrinhId,
                        principalTable: "ToTrinhThamDinhNhaThau",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "DmTrangThaiPheDuyet",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "IsDeleted", "Loai", "Ma", "MoTa", "Stt", "Ten", "UpdatedAt", "UpdatedBy", "Used" },
                values: new object[,]
                {
                    { 71, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "ToTrinhThamDinhNhaThau", "ĐTr", null, 1, "Đã trình", null, "", true },
                    { 72, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "ToTrinhThamDinhNhaThau", "ĐD", null, 2, "Đã duyệt", null, "", true }
                });

            migrationBuilder.CreateIndex(
                name: "IX_VanBanQuyetDinh_NguoiKyChucVuId",
                table: "VanBanQuyetDinh",
                column: "NguoiKyChucVuId");

            migrationBuilder.CreateIndex(
                name: "IX_VanBanQuyetDinh_TrangThaiDuyetId",
                table: "VanBanQuyetDinh",
                column: "TrangThaiDuyetId");

            migrationBuilder.CreateIndex(
                name: "IX_ToTrinhThamDinhNhaThau_GoiThauId",
                table: "ToTrinhThamDinhNhaThau",
                column: "GoiThauId");

            migrationBuilder.CreateIndex(
                name: "IX_ToTrinhQuyetDinh_EntityId_Loai",
                table: "ToTrinhQuyetDinh",
                columns: new[] { "EntityId", "Loai" });

            migrationBuilder.CreateIndex(
                name: "IX_ToTrinhThamDinhBuocXuLy_Index",
                table: "ToTrinhThamDinhBuocXuLy",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_ToTrinhThamDinhBuocXuLy_ToTrinhId_Loai",
                table: "ToTrinhThamDinhBuocXuLy",
                columns: new[] { "ToTrinhId", "Loai" });

            migrationBuilder.AddForeignKey(
                name: "FK_ToTrinhThamDinhNhaThau_GoiThau_GoiThauId",
                table: "ToTrinhThamDinhNhaThau",
                column: "GoiThauId",
                principalTable: "GoiThau",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VanBanQuyetDinh_DmChucVu_NguoiKyChucVuId",
                table: "VanBanQuyetDinh",
                column: "NguoiKyChucVuId",
                principalTable: "DmChucVu",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VanBanQuyetDinh_DmTrangThaiPheDuyet_TrangThaiDuyetId",
                table: "VanBanQuyetDinh",
                column: "TrangThaiDuyetId",
                principalTable: "DmTrangThaiPheDuyet",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ToTrinhThamDinhNhaThau_GoiThau_GoiThauId",
                table: "ToTrinhThamDinhNhaThau");

            migrationBuilder.DropForeignKey(
                name: "FK_VanBanQuyetDinh_DmChucVu_NguoiKyChucVuId",
                table: "VanBanQuyetDinh");

            migrationBuilder.DropForeignKey(
                name: "FK_VanBanQuyetDinh_DmTrangThaiPheDuyet_TrangThaiDuyetId",
                table: "VanBanQuyetDinh");

            migrationBuilder.DropTable(
                name: "ToTrinhThamDinhBuocXuLy");

            migrationBuilder.DropIndex(
                name: "IX_VanBanQuyetDinh_NguoiKyChucVuId",
                table: "VanBanQuyetDinh");

            migrationBuilder.DropIndex(
                name: "IX_VanBanQuyetDinh_TrangThaiDuyetId",
                table: "VanBanQuyetDinh");

            migrationBuilder.DropIndex(
                name: "IX_ToTrinhThamDinhNhaThau_GoiThauId",
                table: "ToTrinhThamDinhNhaThau");

            migrationBuilder.DropIndex(
                name: "IX_ToTrinhQuyetDinh_EntityId_Loai",
                table: "ToTrinhQuyetDinh");

            migrationBuilder.DeleteData(
                table: "DmTrangThaiPheDuyet",
                keyColumn: "Id",
                keyValue: 71);

            migrationBuilder.DeleteData(
                table: "DmTrangThaiPheDuyet",
                keyColumn: "Id",
                keyValue: 72);

            migrationBuilder.DropColumn(
                name: "NguoiKyChucVuId",
                table: "VanBanQuyetDinh");

            migrationBuilder.DropColumn(
                name: "TrangThaiDuyetId",
                table: "VanBanQuyetDinh");

            migrationBuilder.DropColumn(
                name: "GoiThauId",
                table: "ToTrinhThamDinhNhaThau");

            migrationBuilder.DropColumn(
                name: "NgayKetThucDanhGia",
                table: "ToTrinhThamDinhNhaThau");

            migrationBuilder.DropColumn(
                name: "TenNhaThau",
                table: "ToTrinhThamDinhNhaThau");

            migrationBuilder.DropColumn(
                name: "Loai",
                table: "ToTrinhQuyetDinh");

            migrationBuilder.RenameColumn(
                name: "EntityId",
                table: "ToTrinhQuyetDinh",
                newName: "HoSoMoiThauToTrinhId");

            migrationBuilder.AddColumn<Guid>(
                name: "HoSoMoiThauQuyetDinhId",
                table: "ToTrinhQuyetDinh",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ToTrinhQuyetDinh_HoSoMoiThauQuyetDinhId",
                table: "ToTrinhQuyetDinh",
                column: "HoSoMoiThauQuyetDinhId",
                unique: true,
                filter: "[HoSoMoiThauQuyetDinhId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ToTrinhQuyetDinh_HoSoMoiThauToTrinhId",
                table: "ToTrinhQuyetDinh",
                column: "HoSoMoiThauToTrinhId",
                unique: true,
                filter: "[HoSoMoiThauToTrinhId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_ToTrinhQuyetDinh_HoSoMoiThauDienTu_HoSoMoiThauQuyetDinhId",
                table: "ToTrinhQuyetDinh",
                column: "HoSoMoiThauQuyetDinhId",
                principalTable: "HoSoMoiThauDienTu",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ToTrinhQuyetDinh_HoSoMoiThauDienTu_HoSoMoiThauToTrinhId",
                table: "ToTrinhQuyetDinh",
                column: "HoSoMoiThauToTrinhId",
                principalTable: "HoSoMoiThauDienTu",
                principalColumn: "Id");
        }
    }
}
