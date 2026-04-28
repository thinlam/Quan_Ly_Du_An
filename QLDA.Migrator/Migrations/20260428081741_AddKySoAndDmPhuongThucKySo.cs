using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class AddKySoAndDmPhuongThucKySo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DmPhuongThucKySo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Stt = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
                    Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Used = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DmPhuongThucKySo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KySo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    ChuSoHuuId = table.Column<long>(type: "bigint", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ChucVuId = table.Column<int>(type: "int", nullable: true),
                    PhamVi = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PhongBanId = table.Column<int>(type: "int", nullable: true),
                    SerialChungThu = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ToChucCap = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    HieuLucTu = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HieuLucDen = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PhuongThucKySoId = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KySo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KySo_DmChucVu_ChucVuId",
                        column: x => x.ChucVuId,
                        principalTable: "DmChucVu",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_KySo_DmPhuongThucKySo_PhuongThucKySoId",
                        column: x => x.PhuongThucKySoId,
                        principalTable: "DmPhuongThucKySo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DmPhuongThucKySo_Index",
                table: "DmPhuongThucKySo",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_DmPhuongThucKySo_Ma",
                table: "DmPhuongThucKySo",
                column: "Ma",
                unique: true,
                filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_KySo_ChucVuId",
                table: "KySo",
                column: "ChucVuId");

            migrationBuilder.CreateIndex(
                name: "IX_KySo_Index",
                table: "KySo",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_KySo_PhuongThucKySoId",
                table: "KySo",
                column: "PhuongThucKySoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KySo");

            migrationBuilder.DropTable(
                name: "DmPhuongThucKySo");
        }
    }
}
