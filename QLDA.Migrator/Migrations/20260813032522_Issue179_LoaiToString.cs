using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class Issue179_LoaiToString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Không còn dùng bộ trạng thái riêng cho VanBanQuyetDinh của Tờ trình thẩm định nhà thầu —
            // dùng lại chung nhóm DeXuatMacDinh (DT/ĐTr/ĐD/TL) sẵn có (Id=30..33).
            migrationBuilder.DeleteData(
                table: "DmTrangThaiPheDuyet",
                keyColumn: "Id",
                keyValue: 71);

            migrationBuilder.DeleteData(
                table: "DmTrangThaiPheDuyet",
                keyColumn: "Id",
                keyValue: 72);

            // Drop tường minh 2 index gắn với cột Loai TRƯỚC khi đổi cột — tránh để EF tự "bao bọc"
            // DROP/CREATE INDEX quanh AlterColumn (gây xung đột nếu chạy lại/đã xử lý tay trước đó).
            migrationBuilder.DropIndex(
                name: "IX_ToTrinhThamDinhBuocXuLy_ToTrinhId_Loai",
                table: "ToTrinhThamDinhBuocXuLy");

            migrationBuilder.DropIndex(
                name: "IX_ToTrinhQuyetDinh_EntityId_Loai",
                table: "ToTrinhQuyetDinh");

            // ToTrinhThamDinhBuocXuLy.Loai: int (1/2/3) -> string ("DoiChieu"/"ThuongThao"/"ThamDinh").
            // Backfill qua cột tạm trước khi drop cột int — không dùng AlterColumn trực tiếp vì SQL Server
            // sẽ cast số thành chuỗi số ("1") thay vì đúng giá trị nghiệp vụ ("DoiChieu").
            migrationBuilder.AddColumn<string>(
                name: "LoaiMoi",
                table: "ToTrinhThamDinhBuocXuLy",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE ToTrinhThamDinhBuocXuLy SET LoaiMoi = 'DoiChieu' WHERE Loai = 1;
                UPDATE ToTrinhThamDinhBuocXuLy SET LoaiMoi = 'ThuongThao' WHERE Loai = 2;
                UPDATE ToTrinhThamDinhBuocXuLy SET LoaiMoi = 'ThamDinh' WHERE Loai = 3;
                -- Fallback cho dữ liệu ngoài 1/2/3 (nếu có) — tránh chặn NOT NULL.
                UPDATE ToTrinhThamDinhBuocXuLy SET LoaiMoi = 'KhongXacDinh' WHERE LoaiMoi IS NULL;
            ");

            // Set NOT NULL bằng SQL thô (không dùng migrationBuilder.AlterColumn) khi cột còn tên "LoaiMoi"
            // (chưa có index nào gắn vào) — tránh để EF sinh thêm DROP/CREATE INDEX tự động quanh câu lệnh.
            migrationBuilder.Sql("ALTER TABLE ToTrinhThamDinhBuocXuLy ALTER COLUMN LoaiMoi nvarchar(50) NOT NULL;");

            migrationBuilder.DropColumn(name: "Loai", table: "ToTrinhThamDinhBuocXuLy");
            migrationBuilder.RenameColumn(name: "LoaiMoi", table: "ToTrinhThamDinhBuocXuLy", newName: "Loai");

            migrationBuilder.CreateIndex(
                name: "IX_ToTrinhThamDinhBuocXuLy_ToTrinhId_Loai",
                table: "ToTrinhThamDinhBuocXuLy",
                columns: new[] { "ToTrinhId", "Loai" });

            // ToTrinhQuyetDinh.Loai: int (1/2/3) -> string ("HoSoMoiThauToTrinh"/"HoSoMoiThauQuyetDinh"/"ToTrinhThamDinhNhaThau").
            migrationBuilder.AddColumn<string>(
                name: "LoaiMoi",
                table: "ToTrinhQuyetDinh",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE ToTrinhQuyetDinh SET LoaiMoi = 'HoSoMoiThauToTrinh' WHERE Loai = 1;
                UPDATE ToTrinhQuyetDinh SET LoaiMoi = 'HoSoMoiThauQuyetDinh' WHERE Loai = 2;
                UPDATE ToTrinhQuyetDinh SET LoaiMoi = 'ToTrinhThamDinhNhaThau' WHERE Loai = 3;
                -- Fallback cho dữ liệu rác/cũ có Loai ngoài 1/2/3 (VD Loai=0, EntityId=NULL, không gắn
                -- nghiệp vụ nào) — tránh chặn NOT NULL, không map bừa theo 1/2/3 vì không rõ nguồn gốc.
                UPDATE ToTrinhQuyetDinh SET LoaiMoi = 'KhongXacDinh' WHERE LoaiMoi IS NULL;
            ");

            migrationBuilder.Sql("ALTER TABLE ToTrinhQuyetDinh ALTER COLUMN LoaiMoi nvarchar(50) NOT NULL;");

            migrationBuilder.DropColumn(name: "Loai", table: "ToTrinhQuyetDinh");
            migrationBuilder.RenameColumn(name: "LoaiMoi", table: "ToTrinhQuyetDinh", newName: "Loai");

            migrationBuilder.CreateIndex(
                name: "IX_ToTrinhQuyetDinh_EntityId_Loai",
                table: "ToTrinhQuyetDinh",
                columns: new[] { "EntityId", "Loai" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ToTrinhQuyetDinh.Loai: string -> int (rollback schema; không đảm bảo giữ nguyên ngữ nghĩa
            // nếu có giá trị string mới phát sinh ngoài 3 giá trị đã biết khi Up chạy).
            migrationBuilder.DropIndex(
                name: "IX_ToTrinhQuyetDinh_EntityId_Loai",
                table: "ToTrinhQuyetDinh");

            migrationBuilder.DropIndex(
                name: "IX_ToTrinhThamDinhBuocXuLy_ToTrinhId_Loai",
                table: "ToTrinhThamDinhBuocXuLy");

            migrationBuilder.AddColumn<int>(
                name: "LoaiCu",
                table: "ToTrinhQuyetDinh",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE ToTrinhQuyetDinh SET LoaiCu = 1 WHERE Loai = 'HoSoMoiThauToTrinh';
                UPDATE ToTrinhQuyetDinh SET LoaiCu = 2 WHERE Loai = 'HoSoMoiThauQuyetDinh';
                UPDATE ToTrinhQuyetDinh SET LoaiCu = 3 WHERE Loai = 'ToTrinhThamDinhNhaThau';
                UPDATE ToTrinhQuyetDinh SET LoaiCu = 0 WHERE LoaiCu IS NULL;
            ");

            migrationBuilder.Sql("ALTER TABLE ToTrinhQuyetDinh ALTER COLUMN LoaiCu int NOT NULL;");

            migrationBuilder.DropColumn(name: "Loai", table: "ToTrinhQuyetDinh");
            migrationBuilder.RenameColumn(name: "LoaiCu", table: "ToTrinhQuyetDinh", newName: "Loai");

            migrationBuilder.CreateIndex(
                name: "IX_ToTrinhQuyetDinh_EntityId_Loai",
                table: "ToTrinhQuyetDinh",
                columns: new[] { "EntityId", "Loai" });

            migrationBuilder.AddColumn<int>(
                name: "LoaiCu",
                table: "ToTrinhThamDinhBuocXuLy",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE ToTrinhThamDinhBuocXuLy SET LoaiCu = 1 WHERE Loai = 'DoiChieu';
                UPDATE ToTrinhThamDinhBuocXuLy SET LoaiCu = 2 WHERE Loai = 'ThuongThao';
                UPDATE ToTrinhThamDinhBuocXuLy SET LoaiCu = 3 WHERE Loai = 'ThamDinh';
                UPDATE ToTrinhThamDinhBuocXuLy SET LoaiCu = 0 WHERE LoaiCu IS NULL;
            ");

            migrationBuilder.Sql("ALTER TABLE ToTrinhThamDinhBuocXuLy ALTER COLUMN LoaiCu int NOT NULL;");

            migrationBuilder.DropColumn(name: "Loai", table: "ToTrinhThamDinhBuocXuLy");
            migrationBuilder.RenameColumn(name: "LoaiCu", table: "ToTrinhThamDinhBuocXuLy", newName: "Loai");

            migrationBuilder.CreateIndex(
                name: "IX_ToTrinhThamDinhBuocXuLy_ToTrinhId_Loai",
                table: "ToTrinhThamDinhBuocXuLy",
                columns: new[] { "ToTrinhId", "Loai" });

            migrationBuilder.InsertData(
                table: "DmTrangThaiPheDuyet",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "IsDeleted", "Loai", "Ma", "MoTa", "Stt", "Ten", "UpdatedAt", "UpdatedBy", "Used" },
                values: new object[,]
                {
                    { 71, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "ToTrinhThamDinhNhaThau", "ĐTr", null, 1, "Đã trình", null, "", true },
                    { 72, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "ToTrinhThamDinhNhaThau", "ĐD", null, 2, "Đã duyệt", null, "", true }
                });
        }
    }
}
