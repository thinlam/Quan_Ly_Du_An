using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations;

/// <inheritdoc />
public partial class FixDeXuatTrinhJunctionAndDropDanhMucDonViCbo : Migration {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder) {
        // 1. DeXuatTrinhKinhPhiNam — junction chỉ 2 cột: drop constraint trước, rồi mới drop cột dư
        migrationBuilder.Sql("""
            IF OBJECT_ID(N'DeXuatTrinhKinhPhiNam', N'U') IS NOT NULL
            BEGIN
            DECLARE @TableId INT = OBJECT_ID(N'DeXuatTrinhKinhPhiNam');
            DECLARE @Sql NVARCHAR(MAX);

            IF EXISTS (
                SELECT 1 FROM sys.foreign_keys
                WHERE name = N'FK_DeXuatTrinhKinhPhiNam_DeXuatNhuCauKinhPhiNam_DeXuatKinhPhiNamId'
            )
                ALTER TABLE [DeXuatTrinhKinhPhiNam] DROP CONSTRAINT [FK_DeXuatTrinhKinhPhiNam_DeXuatNhuCauKinhPhiNam_DeXuatKinhPhiNamId];

            SET @Sql = N'';
            SELECT @Sql = @Sql + N'ALTER TABLE [DeXuatTrinhKinhPhiNam] DROP CONSTRAINT [' + fk.name + N'];' + CHAR(13)
            FROM sys.foreign_keys fk
            WHERE fk.parent_object_id = @TableId
              AND fk.name <> N'FK_DeXuatTrinhKinhPhiNam_DeXuatNhuCauKinhPhiNam_DeXuatKinhPhiNamId';
            IF LEN(@Sql) > 0 EXEC sp_executesql @Sql;

            SET @Sql = N'';
            SELECT @Sql = @Sql + N'ALTER TABLE [DeXuatTrinhKinhPhiNam] DROP CONSTRAINT [' + kc.name + N'];' + CHAR(13)
            FROM sys.key_constraints kc
            WHERE kc.parent_object_id = @TableId AND kc.type IN (N'PK', N'UQ');
            IF LEN(@Sql) > 0 EXEC sp_executesql @Sql;

            SET @Sql = N'';
            SELECT @Sql = @Sql + N'ALTER TABLE [DeXuatTrinhKinhPhiNam] DROP CONSTRAINT [' + dc.name + N'];' + CHAR(13)
            FROM sys.default_constraints dc
            WHERE dc.parent_object_id = @TableId;
            IF LEN(@Sql) > 0 EXEC sp_executesql @Sql;

            SET @Sql = N'';
            SELECT @Sql = @Sql + N'DROP INDEX [' + i.name + N'] ON [DeXuatTrinhKinhPhiNam];' + CHAR(13)
            FROM sys.indexes i
            WHERE i.object_id = @TableId
              AND i.name IS NOT NULL
              AND i.is_primary_key = 0
              AND i.is_unique_constraint = 0;
            IF LEN(@Sql) > 0 EXEC sp_executesql @Sql;

            IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = @TableId AND name = N'Id')
                ALTER TABLE [DeXuatTrinhKinhPhiNam] DROP COLUMN [Id];
            IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = @TableId AND name = N'CreatedAt')
                ALTER TABLE [DeXuatTrinhKinhPhiNam] DROP COLUMN [CreatedAt];
            IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = @TableId AND name = N'CreatedBy')
                ALTER TABLE [DeXuatTrinhKinhPhiNam] DROP COLUMN [CreatedBy];
            IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = @TableId AND name = N'UpdatedAt')
                ALTER TABLE [DeXuatTrinhKinhPhiNam] DROP COLUMN [UpdatedAt];
            IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = @TableId AND name = N'UpdatedBy')
                ALTER TABLE [DeXuatTrinhKinhPhiNam] DROP COLUMN [UpdatedBy];
            IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = @TableId AND name = N'IsDeleted')
                ALTER TABLE [DeXuatTrinhKinhPhiNam] DROP COLUMN [IsDeleted];
            IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = @TableId AND name = N'Index')
                ALTER TABLE [DeXuatTrinhKinhPhiNam] DROP COLUMN [Index];

            IF NOT EXISTS (
                SELECT 1 FROM sys.key_constraints
                WHERE parent_object_id = @TableId AND name = N'PK_DeXuatTrinhKinhPhiNam'
            )
            AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = @TableId AND name = N'DeXuatKinhPhiNamId')
            AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = @TableId AND name = N'DeXuatNhuCauKinhPhiId')
                ALTER TABLE [DeXuatTrinhKinhPhiNam] ADD CONSTRAINT [PK_DeXuatTrinhKinhPhiNam]
                    PRIMARY KEY ([DeXuatKinhPhiNamId], [DeXuatNhuCauKinhPhiId]);

            IF NOT EXISTS (
                SELECT 1 FROM sys.foreign_keys
                WHERE name = N'FK_DeXuatTrinhKinhPhiNam_DeXuatNhuCauKinhPhiNam_DeXuatKinhPhiNamId'
            )
                ALTER TABLE [DeXuatTrinhKinhPhiNam] ADD CONSTRAINT [FK_DeXuatTrinhKinhPhiNam_DeXuatNhuCauKinhPhiNam_DeXuatKinhPhiNamId]
                    FOREIGN KEY ([DeXuatKinhPhiNamId]) REFERENCES [DeXuatNhuCauKinhPhiNam] ([Id]) ON DELETE CASCADE;
            END
            """);

        // DeXuatNhuCauKinhPhiNam — bỏ DuAnId (không thuộc aggregate tổng hợp KP năm)
        migrationBuilder.Sql("""
            IF EXISTS (
                SELECT 1 FROM sys.foreign_keys
                WHERE name = N'FK_DeXuatNhuCauKinhPhiNam_DuAn_DuAnId'
            )
                ALTER TABLE [DeXuatNhuCauKinhPhiNam] DROP CONSTRAINT [FK_DeXuatNhuCauKinhPhiNam_DuAn_DuAnId];
            """);

        migrationBuilder.Sql("""
            IF EXISTS (
                SELECT 1 FROM sys.indexes i
                INNER JOIN sys.tables t ON i.object_id = t.object_id
                WHERE t.name = N'DeXuatNhuCauKinhPhiNam' AND i.name = N'IX_DeXuatNhuCauKinhPhiNam_DuAnId'
            )
                DROP INDEX [IX_DeXuatNhuCauKinhPhiNam_DuAnId] ON [DeXuatNhuCauKinhPhiNam];
            """);

        migrationBuilder.Sql("""
            IF EXISTS (
                SELECT 1 FROM sys.columns
                WHERE object_id = OBJECT_ID(N'DeXuatNhuCauKinhPhiNam') AND name = N'DuAnId'
            )
                ALTER TABLE [DeXuatNhuCauKinhPhiNam] DROP COLUMN [DuAnId];
            """);

        // 2. DanhMucDonViCbo — chỉ DTO/query (DmDonVi + DeXuatDonViXuLy); drop bảng thừa trên DB dev nếu có
        migrationBuilder.Sql("""
            DECLARE @RefTableId INT = OBJECT_ID(N'[dbo].[DanhMucDonViCbo]');
            IF @RefTableId IS NOT NULL
            BEGIN
                DECLARE @DropFkSql NVARCHAR(MAX) = N'';

                SELECT @DropFkSql = @DropFkSql
                    + N'ALTER TABLE [' + OBJECT_SCHEMA_NAME(fk.parent_object_id) + N'].['
                    + OBJECT_NAME(fk.parent_object_id) + N'] DROP CONSTRAINT [' + fk.name + N'];' + CHAR(13)
                FROM sys.foreign_keys fk
                WHERE fk.referenced_object_id = @RefTableId;

                IF LEN(@DropFkSql) > 0
                    EXEC sp_executesql @DropFkSql;

                DROP TABLE [dbo].[DanhMucDonViCbo];
            END
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) {
        migrationBuilder.Sql("""
            IF NOT EXISTS (
                SELECT 1 FROM sys.columns
                WHERE object_id = OBJECT_ID(N'DeXuatNhuCauKinhPhiNam') AND name = N'DuAnId'
            )
                ALTER TABLE [DeXuatNhuCauKinhPhiNam] ADD [DuAnId] uniqueidentifier NULL;
            """);

        migrationBuilder.Sql("""
            IF NOT EXISTS (
                SELECT 1 FROM sys.indexes i
                INNER JOIN sys.tables t ON i.object_id = t.object_id
                WHERE t.name = N'DeXuatNhuCauKinhPhiNam' AND i.name = N'IX_DeXuatNhuCauKinhPhiNam_DuAnId'
            )
            AND EXISTS (
                SELECT 1 FROM sys.columns
                WHERE object_id = OBJECT_ID(N'DeXuatNhuCauKinhPhiNam') AND name = N'DuAnId'
            )
                CREATE INDEX [IX_DeXuatNhuCauKinhPhiNam_DuAnId] ON [DeXuatNhuCauKinhPhiNam] ([DuAnId]);
            """);

        migrationBuilder.Sql("""
            IF NOT EXISTS (
                SELECT 1 FROM sys.foreign_keys
                WHERE name = N'FK_DeXuatNhuCauKinhPhiNam_DuAn_DuAnId'
            )
            AND EXISTS (
                SELECT 1 FROM sys.columns
                WHERE object_id = OBJECT_ID(N'DeXuatNhuCauKinhPhiNam') AND name = N'DuAnId'
            )
                ALTER TABLE [DeXuatNhuCauKinhPhiNam] ADD CONSTRAINT [FK_DeXuatNhuCauKinhPhiNam_DuAn_DuAnId]
                    FOREIGN KEY ([DuAnId]) REFERENCES [DuAn] ([Id]);
            """);
    }
}
