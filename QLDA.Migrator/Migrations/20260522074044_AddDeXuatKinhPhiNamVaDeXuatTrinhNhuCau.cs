using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class AddDeXuatKinhPhiNamVaDeXuatTrinhNhuCau : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeXuatNhuCauKinhPhiNam",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    So = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NgayKeHoach = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    TrichYeu = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    TongKinhPhiDeXuat = table.Column<long>(type: "bigint", precision: 18, scale: 2, nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    TrangThaiId = table.Column<int>(type: "int", nullable: true),
                    DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeXuatNhuCauKinhPhiNam", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeXuatNhuCauKinhPhiNam_DmTrangThaiPheDuyet_TrangThaiId",
                        column: x => x.TrangThaiId,
                        principalTable: "DmTrangThaiPheDuyet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DeXuatNhuCauKinhPhiNam_DuAn_DuAnId",
                        column: x => x.DuAnId,
                        principalTable: "DuAn",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DeXuatTrinhKinhPhiNam",
                columns: table => new
                {
                    DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeXuatNhuCauKinhPhiId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeXuatTrinhKinhPhiNam", x => new { x.DuAnId, x.DeXuatNhuCauKinhPhiId });
                    table.ForeignKey(
                        name: "FK_DeXuatTrinhKinhPhiNam_DeXuatNhuCauKinhPhiNam_DuAnId",
                        column: x => x.DuAnId,
                        principalTable: "DeXuatNhuCauKinhPhiNam",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeXuatNhuCauKinhPhiNam_DuAnId",
                table: "DeXuatNhuCauKinhPhiNam",
                column: "DuAnId");

            migrationBuilder.CreateIndex(
                name: "IX_DeXuatNhuCauKinhPhiNam_Index",
                table: "DeXuatNhuCauKinhPhiNam",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_DeXuatNhuCauKinhPhiNam_TrangThaiId",
                table: "DeXuatNhuCauKinhPhiNam",
                column: "TrangThaiId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeXuatTrinhKinhPhiNam");

            migrationBuilder.DropTable(
                name: "DeXuatNhuCauKinhPhiNam");
        }
    }
}
