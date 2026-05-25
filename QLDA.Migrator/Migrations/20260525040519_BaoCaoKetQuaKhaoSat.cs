using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations;

/// <inheritdoc />
public partial class BaoCaoKetQuaKhaoSat : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Brownfield: bảng có thể đã tồn tại từ lần apply trước / migration cũ
        migrationBuilder.Sql("""
            IF OBJECT_ID(N'[dbo].[BaoCaoKetQuaKhaoSat]', N'U') IS NULL
            BEGIN
                CREATE TABLE [BaoCaoKetQuaKhaoSat] (
                    [Id] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
                    [DuAnId] uniqueidentifier NOT NULL,
                    [BuocId] int NULL,
                    [NoiDungBaoCao] nvarchar(4000) NULL,
                    [NoiDungNghiemThu] nvarchar(4000) NULL,
                    [NgayKhaoSat] datetimeoffset NULL,
                    [TrangThaiId] int NULL,
                    [NgayTrinh] datetimeoffset NULL,
                    [CreatedBy] nvarchar(max) NOT NULL,
                    [CreatedAt] datetimeoffset NOT NULL DEFAULT (SYSDATETIMEOFFSET()),
                    [UpdatedBy] nvarchar(max) NOT NULL,
                    [UpdatedAt] datetimeoffset NULL,
                    [IsDeleted] bit NOT NULL,
                    [Index] bigint NOT NULL DEFAULT (DATEDIFF(SECOND, '19700101', GETUTCDATE())),
                    CONSTRAINT [PK_BaoCaoKetQuaKhaoSat] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_BaoCaoKetQuaKhaoSat_DmTrangThaiPheDuyet_TrangThaiId] FOREIGN KEY ([TrangThaiId]) REFERENCES [DmTrangThaiPheDuyet] ([Id]) ON DELETE SET NULL,
                    CONSTRAINT [FK_BaoCaoKetQuaKhaoSat_DuAn_DuAnId] FOREIGN KEY ([DuAnId]) REFERENCES [DuAn] ([Id]) ON DELETE NO ACTION
                );

                CREATE INDEX [IX_BaoCaoKetQuaKhaoSat_DuAnId] ON [BaoCaoKetQuaKhaoSat] ([DuAnId]);
                CREATE NONCLUSTERED INDEX [IX_BaoCaoKetQuaKhaoSat_Index] ON [BaoCaoKetQuaKhaoSat] ([Index]);
                CREATE INDEX [IX_BaoCaoKetQuaKhaoSat_TrangThaiId] ON [BaoCaoKetQuaKhaoSat] ([TrangThaiId]);
            END
            """);

        migrationBuilder.Sql("""
            IF NOT EXISTS (SELECT 1 FROM [DmTrangThaiPheDuyet] WHERE [Id] = 26)
                INSERT INTO [DmTrangThaiPheDuyet] ([Id], [Ma], [Ten], [Loai], [Stt], [Used], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [MoTa])
                VALUES (26, N'DT', N'Dự thảo', N'BaoCaoKetQuaKhaoSat', 1, 1, 0, '2025-01-01T00:00:00.0000000+00:00', N'', NULL, N'', NULL);
            IF NOT EXISTS (SELECT 1 FROM [DmTrangThaiPheDuyet] WHERE [Id] = 27)
                INSERT INTO [DmTrangThaiPheDuyet] ([Id], [Ma], [Ten], [Loai], [Stt], [Used], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [MoTa])
                VALUES (27, N'ĐTr', N'Đã trình', N'BaoCaoKetQuaKhaoSat', 2, 1, 0, '2025-01-01T00:00:00.0000000+00:00', N'', NULL, N'', NULL);
            IF NOT EXISTS (SELECT 1 FROM [DmTrangThaiPheDuyet] WHERE [Id] = 28)
                INSERT INTO [DmTrangThaiPheDuyet] ([Id], [Ma], [Ten], [Loai], [Stt], [Used], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [MoTa])
                VALUES (28, N'ĐD', N'Đã duyệt', N'BaoCaoKetQuaKhaoSat', 3, 1, 0, '2025-01-01T00:00:00.0000000+00:00', N'', NULL, N'', NULL);
            IF NOT EXISTS (SELECT 1 FROM [DmTrangThaiPheDuyet] WHERE [Id] = 29)
                INSERT INTO [DmTrangThaiPheDuyet] ([Id], [Ma], [Ten], [Loai], [Stt], [Used], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [MoTa])
                VALUES (29, N'TL', N'Trả lại', N'BaoCaoKetQuaKhaoSat', 4, 1, 0, '2025-01-01T00:00:00.0000000+00:00', N'', NULL, N'', NULL);
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "BaoCaoKetQuaKhaoSat");

        migrationBuilder.DeleteData(
            table: "DmTrangThaiPheDuyet",
            keyColumn: "Id",
            keyValue: 26);

        migrationBuilder.DeleteData(
            table: "DmTrangThaiPheDuyet",
            keyColumn: "Id",
            keyValue: 27);

        migrationBuilder.DeleteData(
            table: "DmTrangThaiPheDuyet",
            keyColumn: "Id",
            keyValue: 28);

        migrationBuilder.DeleteData(
            table: "DmTrangThaiPheDuyet",
            keyColumn: "Id",
            keyValue: 29);
    }
}
