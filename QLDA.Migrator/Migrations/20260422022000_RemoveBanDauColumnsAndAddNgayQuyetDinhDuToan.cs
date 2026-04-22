using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBanDauColumnsAndAddNgayQuyetDinhDuToan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop all indexes related to DuToanBanDau columns
            migrationBuilder.Sql(@"
                IF EXISTS(SELECT 1 FROM sys.indexes WHERE name = 'IX_DuAn_DuToanBanDauId' AND object_id = OBJECT_ID('DuAn'))
                    DROP INDEX [IX_DuAn_DuToanBanDauId] ON [DuAn];
            ");

            migrationBuilder.Sql(@"
                IF EXISTS(SELECT 1 FROM sys.indexes WHERE name = 'IX_DuAn_DuToanBanDauId1' AND object_id = OBJECT_ID('DuAn'))
                    DROP INDEX [IX_DuAn_DuToanBanDauId1] ON [DuAn];
            ");

            // Drop foreign key if exists
            migrationBuilder.Sql(@"
                IF OBJECT_ID('FK_DuAn_DuToan_DuToanBanDauId1', 'F') IS NOT NULL
                    ALTER TABLE [DuAn] DROP CONSTRAINT [FK_DuAn_DuToan_DuToanBanDauId1];
            ");

            // Drop columns if they exist
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'DuAn' AND COLUMN_NAME = 'SoDuToanBanDau')
                    ALTER TABLE [DuAn] DROP COLUMN [SoDuToanBanDau];
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'DuAn' AND COLUMN_NAME = 'SoTienDuToanBanDau')
                    ALTER TABLE [DuAn] DROP COLUMN [SoTienDuToanBanDau];
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'DuAn' AND COLUMN_NAME = 'DuToanBanDauId1')
                    ALTER TABLE [DuAn] DROP COLUMN [DuToanBanDauId1];
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'DuAn' AND COLUMN_NAME = 'DuToanBanDauId')
                    ALTER TABLE [DuAn] DROP COLUMN [DuToanBanDauId];
            ");

            // Rename NgayKyDuToan to NgayQuyetDinhDuToan if NgayKyDuToan exists
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'DuAn' AND COLUMN_NAME = 'NgayKyDuToan')
                    EXEC sp_rename 'DuAn.NgayKyDuToan', 'NgayQuyetDinhDuToan', 'COLUMN';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rename back NgayQuyetDinhDuToan to NgayKyDuToan
            migrationBuilder.RenameColumn(
                name: "NgayQuyetDinhDuToan",
                table: "DuAn",
                newName: "NgayKyDuToan");

            // Add back columns that were removed
            migrationBuilder.AddColumn<long>(
                name: "DuToanBanDauId",
                table: "DuAn",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DuToanBanDauId1",
                table: "DuAn",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SoTienDuToanBanDau",
                table: "DuAn",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SoDuToanBanDau",
                table: "DuAn",
                type: "bigint",
                nullable: true);

            // Recreate index
            migrationBuilder.CreateIndex(
                name: "IX_DuAn_DuToanBanDauId1",
                table: "DuAn",
                column: "DuToanBanDauId1");

            // Recreate foreign key
            migrationBuilder.AddForeignKey(
                name: "FK_DuAn_DuToan_DuToanBanDauId1",
                table: "DuAn",
                column: "DuToanBanDauId1",
                principalTable: "DuToan",
                principalColumn: "Id");
        }
    }
}
