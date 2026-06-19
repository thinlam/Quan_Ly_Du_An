using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class TableChiDinhThau : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ChiDinhThauId",
                table: "HoSoMoiThauDienTu",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ChiDinhThau",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HoSoMoiThauId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    So = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ngay = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    TrichYeu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NguoiKy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayKy = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ChucVu = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiDinhThau", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChiDinhThau_HoSoMoiThauDienTu_HoSoMoiThauId",
                        column: x => x.HoSoMoiThauId,
                        principalTable: "HoSoMoiThauDienTu",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChiDinhThau_HoSoMoiThauId",
                table: "ChiDinhThau",
                column: "HoSoMoiThauId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChiDinhThau_Index",
                table: "ChiDinhThau",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChiDinhThau");

            migrationBuilder.DropColumn(
                name: "ChiDinhThauId",
                table: "HoSoMoiThauDienTu");
        }
    }
}
