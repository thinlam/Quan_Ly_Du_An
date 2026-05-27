using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class AddToTrinhKetQuaGoiThau : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TepDinhKem",
                table: "QuyetDinhDieuChinh");

            migrationBuilder.CreateTable(
                name: "ToTrinhKetQuaGoiThau",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuocId = table.Column<int>(type: "int", nullable: true),
                    So = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NgayToTrinh = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    TrichYeu = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    TrangThaiDangTaiId = table.Column<int>(type: "int", nullable: true),
                    TrangThaiId = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToTrinhKetQuaGoiThau", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ToTrinhKetQuaGoiThau_DmTrangThaiPheDuyet_TrangThaiId",
                        column: x => x.TrangThaiId,
                        principalTable: "DmTrangThaiPheDuyet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ToTrinhKetQuaGoiThau_DuAn_DuAnId",
                        column: x => x.DuAnId,
                        principalTable: "DuAn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GoiThauTrinhPheDuyetKetQua",
                columns: table => new
                {
                    ToTrinhId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GoiThauId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoiThauTrinhPheDuyetKetQua", x => new { x.ToTrinhId, x.GoiThauId });
                    table.ForeignKey(
                        name: "FK_GoiThauTrinhPheDuyetKetQua_ToTrinhKetQuaGoiThau_ToTrinhId",
                        column: x => x.ToTrinhId,
                        principalTable: "ToTrinhKetQuaGoiThau",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ToTrinhKetQuaGoiThau_DuAnId",
                table: "ToTrinhKetQuaGoiThau",
                column: "DuAnId");

            migrationBuilder.CreateIndex(
                name: "IX_ToTrinhKetQuaGoiThau_Index",
                table: "ToTrinhKetQuaGoiThau",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_ToTrinhKetQuaGoiThau_TrangThaiId",
                table: "ToTrinhKetQuaGoiThau",
                column: "TrangThaiId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GoiThauTrinhPheDuyetKetQua");

            migrationBuilder.DropTable(
                name: "ToTrinhKetQuaGoiThau");

            migrationBuilder.AddColumn<string>(
                name: "TepDinhKem",
                table: "QuyetDinhDieuChinh",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
