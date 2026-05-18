using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class AddDuAnBuocPhongPhuTrachChinhVaPhongBanPhoiHop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "PhongPhuTrachChinhId",
                table: "DuAnBuoc",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AuditLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    EntityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OldValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrentValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangedColumns = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DuAnBuocPhongBanPhoiHop",
                columns: table => new
                {
                    DuAnBuocId = table.Column<int>(type: "int", nullable: false),
                    PhongBanId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DuAnBuocPhongBanPhoiHop", x => new { x.DuAnBuocId, x.PhongBanId });
                    table.ForeignKey(
                        name: "FK_DuAnBuocPhongBanPhoiHop_DuAnBuoc_DuAnBuocId",
                        column: x => x.DuAnBuocId,
                        principalTable: "DuAnBuoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_EntityId",
                table: "AuditLog",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_EntityName",
                table: "AuditLog",
                column: "EntityName");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_EntityName_EntityId",
                table: "AuditLog",
                columns: new[] { "EntityName", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_Index",
                table: "AuditLog",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLog");

            migrationBuilder.DropTable(
                name: "DuAnBuocPhongBanPhoiHop");

            migrationBuilder.DropColumn(
                name: "PhongPhuTrachChinhId",
                table: "DuAnBuoc");
        }
    }
}
