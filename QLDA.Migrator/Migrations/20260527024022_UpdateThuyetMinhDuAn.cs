using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class UpdateThuyetMinhDuAn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           

            migrationBuilder.AddColumn<string>(
                name: "KetQuaThamDinh",
                table: "ThuyetMinhDuAn",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BuocId",
                table: "QuyetDinhDieuChinh",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ToTrinhPheDuyet",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuocId = table.Column<int>(type: "int", nullable: true),
                    So = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NgayToTrinh = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    TrichYeu = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Loai = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
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
                    table.PrimaryKey("PK_ToTrinhPheDuyet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ToTrinhPheDuyet_DmTrangThaiPheDuyet_TrangThaiId",
                        column: x => x.TrangThaiId,
                        principalTable: "DmTrangThaiPheDuyet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ToTrinhPheDuyet_DuAn_DuAnId",
                        column: x => x.DuAnId,
                        principalTable: "DuAn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ToTrinhPheDuyet_DuAnId",
                table: "ToTrinhPheDuyet",
                column: "DuAnId");

            migrationBuilder.CreateIndex(
                name: "IX_ToTrinhPheDuyet_Index",
                table: "ToTrinhPheDuyet",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_ToTrinhPheDuyet_TrangThaiId",
                table: "ToTrinhPheDuyet",
                column: "TrangThaiId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ToTrinhPheDuyet");

            migrationBuilder.DropColumn(
                name: "KetQuaThamDinh",
                table: "ThuyetMinhDuAn");

            migrationBuilder.DropColumn(
                name: "BuocId",
                table: "QuyetDinhDieuChinh");

            migrationBuilder.AddColumn<Guid>(
                name: "PheDuyetEntityId",
                table: "QuyetDinhDieuChinh",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "PheDuyetEntityName",
                table: "QuyetDinhDieuChinh",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_QuyetDinhDieuChinh_PheDuyetEntityName_PheDuyetEntityId",
                table: "QuyetDinhDieuChinh",
                columns: new[] { "PheDuyetEntityName", "PheDuyetEntityId" });
        }
    }
}
