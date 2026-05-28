using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class InsertToTrinhThamDinhNhaThau : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ToTrinhThamDinhNhaThau",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuocId = table.Column<int>(type: "int", nullable: true),
                    So = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NgayTrinh = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    TrichYeu = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    TrangThaiId = table.Column<int>(type: "int", nullable: true),
                    TrangThaiDangTaiId = table.Column<int>(type: "int", nullable: true),
                    TrangThaiThamDinhId = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToTrinhThamDinhNhaThau", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ToTrinhThamDinhNhaThau_DmTrangThaiPheDuyet_TrangThaiId",
                        column: x => x.TrangThaiId,
                        principalTable: "DmTrangThaiPheDuyet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ToTrinhThamDinhNhaThau_DmTrangThaiPheDuyet_TrangThaiThamDinhId",
                        column: x => x.TrangThaiThamDinhId,
                        principalTable: "DmTrangThaiPheDuyet",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ToTrinhThamDinhNhaThau_DuAn_DuAnId",
                        column: x => x.DuAnId,
                        principalTable: "DuAn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KetQuaThamDinhNhaThau",
                columns: table => new
                {
                    ToTrinhId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NhaThauId = table.Column<int>(type: "int", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KetQuaDanhGia = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KetQuaThamDinhNhaThau", x => new { x.ToTrinhId, x.NhaThauId });
                    table.ForeignKey(
                        name: "FK_KetQuaThamDinhNhaThau_ToTrinhThamDinhNhaThau_ToTrinhId",
                        column: x => x.ToTrinhId,
                        principalTable: "ToTrinhThamDinhNhaThau",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ToTrinhThamDinhNhaThau_DuAnId",
                table: "ToTrinhThamDinhNhaThau",
                column: "DuAnId");

            migrationBuilder.CreateIndex(
                name: "IX_ToTrinhThamDinhNhaThau_Index",
                table: "ToTrinhThamDinhNhaThau",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_ToTrinhThamDinhNhaThau_TrangThaiId",
                table: "ToTrinhThamDinhNhaThau",
                column: "TrangThaiId");

            migrationBuilder.CreateIndex(
                name: "IX_ToTrinhThamDinhNhaThau_TrangThaiThamDinhId",
                table: "ToTrinhThamDinhNhaThau",
                column: "TrangThaiThamDinhId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KetQuaThamDinhNhaThau");

            migrationBuilder.DropTable(
                name: "ToTrinhThamDinhNhaThau");
        }
    }
}
