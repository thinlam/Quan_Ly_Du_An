using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class AddThuyetMinhDuAn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "NgayKhaoSat",
                table: "BaoCaoKetQuaKhaoSat",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "ThuyetMinhDuAn",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuocId = table.Column<int>(type: "int", nullable: true),
                    TrangThaiId = table.Column<int>(type: "int", nullable: true),
                    TrangThaiThamDinhId = table.Column<int>(type: "int", nullable: true),
                    NgayTrinh = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    So = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TrichYeu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThuyetMinhDuAn", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThuyetMinhDuAn_DmTrangThaiPheDuyet_TrangThaiId",
                        column: x => x.TrangThaiId,
                        principalTable: "DmTrangThaiPheDuyet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ThuyetMinhDuAn_DmTrangThaiPheDuyet_TrangThaiThamDinhId",
                        column: x => x.TrangThaiThamDinhId,
                        principalTable: "DmTrangThaiPheDuyet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ThuyetMinhDuAn_DuAn_DuAnId",
                        column: x => x.DuAnId,
                        principalTable: "DuAn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ThuyetMinhDuAn_DuAnId",
                table: "ThuyetMinhDuAn",
                column: "DuAnId");

            migrationBuilder.CreateIndex(
                name: "IX_ThuyetMinhDuAn_Index",
                table: "ThuyetMinhDuAn",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_ThuyetMinhDuAn_TrangThaiId",
                table: "ThuyetMinhDuAn",
                column: "TrangThaiId");

            migrationBuilder.CreateIndex(
                name: "IX_ThuyetMinhDuAn_TrangThaiThamDinhId",
                table: "ThuyetMinhDuAn",
                column: "TrangThaiThamDinhId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ThuyetMinhDuAn");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "NgayKhaoSat",
                table: "BaoCaoKetQuaKhaoSat",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");
        }
    }
}
