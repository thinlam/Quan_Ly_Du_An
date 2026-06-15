using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class TablePheDuyetHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ToTrinhKeHoach");

            migrationBuilder.AddColumn<int>(
                name: "BuocId",
                table: "PheDuyetHistory",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BuocId",
                table: "PheDuyet",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PheDuyet_BuocId",
                table: "PheDuyet",
                column: "BuocId");

            migrationBuilder.AddForeignKey(
                name: "FK_PheDuyet_DuAnBuoc_BuocId",
                table: "PheDuyet",
                column: "BuocId",
                principalTable: "DuAnBuoc",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PheDuyet_DuAnBuoc_BuocId",
                table: "PheDuyet");

            migrationBuilder.DropIndex(
                name: "IX_PheDuyet_BuocId",
                table: "PheDuyet");

            migrationBuilder.DropColumn(
                name: "BuocId",
                table: "PheDuyetHistory");

            migrationBuilder.DropColumn(
                name: "BuocId",
                table: "PheDuyet");

            migrationBuilder.CreateTable(
                name: "ToTrinhKeHoach",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrangThaiId = table.Column<int>(type: "int", nullable: true),
                    BuocId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    NgayToTrinh = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    So = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TrichYeu = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToTrinhKeHoach", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ToTrinhKeHoach_DmTrangThaiPheDuyet_TrangThaiId",
                        column: x => x.TrangThaiId,
                        principalTable: "DmTrangThaiPheDuyet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ToTrinhKeHoach_DuAn_DuAnId",
                        column: x => x.DuAnId,
                        principalTable: "DuAn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ToTrinhKeHoach_DuAnId",
                table: "ToTrinhKeHoach",
                column: "DuAnId");

            migrationBuilder.CreateIndex(
                name: "IX_ToTrinhKeHoach_Index",
                table: "ToTrinhKeHoach",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_ToTrinhKeHoach_TrangThaiId",
                table: "ToTrinhKeHoach",
                column: "TrangThaiId");
        }
    }
}
