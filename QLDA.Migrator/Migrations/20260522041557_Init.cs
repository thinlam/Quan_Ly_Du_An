using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder){}
        // protected override void Up(MigrationBuilder migrationBuilder)
        // {
        //     migrationBuilder.CreateTable(
        //         name: "AuditLog",
        //         columns: table => new
        //         {
        //             Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
        //             EntityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
        //             EntityId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
        //             Action = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
        //             OldValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             NewValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             CurrentValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             ChangedColumns = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_AuditLog", x => x.Id);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "DanhMucLoaiDieuChinh",
        //         columns: table => new
        //         {
        //             Id = table.Column<int>(type: "int", nullable: false)
        //                 .Annotation("SqlServer:Identity", "1, 1"),
        //             Stt = table.Column<int>(type: "int", nullable: true),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
        //             Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
        //             Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
        //             MoTa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
        //             Used = table.Column<bool>(type: "bit", nullable: false)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_DanhMucLoaiDieuChinh", x => x.Id);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "DmCapDoCntt",
        //         columns: table => new
        //         {
        //             Id = table.Column<int>(type: "int", nullable: false)
        //                 .Annotation("SqlServer:Identity", "1, 1"),
        //             Stt = table.Column<int>(type: "int", nullable: true),
        //             MaMau = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
        //             Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
        //             Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
        //             MoTa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
        //             Used = table.Column<bool>(type: "bit", nullable: false)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_DmCapDoCntt", x => x.Id);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "DmChucVu",
        //         columns: table => new
        //         {
        //             Id = table.Column<int>(type: "int", nullable: false)
        //                 .Annotation("SqlServer:Identity", "1, 1"),
        //             Stt = table.Column<int>(type: "int", nullable: true),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
        //             Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
        //             Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
        //             MoTa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
        //             Used = table.Column<bool>(type: "bit", nullable: false)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_DmChucVu", x => x.Id);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "DmChuDauTu",
        //         columns: table => new
        //         {
        //             Id = table.Column<int>(type: "int", nullable: false)
        //                 .Annotation("SqlServer:Identity", "1, 1"),
        //             Stt = table.Column<int>(type: "int", nullable: true),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
        //             Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
        //             Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
        //             MoTa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
        //             Used = table.Column<bool>(type: "bit", nullable: false)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_DmChuDauTu", x => x.Id);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "DmGiaiDoan",
        //         columns: table => new
        //         {
        //             Id = table.Column<int>(type: "int", nullable: false)
        //                 .Annotation("SqlServer:Identity", "1, 1"),
        //             Stt = table.Column<int>(type: "int", nullable: true),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
        //             Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
        //             Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
        //             MoTa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
        //             Used = table.Column<bool>(type: "bit", nullable: false)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_DmGiaiDoan", x => x.Id);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "DmHinhThucDauTu",
        //         columns: table => new
        //         {
        //             Id = table.Column<int>(type: "int", nullable: false)
        //                 .Annotation("SqlServer:Identity", "1, 1"),
        //             Stt = table.Column<int>(type: "int", nullable: true),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
        //             Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
        //             Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
        //             MoTa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
        //             Used = table.Column<bool>(type: "bit", nullable: false)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_DmHinhThucDauTu", x => x.Id);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "DmHinhThucLuaChonNhaThau",
        //         columns: table => new
        //         {
        //             Id = table.Column<int>(type: "int", nullable: false)
        //                 .Annotation("SqlServer:Identity", "1, 1"),
        //             Stt = table.Column<int>(type: "int", nullable: true),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
        //             Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
        //             Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
        //             MoTa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
        //             Used = table.Column<bool>(type: "bit", nullable: false)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_DmHinhThucLuaChonNhaThau", x => x.Id);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "DmHinhThucQuanLy",
        //         columns: table => new
        //         {
        //             Id = table.Column<int>(type: "int", nullable: false)
        //                 .Annotation("SqlServer:Identity", "1, 1"),
        //             Stt = table.Column<int>(type: "int", nullable: true),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
        //             Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
        //             Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
        //             MoTa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
        //             Used = table.Column<bool>(type: "bit", nullable: false)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_DmHinhThucQuanLy", x => x.Id);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "DmLinhVuc",
        //         columns: table => new
        //         {
        //             Id = table.Column<int>(type: "int", nullable: false)
        //                 .Annotation("SqlServer:Identity", "1, 1"),
        //             Stt = table.Column<int>(type: "int", nullable: true),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
        //             Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
        //             Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
        //             MoTa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
        //             Used = table.Column<bool>(type: "bit", nullable: false)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_DmLinhVuc", x => x.Id);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "DmLoaiDuAn",
        //         columns: table => new
        //         {
        //             Id = table.Column<int>(type: "int", nullable: false)
        //                 .Annotation("SqlServer:Identity", "1, 1"),
        //             Stt = table.Column<int>(type: "int", nullable: true),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
        //             Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
        //             Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
        //             MoTa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
        //             Used = table.Column<bool>(type: "bit", nullable: false)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_DmLoaiDuAn", x => x.Id);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "DmLoaiDuAnTheoNam",
        //         columns: table => new
        //         {
        //             Id = table.Column<int>(type: "int", nullable: false)
        //                 .Annotation("SqlServer:Identity", "1, 1"),
        //             Stt = table.Column<int>(type: "int", nullable: true),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
        //             Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
        //             Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
        //             MoTa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
        //             Used = table.Column<bool>(type: "bit", nullable: false)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_DmLoaiDuAnTheoNam", x => x.Id);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "DmLoaiGoiThau",
        //         columns: table => new
        //         {
        //             Id = table.Column<int>(type: "int", nullable: false)
        //                 .Annotation("SqlServer:Identity", "1, 1"),
        //             Stt = table.Column<int>(type: "int", nullable: true),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
        //             Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
        //             Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
        //             MoTa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
        //             Used = table.Column<bool>(type: "bit", nullable: false)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_DmLoaiGoiThau", x => x.Id);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "DmLoaiHopDong",
        //         columns: table => new
        //         {
        //             Id = table.Column<int>(type: "int", nullable: false)
        //                 .Annotation("SqlServer:Identity", "1, 1"),
        //             Stt = table.Column<int>(type: "int", nullable: true),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
        //             Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
        //             Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
        //             MoTa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
        //             Used = table.Column<bool>(type: "bit", nullable: false)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_DmLoaiHopDong", x => x.Id);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "DmLoaiVanBan",
        //         columns: table => new
        //         {
        //             Id = table.Column<int>(type: "int", nullable: false)
        //                 .Annotation("SqlServer:Identity", "1, 1"),
        //             Stt = table.Column<int>(type: "int", nullable: true),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
        //             Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
        //             Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
        //             MoTa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
        //             Used = table.Column<bool>(type: "bit", nullable: false)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_DmLoaiVanBan", x => x.Id);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "DmMucDoKhoKhan",
        //         columns: table => new
        //         {
        //             Id = table.Column<int>(type: "int", nullable: false)
        //                 .Annotation("SqlServer:Identity", "1, 1"),
        //             Stt = table.Column<int>(type: "int", nullable: true),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
        //             Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
        //             Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
        //             MoTa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
        //             Used = table.Column<bool>(type: "bit", nullable: false)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_DmMucDoKhoKhan", x => x.Id);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "DmNguonVon",
        //         columns: table => new
        //         {
        //             Id = table.Column<int>(type: "int", nullable: false)
        //                 .Annotation("SqlServer:Identity", "1, 1"),
        //             Stt = table.Column<int>(type: "int", nullable: true),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
        //             Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
        //             Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
        //             MoTa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
        //             Used = table.Column<bool>(type: "bit", nullable: false)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_DmNguonVon", x => x.Id);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "DmNhaThau",
        //         columns: table => new
        //         {
        //             Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
        //             Stt = table.Column<int>(type: "int", nullable: true),
        //             DiaChi = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             MaSoThue = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             SoDienThoai = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             NguoiDaiDien = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
        //             Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
        //             Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
        //             MoTa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
        //             Used = table.Column<bool>(type: "bit", nullable: false)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_DmNhaThau", x => x.Id);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "DmNhomDuAn",
        //         columns: table => new
        //         {
        //             Id = table.Column<int>(type: "int", nullable: false)
        //                 .Annotation("SqlServer:Identity", "1, 1"),
        //             Stt = table.Column<int>(type: "int", nullable: true),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
        //             Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
        //             Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
        //             MoTa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
        //             Used = table.Column<bool>(type: "bit", nullable: false)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_DmNhomDuAn", x => x.Id);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "DmPhuongThucKySo",
        //         columns: table => new
        //         {
        //             Id = table.Column<int>(type: "int", nullable: false)
        //                 .Annotation("SqlServer:Identity", "1, 1"),
        //             Stt = table.Column<int>(type: "int", nullable: true),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
        //             Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
        //             Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
        //             MoTa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
        //             Used = table.Column<bool>(type: "bit", nullable: false)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_DmPhuongThucKySo", x => x.Id);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "DmPhuongThucLuaChonNhaThau",
        //         columns: table => new
        //         {
        //             Id = table.Column<int>(type: "int", nullable: false)
        //                 .Annotation("SqlServer:Identity", "1, 1"),
        //             Stt = table.Column<int>(type: "int", nullable: true),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
        //             Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
        //             Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
        //             MoTa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
        //             Used = table.Column<bool>(type: "bit", nullable: false)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_DmPhuongThucLuaChonNhaThau", x => x.Id);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "DmQuyen",
        //         columns: table => new
        //         {
        //             NhomQuyen = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
        //             Id = table.Column<int>(type: "int", nullable: false)
        //                 .Annotation("SqlServer:Identity", "1, 1"),
        //             Stt = table.Column<int>(type: "int", nullable: true),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
        //             Ma = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
        //             Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
        //             MoTa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
        //             Used = table.Column<bool>(type: "bit", nullable: false)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_DmQuyen", x => x.Id);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "DmQuyTrinh",
        //         columns: table => new
        //         {
        //             Id = table.Column<int>(type: "int", nullable: false)
        //                 .Annotation("SqlServer:Identity", "1, 1"),
        //             Stt = table.Column<int>(type: "int", nullable: true),
        //             MacDinh = table.Column<bool>(type: "bit", nullable: false),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
        //             Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
        //             Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
        //             MoTa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
        //             Used = table.Column<bool>(type: "bit", nullable: false)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_DmQuyTrinh", x => x.Id);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "DmTinhTrangKhoKhan",
        //         columns: table => new
        //         {
        //             Id = table.Column<int>(type: "int", nullable: false)
        //                 .Annotation("SqlServer:Identity", "1, 1"),
        //             Stt = table.Column<int>(type: "int", nullable: true),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
        //             Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
        //             Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
        //             MoTa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
        //             Used = table.Column<bool>(type: "bit", nullable: false)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_DmTinhTrangKhoKhan", x => x.Id);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "DmTinhTrangThucHienLcnt",
        //         columns: table => new
        //         {
        //             Id = table.Column<int>(type: "int", nullable: false)
        //                 .Annotation("SqlServer:Identity", "1, 1"),
        //             Stt = table.Column<int>(type: "int", nullable: true),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
        //             Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
        //             Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
        //             MoTa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
        //             Used = table.Column<bool>(type: "bit", nullable: false)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_DmTinhTrangThucHienLcnt", x => x.Id);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "DmTrangThaiDuAn",
        //         columns: table => new
        //         {
        //             Id = table.Column<int>(type: "int", nullable: false)
        //                 .Annotation("SqlServer:Identity", "1, 1"),
        //             Stt = table.Column<int>(type: "int", nullable: true),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
        //             Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
        //             Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
        //             MoTa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
        //             Used = table.Column<bool>(type: "bit", nullable: false)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_DmTrangThaiDuAn", x => x.Id);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "DmTrangThaiPheDuyet",
        //         columns: table => new
        //         {
        //             Id = table.Column<int>(type: "int", nullable: false)
        //                 .Annotation("SqlServer:Identity", "1, 1"),
        //             Stt = table.Column<int>(type: "int", nullable: true),
        //             Loai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
        //             Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
        //             Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
        //             MoTa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
        //             Used = table.Column<bool>(type: "bit", nullable: false)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_DmTrangThaiPheDuyet", x => x.Id);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "DmTrangThaiTienDo",
        //         columns: table => new
        //         {
        //             Id = table.Column<int>(type: "int", nullable: false)
        //                 .Annotation("SqlServer:Identity", "1, 1"),
        //             Stt = table.Column<int>(type: "int", nullable: true),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
        //             Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
        //             Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
        //             MoTa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
        //             Used = table.Column<bool>(type: "bit", nullable: false)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_DmTrangThaiTienDo", x => x.Id);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "E_LoaiVanBanQuyetDinh",
        //         columns: table => new
        //         {
        //             Id = table.Column<int>(type: "int", nullable: false)
        //                 .Annotation("SqlServer:Identity", "1, 1"),
        //             Used = table.Column<bool>(type: "bit", nullable: false),
        //             Ma = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
        //             Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_E_LoaiVanBanQuyetDinh", x => x.Id);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "E_ManHinh",
        //         columns: table => new
        //         {
        //             Id = table.Column<int>(type: "int", nullable: false)
        //                 .Annotation("SqlServer:Identity", "1, 1"),
        //             Used = table.Column<bool>(type: "bit", nullable: false),
        //             Label = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             Stt = table.Column<int>(type: "int", nullable: true),
        //             Ma = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
        //             Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_E_ManHinh", x => x.Id);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "TepDinhKem",
        //         columns: table => new
        //         {
        //             Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
        //             ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
        //             GroupId = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             GroupType = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             FileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             OriginalName = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             Path = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             Size = table.Column<long>(type: "bigint", nullable: false),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_TepDinhKem", x => x.Id);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "USER_MASTER",
        //         columns: table => new
        //         {
        //             User_MasterID = table.Column<long>(type: "bigint", nullable: false)
        //                 .Annotation("SqlServer:Identity", "1, 1"),
        //             UserName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
        //             HoTen = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
        //             PhongBanID = table.Column<long>(type: "bigint", nullable: true),
        //             DonViID = table.Column<long>(type: "bigint", nullable: false),
        //             User_PortalID = table.Column<long>(type: "bigint", nullable: true),
        //             CanBoID = table.Column<long>(type: "bigint", nullable: true),
        //             LaDonViChinh = table.Column<bool>(type: "bit", nullable: true),
        //             Used = table.Column<bool>(type: "bit", nullable: true)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK__USER_MAS__CA9BC5E270CE69C2", x => x.User_MasterID);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "NhaThauNguoiDung",
        //         columns: table => new
        //         {
        //             Id = table.Column<int>(type: "int", nullable: false)
        //                 .Annotation("SqlServer:Identity", "1, 1"),
        //             NhaThauId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
        //             NguoiDungId = table.Column<long>(type: "bigint", nullable: false),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_NhaThauNguoiDung", x => x.Id);
        //             table.ForeignKey(
        //                 name: "FK_NhaThauNguoiDung_DmNhaThau_NhaThauId",
        //                 column: x => x.NhaThauId,
        //                 principalTable: "DmNhaThau",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Cascade);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "KySo",
        //         columns: table => new
        //         {
        //             Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
        //             ChuSoHuuId = table.Column<long>(type: "bigint", nullable: true),
        //             Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
        //             ChucVuId = table.Column<int>(type: "int", nullable: true),
        //             PhamVi = table.Column<int>(type: "int", nullable: true),
        //             PhongBanId = table.Column<int>(type: "int", nullable: true),
        //             SerialChungThu = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
        //             ToChucCap = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
        //             HieuLucTu = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             HieuLucDen = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             PhuongThucKySoId = table.Column<int>(type: "int", nullable: true),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_KySo", x => x.Id);
        //             table.ForeignKey(
        //                 name: "FK_KySo_DmChucVu_ChucVuId",
        //                 column: x => x.ChucVuId,
        //                 principalTable: "DmChucVu",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.SetNull);
        //             table.ForeignKey(
        //                 name: "FK_KySo_DmPhuongThucKySo_PhuongThucKySoId",
        //                 column: x => x.PhuongThucKySoId,
        //                 principalTable: "DmPhuongThucKySo",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.SetNull);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "CauHinhVaiTroQuyen",
        //         columns: table => new
        //         {
        //             VaiTro = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
        //             QuyenId = table.Column<int>(type: "int", nullable: false),
        //             KichHoat = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
        //             Id = table.Column<int>(type: "int", nullable: false)
        //                 .Annotation("SqlServer:Identity", "1, 1"),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_CauHinhVaiTroQuyen", x => x.Id);
        //             table.ForeignKey(
        //                 name: "FK_CauHinhVaiTroQuyen_DmQuyen_QuyenId",
        //                 column: x => x.QuyenId,
        //                 principalTable: "DmQuyen",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Cascade);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "DmBuoc",
        //         columns: table => new
        //         {
        //             Id = table.Column<int>(type: "int", nullable: false)
        //                 .Annotation("SqlServer:Identity", "1, 1"),
        //             QuyTrinhId = table.Column<int>(type: "int", nullable: false),
        //             GiaiDoanId = table.Column<int>(type: "int", nullable: true),
        //             ParentId = table.Column<int>(type: "int", nullable: true),
        //             Stt = table.Column<int>(type: "int", nullable: true),
        //             PartialView = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             SoNgayThucHien = table.Column<int>(type: "int", nullable: false, defaultValueSql: "1"),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
        //             Path = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             Level = table.Column<int>(type: "int", nullable: false),
        //             Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
        //             Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
        //             MoTa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
        //             Used = table.Column<bool>(type: "bit", nullable: false)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_DmBuoc", x => x.Id);
        //             table.ForeignKey(
        //                 name: "FK_DmBuoc_DmGiaiDoan_GiaiDoanId",
        //                 column: x => x.GiaiDoanId,
        //                 principalTable: "DmGiaiDoan",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.SetNull);
        //             table.ForeignKey(
        //                 name: "FK_DmBuoc_DmQuyTrinh_QuyTrinhId",
        //                 column: x => x.QuyTrinhId,
        //                 principalTable: "DmQuyTrinh",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Restrict);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "DmBuocManHinh",
        //         columns: table => new
        //         {
        //             BuocId = table.Column<int>(type: "int", nullable: false),
        //             ManHinhId = table.Column<int>(type: "int", nullable: false),
        //             Stt = table.Column<int>(type: "int", nullable: true)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_DmBuocManHinh", x => new { x.BuocId, x.ManHinhId });
        //             table.ForeignKey(
        //                 name: "FK_DmBuocManHinh_DmBuoc_BuocId",
        //                 column: x => x.BuocId,
        //                 principalTable: "DmBuoc",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Cascade);
        //             table.ForeignKey(
        //                 name: "FK_DmBuocManHinh_E_ManHinh_ManHinhId",
        //                 column: x => x.ManHinhId,
        //                 principalTable: "E_ManHinh",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Cascade);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "DmBuocTrangThaiTienDo",
        //         columns: table => new
        //         {
        //             Id = table.Column<int>(type: "int", nullable: false)
        //                 .Annotation("SqlServer:Identity", "1, 1"),
        //             BuocId = table.Column<int>(type: "int", nullable: false),
        //             TrangThaiId = table.Column<int>(type: "int", nullable: false),
        //             Stt = table.Column<int>(type: "int", nullable: true),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
        //             Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
        //             Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
        //             MoTa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
        //             Used = table.Column<bool>(type: "bit", nullable: false)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_DmBuocTrangThaiTienDo", x => x.Id);
        //             table.ForeignKey(
        //                 name: "FK_DmBuocTrangThaiTienDo_DmBuoc_BuocId",
        //                 column: x => x.BuocId,
        //                 principalTable: "DmBuoc",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Cascade);
        //             table.ForeignKey(
        //                 name: "FK_DmBuocTrangThaiTienDo_DmTrangThaiTienDo_TrangThaiId",
        //                 column: x => x.TrangThaiId,
        //                 principalTable: "DmTrangThaiTienDo",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Cascade);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "BanGiaoHoSo",
        //         columns: table => new
        //         {
        //             Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
        //             Ma = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
        //             TenHoSo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
        //             DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
        //             BuocId = table.Column<int>(type: "int", nullable: true),
        //             PhongBanChuTriId = table.Column<long>(type: "bigint", nullable: true),
        //             GhiChu = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
        //             TrangThai = table.Column<int>(type: "int", nullable: false),
        //             NgayBanGiao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             PhongBanNhanId = table.Column<long>(type: "bigint", nullable: true),
        //             CreatedBy = table.Column<string>(type: "nvarchar(450)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_BanGiaoHoSo", x => x.Id);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "BaoCao",
        //         columns: table => new
        //         {
        //             Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
        //             DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
        //             BuocId = table.Column<int>(type: "int", nullable: true),
        //             Ngay = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             Loai = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_BaoCao", x => x.Id);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "BaoCaoBanGiaoSanPham",
        //         columns: table => new
        //         {
        //             Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
        //             DonViBanGiaoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
        //             DonViNhanBanGiaoId = table.Column<long>(type: "bigint", nullable: true)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_BaoCaoBanGiaoSanPham", x => x.Id);
        //             table.ForeignKey(
        //                 name: "FK_BaoCaoBanGiaoSanPham_BaoCao_Id",
        //                 column: x => x.Id,
        //                 principalTable: "BaoCao",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Cascade);
        //             table.ForeignKey(
        //                 name: "FK_BaoCaoBanGiaoSanPham_DmNhaThau_DonViBanGiaoId",
        //                 column: x => x.DonViBanGiaoId,
        //                 principalTable: "DmNhaThau",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Restrict);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "BaoCaoBaoHanhSanPham",
        //         columns: table => new
        //         {
        //             Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
        //             LanhDaoPhuTrachId = table.Column<long>(type: "bigint", nullable: true)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_BaoCaoBaoHanhSanPham", x => x.Id);
        //             table.ForeignKey(
        //                 name: "FK_BaoCaoBaoHanhSanPham_BaoCao_Id",
        //                 column: x => x.Id,
        //                 principalTable: "BaoCao",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Cascade);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "BaoCaoKhoKhanVuongMac",
        //         columns: table => new
        //         {
        //             Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
        //             MucDoKhoKhanId = table.Column<int>(type: "int", nullable: true),
        //             TinhTrangId = table.Column<int>(type: "int", nullable: true),
        //             HuongXuLy = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             KetQuaXuLy = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             NgayXuLy = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_BaoCaoKhoKhanVuongMac", x => x.Id);
        //             table.ForeignKey(
        //                 name: "FK_BaoCaoKhoKhanVuongMac_BaoCao_Id",
        //                 column: x => x.Id,
        //                 principalTable: "BaoCao",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Cascade);
        //             table.ForeignKey(
        //                 name: "FK_BaoCaoKhoKhanVuongMac_DmMucDoKhoKhan_MucDoKhoKhanId",
        //                 column: x => x.MucDoKhoKhanId,
        //                 principalTable: "DmMucDoKhoKhan",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Restrict);
        //             table.ForeignKey(
        //                 name: "FK_BaoCaoKhoKhanVuongMac_DmTinhTrangKhoKhan_TinhTrangId",
        //                 column: x => x.TinhTrangId,
        //                 principalTable: "DmTinhTrangKhoKhan",
        //                 principalColumn: "Id");
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "BaoCaoTienDo",
        //         columns: table => new
        //         {
        //             Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()")
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_BaoCaoTienDo", x => x.Id);
        //             table.ForeignKey(
        //                 name: "FK_BaoCaoTienDo_BaoCao_Id",
        //                 column: x => x.Id,
        //                 principalTable: "BaoCao",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Cascade);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "DangTaiKeHoachLcntLenMang",
        //         columns: table => new
        //         {
        //             Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
        //             DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
        //             BuocId = table.Column<int>(type: "int", nullable: true),
        //             KeHoachLuaChonNhaThauId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
        //             NgayEHSMT = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             TrangThaiId = table.Column<int>(type: "int", nullable: false),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_DangTaiKeHoachLcntLenMang", x => x.Id);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "DeXuatChuTruongMoi",
        //         columns: table => new
        //         {
        //             Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
        //             DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
        //             BuocId = table.Column<int>(type: "int", nullable: true),
        //             TomTatNoiDung = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
        //             TongMucDauTu = table.Column<long>(type: "bigint", precision: 18, scale: 2, nullable: true),
        //             NgayBatDauDuKien = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             HinhThucDauTuId = table.Column<int>(type: "int", nullable: true),
        //             LanhDaoPhuTrachId = table.Column<long>(type: "bigint", nullable: true),
        //             NguoiXuLyChinhId = table.Column<long>(type: "bigint", nullable: true),
        //             DonViPhuTrachChinhId = table.Column<long>(type: "bigint", nullable: true),
        //             TrangThaiId = table.Column<int>(type: "int", nullable: true),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_DeXuatChuTruongMoi", x => x.Id);
        //             table.ForeignKey(
        //                 name: "FK_DeXuatChuTruongMoi_DmHinhThucDauTu_HinhThucDauTuId",
        //                 column: x => x.HinhThucDauTuId,
        //                 principalTable: "DmHinhThucDauTu",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.SetNull);
        //             table.ForeignKey(
        //                 name: "FK_DeXuatChuTruongMoi_DmTrangThaiPheDuyet_TrangThaiId",
        //                 column: x => x.TrangThaiId,
        //                 principalTable: "DmTrangThaiPheDuyet",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Restrict);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "DeXuatDonViXuLy",
        //         columns: table => new
        //         {
        //             DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
        //             DonViId = table.Column<long>(type: "bigint", nullable: false)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_DeXuatDonViXuLy", x => new { x.DuAnId, x.DonViId });
        //             table.ForeignKey(
        //                 name: "FK_DeXuatDonViXuLy_DeXuatChuTruongMoi_DuAnId",
        //                 column: x => x.DuAnId,
        //                 principalTable: "DeXuatChuTruongMoi",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Cascade);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "DeXuatChuyenTiep",
        //         columns: table => new
        //         {
        //             Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
        //             DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
        //             BuocId = table.Column<int>(type: "int", nullable: true),
        //             SoLieuGiaiNgan = table.Column<long>(type: "bigint", precision: 18, scale: 2, nullable: true),
        //             UocGiaiNgan = table.Column<long>(type: "bigint", precision: 18, scale: 2, nullable: true),
        //             NhuCauKinhPhi = table.Column<long>(type: "bigint", precision: 18, scale: 2, nullable: true),
        //             KhoiLuongThucTe = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
        //             KhoiLuongDuKien = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
        //             TrangThaiId = table.Column<int>(type: "int", nullable: true),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_DeXuatChuyenTiep", x => x.Id);
        //             table.ForeignKey(
        //                 name: "FK_DeXuatChuyenTiep_DmTrangThaiPheDuyet_TrangThaiId",
        //                 column: x => x.TrangThaiId,
        //                 principalTable: "DmTrangThaiPheDuyet",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Restrict);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "DeXuatNhuCauKinhPhi",
        //         columns: table => new
        //         {
        //             Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
        //             DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
        //             BuocId = table.Column<int>(type: "int", nullable: true),
        //             DonViDeXuatId = table.Column<long>(type: "bigint", nullable: true),
        //             SoPhieuChuyen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
        //             NgayPhieuChuyen = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             TrichYeu = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
        //             TrangThaiId = table.Column<int>(type: "int", nullable: true),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_DeXuatNhuCauKinhPhi", x => x.Id);
        //             table.ForeignKey(
        //                 name: "FK_DeXuatNhuCauKinhPhi_DmTrangThaiPheDuyet_TrangThaiId",
        //                 column: x => x.TrangThaiId,
        //                 principalTable: "DmTrangThaiPheDuyet",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Restrict);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "DuAn",
        //         columns: table => new
        //         {
        //             Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
        //             ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
        //             TenDuAn = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             DiaDiem = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             ChuDauTuId = table.Column<int>(type: "int", nullable: true),
        //             ThoiGianKhoiCong = table.Column<int>(type: "int", nullable: true),
        //             ThoiGianHoanThanh = table.Column<int>(type: "int", nullable: true),
        //             MaDuAn = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             MaNganSach = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             DuAnTrongDiem = table.Column<bool>(type: "bit", nullable: false),
        //             LinhVucId = table.Column<int>(type: "int", nullable: true),
        //             NhomDuAnId = table.Column<int>(type: "int", nullable: true),
        //             NangLucThietKe = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             QuyMoDuAn = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             HinhThucQuanLyDuAnId = table.Column<int>(type: "int", nullable: true),
        //             HinhThucDauTuId = table.Column<int>(type: "int", nullable: true),
        //             LoaiDuAnId = table.Column<int>(type: "int", nullable: true),
        //             TongMucDauTu = table.Column<long>(type: "bigint", nullable: true),
        //             KhaiToanKinhPhi = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
        //             SoQuyetDinhPheDuyet = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
        //             NgayQuyetDinhPheDuyet = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             QuyTrinhId = table.Column<int>(type: "int", nullable: true),
        //             BuocHienTaiId = table.Column<int>(type: "int", nullable: true),
        //             GiaiDoanHienTaiId = table.Column<int>(type: "int", nullable: true),
        //             TrangThaiHienTaiId = table.Column<int>(type: "int", nullable: true),
        //             TrangThaiDuAnId = table.Column<int>(type: "int", nullable: true),
        //             LoaiDuAnTheoNamId = table.Column<int>(type: "int", nullable: true),
        //             GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             NgayBatDau = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             LanhDaoPhuTrachId = table.Column<long>(type: "bigint", nullable: true),
        //             DonViPhuTrachChinhId = table.Column<long>(type: "bigint", nullable: true),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
        //             Path = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             Level = table.Column<int>(type: "int", nullable: false)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_DuAn", x => x.Id);
        //             table.ForeignKey(
        //                 name: "FK_DuAn_DmChuDauTu_ChuDauTuId",
        //                 column: x => x.ChuDauTuId,
        //                 principalTable: "DmChuDauTu",
        //                 principalColumn: "Id");
        //             table.ForeignKey(
        //                 name: "FK_DuAn_DmGiaiDoan_GiaiDoanHienTaiId",
        //                 column: x => x.GiaiDoanHienTaiId,
        //                 principalTable: "DmGiaiDoan",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.SetNull);
        //             table.ForeignKey(
        //                 name: "FK_DuAn_DmHinhThucDauTu_HinhThucDauTuId",
        //                 column: x => x.HinhThucDauTuId,
        //                 principalTable: "DmHinhThucDauTu",
        //                 principalColumn: "Id");
        //             table.ForeignKey(
        //                 name: "FK_DuAn_DmHinhThucQuanLy_HinhThucQuanLyDuAnId",
        //                 column: x => x.HinhThucQuanLyDuAnId,
        //                 principalTable: "DmHinhThucQuanLy",
        //                 principalColumn: "Id");
        //             table.ForeignKey(
        //                 name: "FK_DuAn_DmLinhVuc_LinhVucId",
        //                 column: x => x.LinhVucId,
        //                 principalTable: "DmLinhVuc",
        //                 principalColumn: "Id");
        //             table.ForeignKey(
        //                 name: "FK_DuAn_DmLoaiDuAnTheoNam_LoaiDuAnTheoNamId",
        //                 column: x => x.LoaiDuAnTheoNamId,
        //                 principalTable: "DmLoaiDuAnTheoNam",
        //                 principalColumn: "Id");
        //             table.ForeignKey(
        //                 name: "FK_DuAn_DmLoaiDuAn_LoaiDuAnId",
        //                 column: x => x.LoaiDuAnId,
        //                 principalTable: "DmLoaiDuAn",
        //                 principalColumn: "Id");
        //             table.ForeignKey(
        //                 name: "FK_DuAn_DmNhomDuAn_NhomDuAnId",
        //                 column: x => x.NhomDuAnId,
        //                 principalTable: "DmNhomDuAn",
        //                 principalColumn: "Id");
        //             table.ForeignKey(
        //                 name: "FK_DuAn_DmQuyTrinh_QuyTrinhId",
        //                 column: x => x.QuyTrinhId,
        //                 principalTable: "DmQuyTrinh",
        //                 principalColumn: "Id");
        //             table.ForeignKey(
        //                 name: "FK_DuAn_DmTrangThaiDuAn_TrangThaiDuAnId",
        //                 column: x => x.TrangThaiDuAnId,
        //                 principalTable: "DmTrangThaiDuAn",
        //                 principalColumn: "Id");
        //             table.ForeignKey(
        //                 name: "FK_DuAn_DmTrangThaiTienDo_TrangThaiHienTaiId",
        //                 column: x => x.TrangThaiHienTaiId,
        //                 principalTable: "DmTrangThaiTienDo",
        //                 principalColumn: "Id");
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "DuAnBuoc",
        //         columns: table => new
        //         {
        //             Id = table.Column<int>(type: "int", nullable: false)
        //                 .Annotation("SqlServer:Identity", "1, 1"),
        //             BuocId = table.Column<int>(type: "int", nullable: false),
        //             DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
        //             TenBuoc = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             PartialView = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             Used = table.Column<bool>(type: "bit", nullable: false),
        //             TrangThaiId = table.Column<int>(type: "int", nullable: true),
        //             NgayDuKienBatDau = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             NgayDuKienKetThuc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             NgayThucTeBatDau = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             NgayThucTeKetThuc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             TrachNhiemThucHien = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             IsKetThuc = table.Column<bool>(type: "bit", nullable: false),
        //             PhongPhuTrachChinhId = table.Column<long>(type: "bigint", nullable: true),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_DuAnBuoc", x => x.Id);
        //             table.ForeignKey(
        //                 name: "FK_DuAnBuoc_DmBuoc_BuocId",
        //                 column: x => x.BuocId,
        //                 principalTable: "DmBuoc",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Cascade);
        //             table.ForeignKey(
        //                 name: "FK_DuAnBuoc_DuAn_DuAnId",
        //                 column: x => x.DuAnId,
        //                 principalTable: "DuAn",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Restrict);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "DuAnChiuTrachNhiemXuLy",
        //         columns: table => new
        //         {
        //             DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
        //             ChiuTrachNhiemXuLyId = table.Column<long>(type: "bigint", nullable: false),
        //             Loai = table.Column<string>(type: "nvarchar(max)", nullable: false)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_DuAnChiuTrachNhiemXuLy", x => new { x.DuAnId, x.ChiuTrachNhiemXuLyId });
        //             table.ForeignKey(
        //                 name: "FK_DuAnChiuTrachNhiemXuLy_DuAn_DuAnId",
        //                 column: x => x.DuAnId,
        //                 principalTable: "DuAn",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Cascade);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "DuAnCongViec",
        //         columns: table => new
        //         {
        //             DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
        //             CongViecId = table.Column<long>(type: "bigint", nullable: false),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
        //             IsHoanThanh = table.Column<bool>(type: "bit", nullable: true),
        //             NguoiPhuTrachChinhId = table.Column<long>(type: "bigint", nullable: true),
        //             NguoiTaoId = table.Column<long>(type: "bigint", nullable: true)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_DuAnCongViec", x => new { x.DuAnId, x.CongViecId });
        //             table.ForeignKey(
        //                 name: "FK_DuAnCongViec_DuAn_DuAnId",
        //                 column: x => x.DuAnId,
        //                 principalTable: "DuAn",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Cascade);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "DuAnNguonVon",
        //         columns: table => new
        //         {
        //             DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
        //             NguonVonId = table.Column<int>(type: "int", nullable: false)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_DuAnNguonVon", x => new { x.DuAnId, x.NguonVonId });
        //             table.ForeignKey(
        //                 name: "FK_DuAnNguonVon_DmNguonVon_NguonVonId",
        //                 column: x => x.NguonVonId,
        //                 principalTable: "DmNguonVon",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Restrict);
        //             table.ForeignKey(
        //                 name: "FK_DuAnNguonVon_DuAn_DuAnId",
        //                 column: x => x.DuAnId,
        //                 principalTable: "DuAn",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Cascade);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "DuToan",
        //         columns: table => new
        //         {
        //             Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
        //             DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
        //             SoDuToan = table.Column<long>(type: "bigint", nullable: false),
        //             NamDuToan = table.Column<int>(type: "int", nullable: false),
        //             SoQuyetDinhDuToan = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
        //             NgayKyDuToan = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_DuToan", x => x.Id);
        //             table.ForeignKey(
        //                 name: "FK_DuToan_DuAn_DuAnId",
        //                 column: x => x.DuAnId,
        //                 principalTable: "DuAn",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Cascade);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "HoSoDeXuatCapDoCntt",
        //         columns: table => new
        //         {
        //             Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
        //             DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
        //             BuocId = table.Column<int>(type: "int", nullable: true),
        //             TrangThaiId = table.Column<int>(type: "int", nullable: true),
        //             CapDoId = table.Column<int>(type: "int", nullable: true),
        //             NgayTrinh = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             DonViChuTriId = table.Column<int>(type: "int", nullable: true),
        //             NoiDungDeNghi = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
        //             NoiDungBaoCao = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
        //             NoiDungDuThao = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_HoSoDeXuatCapDoCntt", x => x.Id);
        //             table.ForeignKey(
        //                 name: "FK_HoSoDeXuatCapDoCntt_DmCapDoCntt_CapDoId",
        //                 column: x => x.CapDoId,
        //                 principalTable: "DmCapDoCntt",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.SetNull);
        //             table.ForeignKey(
        //                 name: "FK_HoSoDeXuatCapDoCntt_DmTrangThaiPheDuyet_TrangThaiId",
        //                 column: x => x.TrangThaiId,
        //                 principalTable: "DmTrangThaiPheDuyet",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.SetNull);
        //             table.ForeignKey(
        //                 name: "FK_HoSoDeXuatCapDoCntt_DuAn_DuAnId",
        //                 column: x => x.DuAnId,
        //                 principalTable: "DuAn",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Cascade);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "KeHoachVon",
        //         columns: table => new
        //         {
        //             Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
        //             DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
        //             NguonVonId = table.Column<int>(type: "int", nullable: true),
        //             Nam = table.Column<int>(type: "int", nullable: false),
        //             SoVon = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
        //             SoVonDieuChinh = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
        //             SoQuyetDinh = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
        //             NgayKy = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_KeHoachVon", x => x.Id);
        //             table.ForeignKey(
        //                 name: "FK_KeHoachVon_DmNguonVon_NguonVonId",
        //                 column: x => x.NguonVonId,
        //                 principalTable: "DmNguonVon",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Restrict);
        //             table.ForeignKey(
        //                 name: "FK_KeHoachVon_DuAn_DuAnId",
        //                 column: x => x.DuAnId,
        //                 principalTable: "DuAn",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Cascade);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "PhanKhaiKinhPhi",
        //         columns: table => new
        //         {
        //             Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
        //             DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
        //             SoToTrinh = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
        //             NgayToTrinh = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             NguonVonId = table.Column<int>(type: "int", nullable: true),
        //             KinhPhiDeXuat = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
        //             KinhPhiPhanKhai = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
        //             ThuyetMinh = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             TrangThaiId = table.Column<int>(type: "int", nullable: true),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_PhanKhaiKinhPhi", x => x.Id);
        //             table.ForeignKey(
        //                 name: "FK_PhanKhaiKinhPhi_DmNguonVon_NguonVonId",
        //                 column: x => x.NguonVonId,
        //                 principalTable: "DmNguonVon",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Restrict);
        //             table.ForeignKey(
        //                 name: "FK_PhanKhaiKinhPhi_DmTrangThaiPheDuyet_TrangThaiId",
        //                 column: x => x.TrangThaiId,
        //                 principalTable: "DmTrangThaiPheDuyet",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Restrict);
        //             table.ForeignKey(
        //                 name: "FK_PhanKhaiKinhPhi_DuAn_DuAnId",
        //                 column: x => x.DuAnId,
        //                 principalTable: "DuAn",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Restrict);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "PheDuyetHistory",
        //         columns: table => new
        //         {
        //             Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
        //             EntityName = table.Column<string>(type: "nvarchar(450)", nullable: false),
        //             EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
        //             DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
        //             NguoiXuLyId = table.Column<long>(type: "bigint", nullable: true),
        //             TrangThaiId = table.Column<int>(type: "int", nullable: true),
        //             NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             NgayXuLy = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_PheDuyetHistory", x => x.Id);
        //             table.ForeignKey(
        //                 name: "FK_PheDuyetHistory_DmTrangThaiPheDuyet_TrangThaiId",
        //                 column: x => x.TrangThaiId,
        //                 principalTable: "DmTrangThaiPheDuyet",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Restrict);
        //             table.ForeignKey(
        //                 name: "FK_PheDuyetHistory_DuAn_DuAnId",
        //                 column: x => x.DuAnId,
        //                 principalTable: "DuAn",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Restrict);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "QuyetDinhDieuChinh",
        //         columns: table => new
        //         {
        //             Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
        //             PheDuyetEntityName = table.Column<string>(type: "nvarchar(450)", nullable: false),
        //             PheDuyetEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
        //             DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
        //             SoQuyetDinh = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             NgayQuyetDinh = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             TrichYeu = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             LoaiDieuChinhId = table.Column<int>(type: "int", nullable: false),
        //             LyDo = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             TepDinhKem = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             TrangThaiId = table.Column<int>(type: "int", nullable: false),
        //             Lan = table.Column<int>(type: "int", nullable: false),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_QuyetDinhDieuChinh", x => x.Id);
        //             table.ForeignKey(
        //                 name: "FK_QuyetDinhDieuChinh_DanhMucLoaiDieuChinh_LoaiDieuChinhId",
        //                 column: x => x.LoaiDieuChinhId,
        //                 principalTable: "DanhMucLoaiDieuChinh",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Restrict);
        //             table.ForeignKey(
        //                 name: "FK_QuyetDinhDieuChinh_DmTrangThaiPheDuyet_TrangThaiId",
        //                 column: x => x.TrangThaiId,
        //                 principalTable: "DmTrangThaiPheDuyet",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Restrict);
        //             table.ForeignKey(
        //                 name: "FK_QuyetDinhDieuChinh_DuAn_DuAnId",
        //                 column: x => x.DuAnId,
        //                 principalTable: "DuAn",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Restrict);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "ToTrinhKeHoach",
        //         columns: table => new
        //         {
        //             Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
        //             DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
        //             BuocId = table.Column<int>(type: "int", nullable: true),
        //             So = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
        //             NgayToTrinh = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             TrichYeu = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
        //             TrangThaiId = table.Column<int>(type: "int", nullable: true),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_ToTrinhKeHoach", x => x.Id);
        //             table.ForeignKey(
        //                 name: "FK_ToTrinhKeHoach_DmTrangThaiPheDuyet_TrangThaiId",
        //                 column: x => x.TrangThaiId,
        //                 principalTable: "DmTrangThaiPheDuyet",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Restrict);
        //             table.ForeignKey(
        //                 name: "FK_ToTrinhKeHoach_DuAn_DuAnId",
        //                 column: x => x.DuAnId,
        //                 principalTable: "DuAn",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Restrict);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "DuAnBuocManHinh",
        //         columns: table => new
        //         {
        //             BuocId = table.Column<int>(type: "int", nullable: false),
        //             ManHinhId = table.Column<int>(type: "int", nullable: false),
        //             Stt = table.Column<int>(type: "int", nullable: true)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_DuAnBuocManHinh", x => new { x.BuocId, x.ManHinhId });
        //             table.ForeignKey(
        //                 name: "FK_DuAnBuocManHinh_DuAnBuoc_BuocId",
        //                 column: x => x.BuocId,
        //                 principalTable: "DuAnBuoc",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Cascade);
        //             table.ForeignKey(
        //                 name: "FK_DuAnBuocManHinh_E_ManHinh_ManHinhId",
        //                 column: x => x.ManHinhId,
        //                 principalTable: "E_ManHinh",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Cascade);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "DuAnBuocPhongBanPhoiHop",
        //         columns: table => new
        //         {
        //             DuAnBuocId = table.Column<int>(type: "int", nullable: false),
        //             PhongBanId = table.Column<long>(type: "bigint", nullable: false)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_DuAnBuocPhongBanPhoiHop", x => new { x.DuAnBuocId, x.PhongBanId });
        //             table.ForeignKey(
        //                 name: "FK_DuAnBuocPhongBanPhoiHop_DuAnBuoc_DuAnBuocId",
        //                 column: x => x.DuAnBuocId,
        //                 principalTable: "DuAnBuoc",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Cascade);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "VanBanQuyetDinh",
        //         columns: table => new
        //         {
        //             Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
        //             DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
        //             BuocId = table.Column<int>(type: "int", nullable: true),
        //             So = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             Ngay = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             TrichYeu = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             NguoiKy = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             NgayKy = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             Loai = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_VanBanQuyetDinh", x => x.Id);
        //             table.ForeignKey(
        //                 name: "FK_VanBanQuyetDinh_DuAnBuoc_BuocId",
        //                 column: x => x.BuocId,
        //                 principalTable: "DuAnBuoc",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Restrict);
        //             table.ForeignKey(
        //                 name: "FK_VanBanQuyetDinh_DuAn_DuAnId",
        //                 column: x => x.DuAnId,
        //                 principalTable: "DuAn",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Restrict);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "ThongTinDieuChinhChiPhi",
        //         columns: table => new
        //         {
        //             Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
        //             QuyetDinhDieuChinhId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
        //             TongMucDauTu = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
        //             ChiPhiXayLap = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
        //             ChiPhiThietBi = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
        //             ChiPhiKhac = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
        //             ChiPhiDuPhong = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_ThongTinDieuChinhChiPhi", x => x.Id);
        //             table.ForeignKey(
        //                 name: "FK_ThongTinDieuChinhChiPhi_QuyetDinhDieuChinh_QuyetDinhDieuChinhId",
        //                 column: x => x.QuyetDinhDieuChinhId,
        //                 principalTable: "QuyetDinhDieuChinh",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Cascade);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "KeHoachLuaChonNhaThau",
        //         columns: table => new
        //         {
        //             Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
        //             Ten = table.Column<string>(type: "nvarchar(max)", nullable: true)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_KeHoachLuaChonNhaThau", x => x.Id);
        //             table.ForeignKey(
        //                 name: "FK_KeHoachLuaChonNhaThau_VanBanQuyetDinh_Id",
        //                 column: x => x.Id,
        //                 principalTable: "VanBanQuyetDinh",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Cascade);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "PheDuyetDuToan",
        //         columns: table => new
        //         {
        //             Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
        //             ChucVuId = table.Column<int>(type: "int", nullable: true),
        //             GiaTriDuThau = table.Column<long>(type: "bigint", precision: 18, scale: 2, nullable: true),
        //             TrangThaiId = table.Column<int>(type: "int", nullable: true),
        //             NguoiXuLyId = table.Column<long>(type: "bigint", nullable: true)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_PheDuyetDuToan", x => x.Id);
        //             table.ForeignKey(
        //                 name: "FK_PheDuyetDuToan_DmChucVu_ChucVuId",
        //                 column: x => x.ChucVuId,
        //                 principalTable: "DmChucVu",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Restrict);
        //             table.ForeignKey(
        //                 name: "FK_PheDuyetDuToan_DmTrangThaiPheDuyet_TrangThaiId",
        //                 column: x => x.TrangThaiId,
        //                 principalTable: "DmTrangThaiPheDuyet",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Restrict);
        //             table.ForeignKey(
        //                 name: "FK_PheDuyetDuToan_VanBanQuyetDinh_Id",
        //                 column: x => x.Id,
        //                 principalTable: "VanBanQuyetDinh",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Cascade);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "QuyetDinhDuyetDuAn",
        //         columns: table => new
        //         {
        //             Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
        //             CoQuanQuyetDinhDauTu = table.Column<string>(type: "nvarchar(max)", nullable: true)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_QuyetDinhDuyetDuAn", x => x.Id);
        //             table.ForeignKey(
        //                 name: "FK_QuyetDinhDuyetDuAn_VanBanQuyetDinh_Id",
        //                 column: x => x.Id,
        //                 principalTable: "VanBanQuyetDinh",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Cascade);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "QuyetDinhDuyetQuyetToan",
        //         columns: table => new
        //         {
        //             Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
        //             CoQuanQuyetDinh = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             GiaTri = table.Column<long>(type: "bigint", nullable: true)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_QuyetDinhDuyetQuyetToan", x => x.Id);
        //             table.ForeignKey(
        //                 name: "FK_QuyetDinhDuyetQuyetToan_VanBanQuyetDinh_Id",
        //                 column: x => x.Id,
        //                 principalTable: "VanBanQuyetDinh",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Cascade);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "QuyetDinhLapBanQLDA",
        //         columns: table => new
        //         {
        //             Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()")
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_QuyetDinhLapBanQLDA", x => x.Id);
        //             table.ForeignKey(
        //                 name: "FK_QuyetDinhLapBanQLDA_VanBanQuyetDinh_Id",
        //                 column: x => x.Id,
        //                 principalTable: "VanBanQuyetDinh",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Cascade);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "QuyetDinhLapBenMoiThau",
        //         columns: table => new
        //         {
        //             Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
        //             NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: true)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_QuyetDinhLapBenMoiThau", x => x.Id);
        //             table.ForeignKey(
        //                 name: "FK_QuyetDinhLapBenMoiThau_VanBanQuyetDinh_Id",
        //                 column: x => x.Id,
        //                 principalTable: "VanBanQuyetDinh",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Cascade);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "QuyetDinhLapHoiDongThamDinh",
        //         columns: table => new
        //         {
        //             Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
        //             NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: true)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_QuyetDinhLapHoiDongThamDinh", x => x.Id);
        //             table.ForeignKey(
        //                 name: "FK_QuyetDinhLapHoiDongThamDinh_VanBanQuyetDinh_Id",
        //                 column: x => x.Id,
        //                 principalTable: "VanBanQuyetDinh",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Cascade);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "VanBanChuTruong",
        //         columns: table => new
        //         {
        //             Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
        //             LoaiVanBanId = table.Column<int>(type: "int", nullable: true),
        //             ChucVuId = table.Column<int>(type: "int", nullable: true)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_VanBanChuTruong", x => x.Id);
        //             table.ForeignKey(
        //                 name: "FK_VanBanChuTruong_DmChucVu_ChucVuId",
        //                 column: x => x.ChucVuId,
        //                 principalTable: "DmChucVu",
        //                 principalColumn: "Id");
        //             table.ForeignKey(
        //                 name: "FK_VanBanChuTruong_DmLoaiVanBan_LoaiVanBanId",
        //                 column: x => x.LoaiVanBanId,
        //                 principalTable: "DmLoaiVanBan",
        //                 principalColumn: "Id");
        //             table.ForeignKey(
        //                 name: "FK_VanBanChuTruong_VanBanQuyetDinh_Id",
        //                 column: x => x.Id,
        //                 principalTable: "VanBanQuyetDinh",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Cascade);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "VanBanPhapLy",
        //         columns: table => new
        //         {
        //             Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
        //             LoaiVanBanId = table.Column<int>(type: "int", nullable: true),
        //             ChucVuId = table.Column<int>(type: "int", nullable: true),
        //             ChuDauTuId = table.Column<int>(type: "int", nullable: true)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_VanBanPhapLy", x => x.Id);
        //             table.ForeignKey(
        //                 name: "FK_VanBanPhapLy_DmChuDauTu_ChuDauTuId",
        //                 column: x => x.ChuDauTuId,
        //                 principalTable: "DmChuDauTu",
        //                 principalColumn: "Id");
        //             table.ForeignKey(
        //                 name: "FK_VanBanPhapLy_DmChucVu_ChucVuId",
        //                 column: x => x.ChucVuId,
        //                 principalTable: "DmChucVu",
        //                 principalColumn: "Id");
        //             table.ForeignKey(
        //                 name: "FK_VanBanPhapLy_DmLoaiVanBan_LoaiVanBanId",
        //                 column: x => x.LoaiVanBanId,
        //                 principalTable: "DmLoaiVanBan",
        //                 principalColumn: "Id");
        //             table.ForeignKey(
        //                 name: "FK_VanBanPhapLy_VanBanQuyetDinh_Id",
        //                 column: x => x.Id,
        //                 principalTable: "VanBanQuyetDinh",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Cascade);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "GoiThau",
        //         columns: table => new
        //         {
        //             Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
        //             DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
        //             BuocId = table.Column<int>(type: "int", nullable: true),
        //             KeHoachLuaChonNhaThauId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
        //             Ten = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             GiaTri = table.Column<long>(type: "bigint", nullable: true),
        //             LoaiHopDongId = table.Column<int>(type: "int", nullable: true),
        //             HinhThucLuaChonNhaThauId = table.Column<int>(type: "int", nullable: true),
        //             PhuongThucLuaChonNhaThauId = table.Column<int>(type: "int", nullable: true),
        //             ThoiGianLuaNhaThau = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             ThoiGianHopDong = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             NguonVonId = table.Column<int>(type: "int", nullable: true),
        //             TomTatCongViecChinhGoiThau = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             ThoiGianBatDauToChucLuaChonNhaThau = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             ThoiGianThucHienGoiThau = table.Column<int>(type: "int", nullable: true),
        //             TuyChonMuaThem = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             GiamSatHoatDongDauThau = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             DaDuyet = table.Column<bool>(type: "bit", nullable: false),
        //             DuAnBuocId = table.Column<int>(type: "int", nullable: true),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_GoiThau", x => x.Id);
        //             table.ForeignKey(
        //                 name: "FK_GoiThau_DmHinhThucLuaChonNhaThau_HinhThucLuaChonNhaThauId",
        //                 column: x => x.HinhThucLuaChonNhaThauId,
        //                 principalTable: "DmHinhThucLuaChonNhaThau",
        //                 principalColumn: "Id");
        //             table.ForeignKey(
        //                 name: "FK_GoiThau_DmLoaiHopDong_LoaiHopDongId",
        //                 column: x => x.LoaiHopDongId,
        //                 principalTable: "DmLoaiHopDong",
        //                 principalColumn: "Id");
        //             table.ForeignKey(
        //                 name: "FK_GoiThau_DmNguonVon_NguonVonId",
        //                 column: x => x.NguonVonId,
        //                 principalTable: "DmNguonVon",
        //                 principalColumn: "Id");
        //             table.ForeignKey(
        //                 name: "FK_GoiThau_DmPhuongThucLuaChonNhaThau_PhuongThucLuaChonNhaThauId",
        //                 column: x => x.PhuongThucLuaChonNhaThauId,
        //                 principalTable: "DmPhuongThucLuaChonNhaThau",
        //                 principalColumn: "Id");
        //             table.ForeignKey(
        //                 name: "FK_GoiThau_DuAnBuoc_DuAnBuocId",
        //                 column: x => x.DuAnBuocId,
        //                 principalTable: "DuAnBuoc",
        //                 principalColumn: "Id");
        //             table.ForeignKey(
        //                 name: "FK_GoiThau_DuAn_DuAnId",
        //                 column: x => x.DuAnId,
        //                 principalTable: "DuAn",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Restrict);
        //             table.ForeignKey(
        //                 name: "FK_GoiThau_KeHoachLuaChonNhaThau_KeHoachLuaChonNhaThauId",
        //                 column: x => x.KeHoachLuaChonNhaThauId,
        //                 principalTable: "KeHoachLuaChonNhaThau",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Restrict);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "QuyetDinhDuyetKHLCNT",
        //         columns: table => new
        //         {
        //             Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
        //             KeHoachLuaChonNhaThauId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
        //             CoQuanQuyetDinh = table.Column<string>(type: "nvarchar(max)", nullable: true)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_QuyetDinhDuyetKHLCNT", x => x.Id);
        //             table.ForeignKey(
        //                 name: "FK_QuyetDinhDuyetKHLCNT_KeHoachLuaChonNhaThau_KeHoachLuaChonNhaThauId",
        //                 column: x => x.KeHoachLuaChonNhaThauId,
        //                 principalTable: "KeHoachLuaChonNhaThau",
        //                 principalColumn: "Id");
        //             table.ForeignKey(
        //                 name: "FK_QuyetDinhDuyetKHLCNT_VanBanQuyetDinh_Id",
        //                 column: x => x.Id,
        //                 principalTable: "VanBanQuyetDinh",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Cascade);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "QuyetDinhDuyetDuAnNguonVon",
        //         columns: table => new
        //         {
        //             Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
        //             QuyetDinhDuyetDuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
        //             NguonVonId = table.Column<int>(type: "int", nullable: false),
        //             GiaTri = table.Column<long>(type: "bigint", nullable: true),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_QuyetDinhDuyetDuAnNguonVon", x => x.Id);
        //             table.ForeignKey(
        //                 name: "FK_QuyetDinhDuyetDuAnNguonVon_QuyetDinhDuyetDuAn_QuyetDinhDuyetDuAnId",
        //                 column: x => x.QuyetDinhDuyetDuAnId,
        //                 principalTable: "QuyetDinhDuyetDuAn",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Cascade);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "ThanhVienBanQLDA",
        //         columns: table => new
        //         {
        //             Id = table.Column<int>(type: "int", nullable: false)
        //                 .Annotation("SqlServer:Identity", "1, 1"),
        //             QuyetDinhId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
        //             Ten = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             ChucVu = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             VaiTro = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_ThanhVienBanQLDA", x => x.Id);
        //             table.ForeignKey(
        //                 name: "FK_ThanhVienBanQLDA_QuyetDinhLapBanQLDA_QuyetDinhId",
        //                 column: x => x.QuyetDinhId,
        //                 principalTable: "QuyetDinhLapBanQLDA",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Cascade);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "HopDong",
        //         columns: table => new
        //         {
        //             Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
        //             DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
        //             BuocId = table.Column<int>(type: "int", nullable: true),
        //             GoiThauId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
        //             Ten = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             SoHopDong = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             NgayKy = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             GiaTri = table.Column<long>(type: "bigint", nullable: true),
        //             NgayHieuLuc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             NgayDuKienKetThucHopDong = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             NgayDuKienKetThucGoiThau = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             LoaiHopDongId = table.Column<int>(type: "int", nullable: true),
        //             DonViThucHienId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
        //             IsBienBan = table.Column<bool>(type: "bit", nullable: false),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_HopDong", x => x.Id);
        //             table.ForeignKey(
        //                 name: "FK_HopDong_DmLoaiHopDong_LoaiHopDongId",
        //                 column: x => x.LoaiHopDongId,
        //                 principalTable: "DmLoaiHopDong",
        //                 principalColumn: "Id");
        //             table.ForeignKey(
        //                 name: "FK_HopDong_DmNhaThau_DonViThucHienId",
        //                 column: x => x.DonViThucHienId,
        //                 principalTable: "DmNhaThau",
        //                 principalColumn: "Id");
        //             table.ForeignKey(
        //                 name: "FK_HopDong_DuAnBuoc_BuocId",
        //                 column: x => x.BuocId,
        //                 principalTable: "DuAnBuoc",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Restrict);
        //             table.ForeignKey(
        //                 name: "FK_HopDong_DuAn_DuAnId",
        //                 column: x => x.DuAnId,
        //                 principalTable: "DuAn",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Restrict);
        //             table.ForeignKey(
        //                 name: "FK_HopDong_GoiThau_GoiThauId",
        //                 column: x => x.GoiThauId,
        //                 principalTable: "GoiThau",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Restrict);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "HoSoMoiThauDienTu",
        //         columns: table => new
        //         {
        //             Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
        //             DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
        //             BuocId = table.Column<int>(type: "int", nullable: true),
        //             HinhThucLuaChonNhaThauId = table.Column<int>(type: "int", nullable: true),
        //             GoiThauId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
        //             GiaTri = table.Column<long>(type: "bigint", nullable: true),
        //             ThoiGianThucHien = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
        //             TrangThaiDangTai = table.Column<bool>(type: "bit", nullable: false),
        //             TrangThaiId = table.Column<int>(type: "int", nullable: true),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_HoSoMoiThauDienTu", x => x.Id);
        //             table.ForeignKey(
        //                 name: "FK_HoSoMoiThauDienTu_DmHinhThucLuaChonNhaThau_HinhThucLuaChonNhaThauId",
        //                 column: x => x.HinhThucLuaChonNhaThauId,
        //                 principalTable: "DmHinhThucLuaChonNhaThau",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.SetNull);
        //             table.ForeignKey(
        //                 name: "FK_HoSoMoiThauDienTu_DmTrangThaiPheDuyet_TrangThaiId",
        //                 column: x => x.TrangThaiId,
        //                 principalTable: "DmTrangThaiPheDuyet",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.SetNull);
        //             table.ForeignKey(
        //                 name: "FK_HoSoMoiThauDienTu_DuAnBuoc_BuocId",
        //                 column: x => x.BuocId,
        //                 principalTable: "DuAnBuoc",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.SetNull);
        //             table.ForeignKey(
        //                 name: "FK_HoSoMoiThauDienTu_DuAn_DuAnId",
        //                 column: x => x.DuAnId,
        //                 principalTable: "DuAn",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.SetNull);
        //             table.ForeignKey(
        //                 name: "FK_HoSoMoiThauDienTu_GoiThau_GoiThauId",
        //                 column: x => x.GoiThauId,
        //                 principalTable: "GoiThau",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.SetNull);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "KetQuaTrungThau",
        //         columns: table => new
        //         {
        //             Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
        //             DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
        //             BuocId = table.Column<int>(type: "int", nullable: true),
        //             GoiThauId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
        //             GiaTriTrungThau = table.Column<long>(type: "bigint", nullable: false),
        //             DonViTrungThauId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
        //             SoNgayTrienKhai = table.Column<int>(type: "int", nullable: true),
        //             TrichYeu = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             LoaiGoiThauId = table.Column<int>(type: "int", nullable: true),
        //             NgayEHSMT = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             NgayMoThau = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             SoNgayThucHienHopDong = table.Column<int>(type: "int", nullable: true),
        //             SoQuyetDinh = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             NgayQuyetDinh = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_KetQuaTrungThau", x => x.Id);
        //             table.ForeignKey(
        //                 name: "FK_KetQuaTrungThau_DmLoaiGoiThau_LoaiGoiThauId",
        //                 column: x => x.LoaiGoiThauId,
        //                 principalTable: "DmLoaiGoiThau",
        //                 principalColumn: "Id");
        //             table.ForeignKey(
        //                 name: "FK_KetQuaTrungThau_DmNhaThau_DonViTrungThauId",
        //                 column: x => x.DonViTrungThauId,
        //                 principalTable: "DmNhaThau",
        //                 principalColumn: "Id");
        //             table.ForeignKey(
        //                 name: "FK_KetQuaTrungThau_DuAnBuoc_BuocId",
        //                 column: x => x.BuocId,
        //                 principalTable: "DuAnBuoc",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Restrict);
        //             table.ForeignKey(
        //                 name: "FK_KetQuaTrungThau_DuAn_DuAnId",
        //                 column: x => x.DuAnId,
        //                 principalTable: "DuAn",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Restrict);
        //             table.ForeignKey(
        //                 name: "FK_KetQuaTrungThau_GoiThau_GoiThauId",
        //                 column: x => x.GoiThauId,
        //                 principalTable: "GoiThau",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Cascade);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "QuyetDinhDuyetDuAnHangMuc",
        //         columns: table => new
        //         {
        //             Id = table.Column<int>(type: "int", nullable: false)
        //                 .Annotation("SqlServer:Identity", "1, 1"),
        //             QuyetDinhDuyetDuAnNguonVonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
        //             TenHangMuc = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             QuyMoHangMuc = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             TongMucDauTu = table.Column<long>(type: "bigint", nullable: true),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_QuyetDinhDuyetDuAnHangMuc", x => x.Id);
        //             table.ForeignKey(
        //                 name: "FK_QuyetDinhDuyetDuAnHangMuc_QuyetDinhDuyetDuAnNguonVon_QuyetDinhDuyetDuAnNguonVonId",
        //                 column: x => x.QuyetDinhDuyetDuAnNguonVonId,
        //                 principalTable: "QuyetDinhDuyetDuAnNguonVon",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Cascade);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "NghiemThu",
        //         columns: table => new
        //         {
        //             Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
        //             DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
        //             BuocId = table.Column<int>(type: "int", nullable: true),
        //             HopDongId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
        //             SoBienBan = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             Dot = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             Ngay = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             GiaTri = table.Column<long>(type: "bigint", nullable: false),
        //             DuAnBuocId = table.Column<int>(type: "int", nullable: true),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_NghiemThu", x => x.Id);
        //             table.ForeignKey(
        //                 name: "FK_NghiemThu_DuAnBuoc_DuAnBuocId",
        //                 column: x => x.DuAnBuocId,
        //                 principalTable: "DuAnBuoc",
        //                 principalColumn: "Id");
        //             table.ForeignKey(
        //                 name: "FK_NghiemThu_DuAn_DuAnId",
        //                 column: x => x.DuAnId,
        //                 principalTable: "DuAn",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Restrict);
        //             table.ForeignKey(
        //                 name: "FK_NghiemThu_HopDong_HopDongId",
        //                 column: x => x.HopDongId,
        //                 principalTable: "HopDong",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Restrict);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "PhuLucHopDong",
        //         columns: table => new
        //         {
        //             Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
        //             DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
        //             BuocId = table.Column<int>(type: "int", nullable: true),
        //             Ten = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             SoPhuLucHopDong = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             Ngay = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             HopDongId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
        //             GiaTri = table.Column<long>(type: "bigint", nullable: true),
        //             NgayDuKienKetThuc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_PhuLucHopDong", x => x.Id);
        //             table.ForeignKey(
        //                 name: "FK_PhuLucHopDong_DuAnBuoc_BuocId",
        //                 column: x => x.BuocId,
        //                 principalTable: "DuAnBuoc",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Restrict);
        //             table.ForeignKey(
        //                 name: "FK_PhuLucHopDong_DuAn_DuAnId",
        //                 column: x => x.DuAnId,
        //                 principalTable: "DuAn",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Restrict);
        //             table.ForeignKey(
        //                 name: "FK_PhuLucHopDong_HopDong_HopDongId",
        //                 column: x => x.HopDongId,
        //                 principalTable: "HopDong",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Restrict);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "TamUng",
        //         columns: table => new
        //         {
        //             Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
        //             DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
        //             BuocId = table.Column<int>(type: "int", nullable: true),
        //             HopDongId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
        //             SoPhieuChi = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             GiaTri = table.Column<long>(type: "bigint", nullable: true),
        //             NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             NgayTamUng = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             SoBaoLanh = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
        //             NgayBaoLanh = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             NgayKetThucBaoLanh = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_TamUng", x => x.Id);
        //             table.ForeignKey(
        //                 name: "FK_TamUng_DuAnBuoc_BuocId",
        //                 column: x => x.BuocId,
        //                 principalTable: "DuAnBuoc",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Restrict);
        //             table.ForeignKey(
        //                 name: "FK_TamUng_DuAn_DuAnId",
        //                 column: x => x.DuAnId,
        //                 principalTable: "DuAn",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Restrict);
        //             table.ForeignKey(
        //                 name: "FK_TamUng_HopDong_HopDongId",
        //                 column: x => x.HopDongId,
        //                 principalTable: "HopDong",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Restrict);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "ThanhToan",
        //         columns: table => new
        //         {
        //             Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
        //             DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
        //             BuocId = table.Column<int>(type: "int", nullable: true),
        //             NghiemThuId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
        //             SoHoaDon = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             NgayHoaDon = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             GiaTri = table.Column<long>(type: "bigint", nullable: true),
        //             NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: true),
        //             CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
        //             UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
        //             IsDeleted = table.Column<bool>(type: "bit", nullable: false),
        //             Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_ThanhToan", x => x.Id);
        //             table.ForeignKey(
        //                 name: "FK_ThanhToan_DuAnBuoc_BuocId",
        //                 column: x => x.BuocId,
        //                 principalTable: "DuAnBuoc",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Restrict);
        //             table.ForeignKey(
        //                 name: "FK_ThanhToan_DuAn_DuAnId",
        //                 column: x => x.DuAnId,
        //                 principalTable: "DuAn",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Restrict);
        //             table.ForeignKey(
        //                 name: "FK_ThanhToan_NghiemThu_NghiemThuId",
        //                 column: x => x.NghiemThuId,
        //                 principalTable: "NghiemThu",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Restrict);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "NghiemThuPhuLucHopDong",
        //         columns: table => new
        //         {
        //             NghiemThuId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
        //             PhuLucHopDongId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_NghiemThuPhuLucHopDong", x => new { x.NghiemThuId, x.PhuLucHopDongId });
        //             table.ForeignKey(
        //                 name: "FK_NghiemThuPhuLucHopDong_NghiemThu_NghiemThuId",
        //                 column: x => x.NghiemThuId,
        //                 principalTable: "NghiemThu",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Cascade);
        //             table.ForeignKey(
        //                 name: "FK_NghiemThuPhuLucHopDong_PhuLucHopDong_PhuLucHopDongId",
        //                 column: x => x.PhuLucHopDongId,
        //                 principalTable: "PhuLucHopDong",
        //                 principalColumn: "Id",
        //                 onDelete: ReferentialAction.Restrict);
        //         });

        //     migrationBuilder.InsertData(
        //         table: "DanhMucLoaiDieuChinh",
        //         columns: new[] { "Id", "CreatedAt", "CreatedBy", "IsDeleted", "Ma", "MoTa", "Stt", "Ten", "UpdatedAt", "UpdatedBy", "Used" },
        //         values: new object[,]
        //         {
        //             { 1, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "MDQ", null, 1, "Điều chỉnh mục tiêu, quy mô đầu tư", null, "", true },
        //             { 2, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "TMDT", null, 2, "Điều chỉnh tổng mức đầu tư", null, "", true },
        //             { 3, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "TDO", null, 3, "Điều chỉnh tiến độ đầu tư", null, "", true },
        //             { 4, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "CDT", null, 4, "Chuyển đổi chủ đầu tư", null, "", true },
        //             { 5, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "TDD", null, 5, "Tạm dừng dự án", null, "", true },
        //             { 6, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "NVU", null, 6, "Điều chỉnh nguồn vốn dự án", null, "", true },
        //             { 7, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "CTMDT", null, 7, "Điều chỉnh cơ cấu tổng mức đầu tư", null, "", true }
        //         });

        //     migrationBuilder.InsertData(
        //         table: "DmLoaiDuAnTheoNam",
        //         columns: new[] { "Id", "CreatedBy", "IsDeleted", "Ma", "MoTa", "Stt", "Ten", "UpdatedAt", "UpdatedBy", "Used" },
        //         values: new object[,]
        //         {
        //             { 1, "", false, "CBDT", null, null, "Chuẩn bị đầu tư", null, "", false },
        //             { 2, "", false, "CT", null, null, "Chuyển tiếp", null, "", false },
        //             { 3, "", false, "KCM", null, null, "Khởi công mới", null, "", false },
        //             { 4, "", false, "KLTD", null, null, "Khối lượng tồn đọng", null, "", false }
        //         });

        //     migrationBuilder.InsertData(
        //         table: "DmQuyen",
        //         columns: new[] { "Id", "CreatedAt", "CreatedBy", "IsDeleted", "Ma", "MoTa", "NhomQuyen", "Stt", "Ten", "UpdatedAt", "UpdatedBy", "Used" },
        //         values: new object[,]
        //         {
        //             { 1, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "DuAn.XemTatCa", null, "DuAn", 1, "Xem tất cả dự án", null, "", true },
        //             { 2, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "DuAn.XemTheoPhong", null, "DuAn", 2, "Xem theo phòng dự án", null, "", true },
        //             { 3, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "DuAn.Tao", null, "DuAn", 3, "Tạo dự án", null, "", true },
        //             { 4, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "DuAn.Sua", null, "DuAn", 4, "Sửa dự án", null, "", true },
        //             { 5, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "DuAn.Xoa", null, "DuAn", 5, "Xóa dự án", null, "", true },
        //             { 6, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "DuAn.PheDuyet", null, "DuAn", 6, "Phê duyệt dự án", null, "", true },
        //             { 7, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "GoiThau.XemTatCa", null, "GoiThau", 1, "Xem tất cả gói thầu", null, "", true },
        //             { 8, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "GoiThau.XemTheoPhong", null, "GoiThau", 2, "Xem theo phòng gói thầu", null, "", true },
        //             { 9, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "GoiThau.Tao", null, "GoiThau", 3, "Tạo gói thầu", null, "", true },
        //             { 10, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "GoiThau.Sua", null, "GoiThau", 4, "Sửa gói thầu", null, "", true },
        //             { 11, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "GoiThau.Xoa", null, "GoiThau", 5, "Xóa gói thầu", null, "", true },
        //             { 12, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "HopDong.XemTatCa", null, "HopDong", 1, "Xem tất cả hợp đồng", null, "", true },
        //             { 13, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "HopDong.XemTheoPhong", null, "HopDong", 2, "Xem theo phòng hợp đồng", null, "", true },
        //             { 14, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "HopDong.Tao", null, "HopDong", 3, "Tạo hợp đồng", null, "", true },
        //             { 15, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "HopDong.Sua", null, "HopDong", 4, "Sửa hợp đồng", null, "", true },
        //             { 16, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "HopDong.Xoa", null, "HopDong", 5, "Xóa hợp đồng", null, "", true },
        //             { 17, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "VanBan.XemTatCa", null, "VanBan", 1, "Xem tất cả văn bản", null, "", true },
        //             { 18, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "VanBan.XemTheoPhong", null, "VanBan", 2, "Xem theo phòng văn bản", null, "", true },
        //             { 19, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "VanBan.Tao", null, "VanBan", 3, "Tạo văn bản", null, "", true },
        //             { 20, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "VanBan.Sua", null, "VanBan", 4, "Sửa văn bản", null, "", true },
        //             { 21, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "VanBan.Xoa", null, "VanBan", 5, "Xóa văn bản", null, "", true },
        //             { 22, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "ThanhToan.QuanLy", null, "ThanhToan", 1, "Quản lý thanh toán", null, "", true },
        //             { 23, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "PheDuyet.XemTatCa", null, "PheDuyet", 1, "Xem tất cả PheDuyet", null, "", true },
        //             { 24, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "PheDuyet.XemTheoPhong", null, "PheDuyet", 2, "Xem theo phòng PheDuyet", null, "", true },
        //             { 25, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "PheDuyet.Duyet", null, "PheDuyet", 3, "Duyet PheDuyet", null, "", true },
        //             { 26, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "PheDuyet.KySo", null, "PheDuyet", 4, "KySo PheDuyet", null, "", true },
        //             { 27, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "PheDuyet.ChuyenQLVB", null, "PheDuyet", 5, "ChuyenQLVB PheDuyet", null, "", true },
        //             { 28, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "PheDuyet.PhatHanh", null, "PheDuyet", 6, "PhatHanh PheDuyet", null, "", true },
        //             { 29, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "PheDuyet.TuChoi", null, "PheDuyet", 7, "TuChoi PheDuyet", null, "", true }
        //         });

        //     migrationBuilder.InsertData(
        //         table: "DmTrangThaiDuAn",
        //         columns: new[] { "Id", "CreatedBy", "IsDeleted", "Ma", "MoTa", "Stt", "Ten", "UpdatedAt", "UpdatedBy", "Used" },
        //         values: new object[,]
        //         {
        //             { 1, "", false, "DTH", null, null, "Đang thực hiện", null, "", false },
        //             { 2, "", false, "PDDT", null, null, "Đã phê duyệt đầu tư", null, "", false },
        //             { 3, "", false, "HT", null, null, "Đã hoàn thành", null, "", false },
        //             { 4, "", false, "TD", null, null, "Tạm dừng", null, "", false }
        //         });

        //     migrationBuilder.InsertData(
        //         table: "DmTrangThaiPheDuyet",
        //         columns: new[] { "Id", "CreatedAt", "CreatedBy", "IsDeleted", "Loai", "Ma", "MoTa", "Stt", "Ten", "UpdatedAt", "UpdatedBy", "Used" },
        //         values: new object[,]
        //         {
        //             { 1, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "PheDuyetDuToan", "DT", null, 1, "Dự thảo", null, "", true },
        //             { 2, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "PheDuyetDuToan", "ĐTr", null, 2, "Đã trình", null, "", true },
        //             { 3, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "PheDuyetDuToan", "ĐD", null, 3, "Đã duyệt", null, "", true },
        //             { 4, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "PheDuyetDuToan", "TL", null, 4, "Trả lại", null, "", true },
        //             { 5, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "PheDuyetDuToan", "TC", null, 5, "Từ chối", null, "", true },
        //             { 6, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "Default", "LEG", null, 0, "Migrated", null, "", false },
        //             { 7, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "HoSoDeXuatCapDoCntt", "DT", null, 1, "Dự thảo", null, "", true },
        //             { 8, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "HoSoDeXuatCapDoCntt", "ĐTr", null, 2, "Đã trình", null, "", true },
        //             { 9, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "HoSoDeXuatCapDoCntt", "ĐD", null, 3, "Đã duyệt", null, "", true },
        //             { 10, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "HoSoDeXuatCapDoCntt", "TL", null, 4, "Trả lại", null, "", true },
        //             { 11, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "HoSoDeXuatCapDoCntt", "TC", null, 5, "Từ chối", null, "", true },
        //             { 12, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "HoSoMoiThauDienTu", "DT", null, 1, "Dự thảo", null, "", true },
        //             { 13, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "HoSoMoiThauDienTu", "ĐTr", null, 2, "Đã trình", null, "", true },
        //             { 14, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "HoSoMoiThauDienTu", "ĐD", null, 3, "Đã duyệt", null, "", true },
        //             { 15, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "HoSoMoiThauDienTu", "TL", null, 4, "Trả lại", null, "", true },
        //             { 16, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "HoSoMoiThauDienTu", "TC", null, 5, "Từ chối", null, "", true },
        //             { 17, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "PhanKhaiKinhPhi", "DT", null, 1, "Dự thảo", null, "", true },
        //             { 18, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "PhanKhaiKinhPhi", "ĐTr", null, 2, "Đã trình", null, "", true },
        //             { 19, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "PhanKhaiKinhPhi", "ĐD", null, 3, "Đã duyệt", null, "", true },
        //             { 20, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "PhanKhaiKinhPhi", "TL", null, 4, "Trả lại", null, "", true },
        //             { 21, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "QuyetDinhDieuChinh", "DT", null, 1, "Dự thảo", null, "", true },
        //             { 22, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "QuyetDinhDieuChinh", "ĐTr", null, 2, "Đã trình", null, "", true },
        //             { 23, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "QuyetDinhDieuChinh", "ĐD", null, 3, "Đã duyệt", null, "", true },
        //             { 24, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "QuyetDinhDieuChinh", "TL", null, 4, "Trả lại", null, "", true },
        //             { 25, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "QuyetDinhDieuChinh", "TC", null, 5, "Từ chối", null, "", true }
        //         });

        //     migrationBuilder.InsertData(
        //         table: "CauHinhVaiTroQuyen",
        //         columns: new[] { "Id", "CreatedAt", "CreatedBy", "IsDeleted", "KichHoat", "QuyenId", "UpdatedAt", "UpdatedBy", "VaiTro" },
        //         values: new object[,]
        //         {
        //             { 1, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 1, null, "", "QLDA_TatCa" },
        //             { 2, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 2, null, "", "QLDA_TatCa" },
        //             { 3, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 3, null, "", "QLDA_TatCa" },
        //             { 4, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 4, null, "", "QLDA_TatCa" },
        //             { 5, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 5, null, "", "QLDA_TatCa" },
        //             { 6, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 6, null, "", "QLDA_TatCa" },
        //             { 7, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 7, null, "", "QLDA_TatCa" },
        //             { 8, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 8, null, "", "QLDA_TatCa" },
        //             { 9, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 9, null, "", "QLDA_TatCa" },
        //             { 10, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 10, null, "", "QLDA_TatCa" },
        //             { 11, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 11, null, "", "QLDA_TatCa" },
        //             { 12, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 12, null, "", "QLDA_TatCa" },
        //             { 13, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 13, null, "", "QLDA_TatCa" },
        //             { 14, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 14, null, "", "QLDA_TatCa" },
        //             { 15, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 15, null, "", "QLDA_TatCa" },
        //             { 16, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 16, null, "", "QLDA_TatCa" },
        //             { 17, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 17, null, "", "QLDA_TatCa" },
        //             { 18, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 18, null, "", "QLDA_TatCa" },
        //             { 19, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 19, null, "", "QLDA_TatCa" },
        //             { 20, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 20, null, "", "QLDA_TatCa" },
        //             { 21, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 21, null, "", "QLDA_TatCa" },
        //             { 22, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 22, null, "", "QLDA_TatCa" },
        //             { 23, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 23, null, "", "QLDA_TatCa" },
        //             { 24, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 24, null, "", "QLDA_TatCa" },
        //             { 25, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 25, null, "", "QLDA_TatCa" },
        //             { 26, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 26, null, "", "QLDA_TatCa" },
        //             { 27, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 27, null, "", "QLDA_TatCa" },
        //             { 28, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 28, null, "", "QLDA_TatCa" },
        //             { 29, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 29, null, "", "QLDA_TatCa" },
        //             { 30, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 1, null, "", "QLDA_QuanTri" },
        //             { 31, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 2, null, "", "QLDA_QuanTri" },
        //             { 32, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 3, null, "", "QLDA_QuanTri" },
        //             { 33, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 4, null, "", "QLDA_QuanTri" },
        //             { 34, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 5, null, "", "QLDA_QuanTri" },
        //             { 35, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 6, null, "", "QLDA_QuanTri" },
        //             { 36, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 7, null, "", "QLDA_QuanTri" },
        //             { 37, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 8, null, "", "QLDA_QuanTri" },
        //             { 38, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 9, null, "", "QLDA_QuanTri" },
        //             { 39, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 10, null, "", "QLDA_QuanTri" },
        //             { 40, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 11, null, "", "QLDA_QuanTri" },
        //             { 41, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 12, null, "", "QLDA_QuanTri" },
        //             { 42, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 13, null, "", "QLDA_QuanTri" },
        //             { 43, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 14, null, "", "QLDA_QuanTri" },
        //             { 44, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 15, null, "", "QLDA_QuanTri" },
        //             { 45, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 16, null, "", "QLDA_QuanTri" },
        //             { 46, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 17, null, "", "QLDA_QuanTri" },
        //             { 47, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 18, null, "", "QLDA_QuanTri" },
        //             { 48, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 19, null, "", "QLDA_QuanTri" },
        //             { 49, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 20, null, "", "QLDA_QuanTri" },
        //             { 50, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 21, null, "", "QLDA_QuanTri" },
        //             { 51, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 22, null, "", "QLDA_QuanTri" },
        //             { 52, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 23, null, "", "QLDA_QuanTri" },
        //             { 53, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 24, null, "", "QLDA_QuanTri" },
        //             { 54, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 25, null, "", "QLDA_QuanTri" },
        //             { 55, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 26, null, "", "QLDA_QuanTri" },
        //             { 56, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 27, null, "", "QLDA_QuanTri" },
        //             { 57, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 28, null, "", "QLDA_QuanTri" },
        //             { 58, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 29, null, "", "QLDA_QuanTri" },
        //             { 59, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 1, null, "", "QLDA_LDDV" },
        //             { 60, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 7, null, "", "QLDA_LDDV" },
        //             { 61, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 12, null, "", "QLDA_LDDV" },
        //             { 62, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 17, null, "", "QLDA_LDDV" },
        //             { 63, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 23, null, "", "QLDA_LDDV" },
        //             { 64, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 25, null, "", "QLDA_LDDV" },
        //             { 65, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 26, null, "", "QLDA_LDDV" },
        //             { 66, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 27, null, "", "QLDA_LDDV" },
        //             { 67, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 29, null, "", "QLDA_LDDV" },
        //             { 68, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 2, null, "", "QLDA_ChuyenVien" },
        //             { 69, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 8, null, "", "QLDA_ChuyenVien" },
        //             { 70, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 13, null, "", "QLDA_ChuyenVien" },
        //             { 71, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 18, null, "", "QLDA_ChuyenVien" },
        //             { 72, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 24, null, "", "QLDA_ChuyenVien" },
        //             { 73, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 3, null, "", "QLDA_ChuyenVien" },
        //             { 74, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 4, null, "", "QLDA_ChuyenVien" },
        //             { 75, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 9, null, "", "QLDA_ChuyenVien" },
        //             { 76, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 10, null, "", "QLDA_ChuyenVien" },
        //             { 77, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 14, null, "", "QLDA_ChuyenVien" },
        //             { 78, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 15, null, "", "QLDA_ChuyenVien" },
        //             { 79, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 19, null, "", "QLDA_ChuyenVien" },
        //             { 80, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 20, null, "", "QLDA_ChuyenVien" }
        //         });

        //     migrationBuilder.CreateIndex(
        //         name: "IX_AuditLog_EntityId",
        //         table: "AuditLog",
        //         column: "EntityId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_AuditLog_EntityName",
        //         table: "AuditLog",
        //         column: "EntityName");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_AuditLog_EntityName_EntityId",
        //         table: "AuditLog",
        //         columns: new[] { "EntityName", "EntityId" });

        //     migrationBuilder.CreateIndex(
        //         name: "IX_AuditLog_Index",
        //         table: "AuditLog",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_BanGiaoHoSo_BuocId",
        //         table: "BanGiaoHoSo",
        //         column: "BuocId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_BanGiaoHoSo_CreatedBy_TrangThai",
        //         table: "BanGiaoHoSo",
        //         columns: new[] { "CreatedBy", "TrangThai" });

        //     migrationBuilder.CreateIndex(
        //         name: "IX_BanGiaoHoSo_DuAnId",
        //         table: "BanGiaoHoSo",
        //         column: "DuAnId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_BanGiaoHoSo_Index",
        //         table: "BanGiaoHoSo",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_BaoCao_BuocId",
        //         table: "BaoCao",
        //         column: "BuocId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_BaoCao_DuAnId",
        //         table: "BaoCao",
        //         column: "DuAnId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_BaoCao_Index",
        //         table: "BaoCao",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_BaoCaoBanGiaoSanPham_DonViBanGiaoId",
        //         table: "BaoCaoBanGiaoSanPham",
        //         column: "DonViBanGiaoId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_BaoCaoKhoKhanVuongMac_MucDoKhoKhanId",
        //         table: "BaoCaoKhoKhanVuongMac",
        //         column: "MucDoKhoKhanId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_BaoCaoKhoKhanVuongMac_TinhTrangId",
        //         table: "BaoCaoKhoKhanVuongMac",
        //         column: "TinhTrangId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_CauHinhVaiTroQuyen_Index",
        //         table: "CauHinhVaiTroQuyen",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_CauHinhVaiTroQuyen_QuyenId",
        //         table: "CauHinhVaiTroQuyen",
        //         column: "QuyenId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_CauHinhVaiTroQuyen_VaiTro_QuyenId",
        //         table: "CauHinhVaiTroQuyen",
        //         columns: new[] { "VaiTro", "QuyenId" },
        //         unique: true);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DangTaiKeHoachLcntLenMang_BuocId",
        //         table: "DangTaiKeHoachLcntLenMang",
        //         column: "BuocId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DangTaiKeHoachLcntLenMang_Index",
        //         table: "DangTaiKeHoachLcntLenMang",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DangTaiKeHoachLcntLenMang_KeHoachLuaChonNhaThauId",
        //         table: "DangTaiKeHoachLcntLenMang",
        //         column: "KeHoachLuaChonNhaThauId",
        //         unique: true,
        //         filter: "[IsDeleted] = 0");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DanhMucLoaiDieuChinh_Index",
        //         table: "DanhMucLoaiDieuChinh",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DanhMucLoaiDieuChinh_Ma",
        //         table: "DanhMucLoaiDieuChinh",
        //         column: "Ma",
        //         unique: true,
        //         filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DeXuatChuTruongMoi_DuAnId",
        //         table: "DeXuatChuTruongMoi",
        //         column: "DuAnId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DeXuatChuTruongMoi_HinhThucDauTuId",
        //         table: "DeXuatChuTruongMoi",
        //         column: "HinhThucDauTuId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DeXuatChuTruongMoi_Index",
        //         table: "DeXuatChuTruongMoi",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DeXuatChuTruongMoi_TrangThaiId",
        //         table: "DeXuatChuTruongMoi",
        //         column: "TrangThaiId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DeXuatChuyenTiep_DuAnId",
        //         table: "DeXuatChuyenTiep",
        //         column: "DuAnId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DeXuatChuyenTiep_Index",
        //         table: "DeXuatChuyenTiep",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DeXuatChuyenTiep_TrangThaiId",
        //         table: "DeXuatChuyenTiep",
        //         column: "TrangThaiId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DeXuatNhuCauKinhPhi_DuAnId",
        //         table: "DeXuatNhuCauKinhPhi",
        //         column: "DuAnId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DeXuatNhuCauKinhPhi_Index",
        //         table: "DeXuatNhuCauKinhPhi",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DeXuatNhuCauKinhPhi_TrangThaiId",
        //         table: "DeXuatNhuCauKinhPhi",
        //         column: "TrangThaiId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmBuoc_GiaiDoanId",
        //         table: "DmBuoc",
        //         column: "GiaiDoanId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmBuoc_Index",
        //         table: "DmBuoc",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmBuoc_Ma",
        //         table: "DmBuoc",
        //         column: "Ma",
        //         unique: true,
        //         filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmBuoc_QuyTrinhId",
        //         table: "DmBuoc",
        //         column: "QuyTrinhId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmBuocManHinh_ManHinhId",
        //         table: "DmBuocManHinh",
        //         column: "ManHinhId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmBuocTrangThaiTienDo_BuocId",
        //         table: "DmBuocTrangThaiTienDo",
        //         column: "BuocId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmBuocTrangThaiTienDo_Index",
        //         table: "DmBuocTrangThaiTienDo",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmBuocTrangThaiTienDo_Ma",
        //         table: "DmBuocTrangThaiTienDo",
        //         column: "Ma",
        //         unique: true,
        //         filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmBuocTrangThaiTienDo_TrangThaiId",
        //         table: "DmBuocTrangThaiTienDo",
        //         column: "TrangThaiId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmCapDoCntt_Index",
        //         table: "DmCapDoCntt",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmCapDoCntt_Ma",
        //         table: "DmCapDoCntt",
        //         column: "Ma",
        //         unique: true,
        //         filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmChucVu_Index",
        //         table: "DmChucVu",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmChucVu_Ma",
        //         table: "DmChucVu",
        //         column: "Ma",
        //         unique: true,
        //         filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmChuDauTu_Index",
        //         table: "DmChuDauTu",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmChuDauTu_Ma",
        //         table: "DmChuDauTu",
        //         column: "Ma",
        //         unique: true,
        //         filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmGiaiDoan_Index",
        //         table: "DmGiaiDoan",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmGiaiDoan_Ma",
        //         table: "DmGiaiDoan",
        //         column: "Ma",
        //         unique: true,
        //         filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmHinhThucDauTu_Index",
        //         table: "DmHinhThucDauTu",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmHinhThucDauTu_Ma",
        //         table: "DmHinhThucDauTu",
        //         column: "Ma",
        //         unique: true,
        //         filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmHinhThucLuaChonNhaThau_Index",
        //         table: "DmHinhThucLuaChonNhaThau",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmHinhThucLuaChonNhaThau_Ma",
        //         table: "DmHinhThucLuaChonNhaThau",
        //         column: "Ma",
        //         unique: true,
        //         filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmHinhThucQuanLy_Index",
        //         table: "DmHinhThucQuanLy",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmHinhThucQuanLy_Ma",
        //         table: "DmHinhThucQuanLy",
        //         column: "Ma",
        //         unique: true,
        //         filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmLinhVuc_Index",
        //         table: "DmLinhVuc",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmLinhVuc_Ma",
        //         table: "DmLinhVuc",
        //         column: "Ma",
        //         unique: true,
        //         filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmLoaiDuAn_Index",
        //         table: "DmLoaiDuAn",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmLoaiDuAn_Ma",
        //         table: "DmLoaiDuAn",
        //         column: "Ma",
        //         unique: true,
        //         filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmLoaiDuAnTheoNam_Index",
        //         table: "DmLoaiDuAnTheoNam",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmLoaiDuAnTheoNam_Ma",
        //         table: "DmLoaiDuAnTheoNam",
        //         column: "Ma",
        //         unique: true,
        //         filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmLoaiGoiThau_Index",
        //         table: "DmLoaiGoiThau",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmLoaiGoiThau_Ma",
        //         table: "DmLoaiGoiThau",
        //         column: "Ma",
        //         unique: true,
        //         filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmLoaiHopDong_Index",
        //         table: "DmLoaiHopDong",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmLoaiHopDong_Ma",
        //         table: "DmLoaiHopDong",
        //         column: "Ma",
        //         unique: true,
        //         filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmLoaiVanBan_Index",
        //         table: "DmLoaiVanBan",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmLoaiVanBan_Ma",
        //         table: "DmLoaiVanBan",
        //         column: "Ma",
        //         unique: true,
        //         filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmMucDoKhoKhan_Index",
        //         table: "DmMucDoKhoKhan",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmMucDoKhoKhan_Ma",
        //         table: "DmMucDoKhoKhan",
        //         column: "Ma",
        //         unique: true,
        //         filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmNguonVon_Index",
        //         table: "DmNguonVon",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmNguonVon_Ma",
        //         table: "DmNguonVon",
        //         column: "Ma",
        //         unique: true,
        //         filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmNhaThau_Index",
        //         table: "DmNhaThau",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmNhaThau_Ma",
        //         table: "DmNhaThau",
        //         column: "Ma",
        //         unique: true,
        //         filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmNhomDuAn_Index",
        //         table: "DmNhomDuAn",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmNhomDuAn_Ma",
        //         table: "DmNhomDuAn",
        //         column: "Ma",
        //         unique: true,
        //         filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmPhuongThucKySo_Index",
        //         table: "DmPhuongThucKySo",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmPhuongThucKySo_Ma",
        //         table: "DmPhuongThucKySo",
        //         column: "Ma",
        //         unique: true,
        //         filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmPhuongThucLuaChonNhaThau_Index",
        //         table: "DmPhuongThucLuaChonNhaThau",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmPhuongThucLuaChonNhaThau_Ma",
        //         table: "DmPhuongThucLuaChonNhaThau",
        //         column: "Ma",
        //         unique: true,
        //         filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmQuyen_Index",
        //         table: "DmQuyen",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmQuyen_Ma",
        //         table: "DmQuyen",
        //         column: "Ma",
        //         unique: true,
        //         filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmQuyen_Ma_NhomQuyen",
        //         table: "DmQuyen",
        //         columns: new[] { "Ma", "NhomQuyen" },
        //         unique: true,
        //         filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmQuyTrinh_Index",
        //         table: "DmQuyTrinh",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmQuyTrinh_Ma",
        //         table: "DmQuyTrinh",
        //         column: "Ma",
        //         unique: true,
        //         filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmQuyTrinh_MacDinh",
        //         table: "DmQuyTrinh",
        //         column: "MacDinh",
        //         unique: true,
        //         filter: "[MacDinh] = 1 AND [IsDeleted] = 0");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmTinhTrangKhoKhan_Index",
        //         table: "DmTinhTrangKhoKhan",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmTinhTrangKhoKhan_Ma",
        //         table: "DmTinhTrangKhoKhan",
        //         column: "Ma",
        //         unique: true,
        //         filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmTinhTrangThucHienLcnt_Index",
        //         table: "DmTinhTrangThucHienLcnt",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmTinhTrangThucHienLcnt_Ma",
        //         table: "DmTinhTrangThucHienLcnt",
        //         column: "Ma",
        //         unique: true,
        //         filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmTrangThaiDuAn_Index",
        //         table: "DmTrangThaiDuAn",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmTrangThaiDuAn_Ma",
        //         table: "DmTrangThaiDuAn",
        //         column: "Ma",
        //         unique: true,
        //         filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmTrangThaiPheDuyet_Index",
        //         table: "DmTrangThaiPheDuyet",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmTrangThaiPheDuyet_Ma_Loai",
        //         table: "DmTrangThaiPheDuyet",
        //         columns: new[] { "Ma", "Loai" },
        //         unique: true,
        //         filter: "[Ma] IS NOT NULL AND [Ma] <> '' AND [IsDeleted] = 0");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmTrangThaiTienDo_Index",
        //         table: "DmTrangThaiTienDo",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DmTrangThaiTienDo_Ma",
        //         table: "DmTrangThaiTienDo",
        //         column: "Ma",
        //         unique: true,
        //         filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DuAn_BuocHienTaiId",
        //         table: "DuAn",
        //         column: "BuocHienTaiId",
        //         unique: true,
        //         filter: "[BuocHienTaiId] IS NOT NULL");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DuAn_ChuDauTuId",
        //         table: "DuAn",
        //         column: "ChuDauTuId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DuAn_GiaiDoanHienTaiId",
        //         table: "DuAn",
        //         column: "GiaiDoanHienTaiId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DuAn_HinhThucDauTuId",
        //         table: "DuAn",
        //         column: "HinhThucDauTuId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DuAn_HinhThucQuanLyDuAnId",
        //         table: "DuAn",
        //         column: "HinhThucQuanLyDuAnId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DuAn_Index",
        //         table: "DuAn",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DuAn_LinhVucId",
        //         table: "DuAn",
        //         column: "LinhVucId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DuAn_LoaiDuAnId",
        //         table: "DuAn",
        //         column: "LoaiDuAnId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DuAn_LoaiDuAnTheoNamId",
        //         table: "DuAn",
        //         column: "LoaiDuAnTheoNamId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DuAn_NhomDuAnId",
        //         table: "DuAn",
        //         column: "NhomDuAnId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DuAn_QuyTrinhId",
        //         table: "DuAn",
        //         column: "QuyTrinhId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DuAn_TrangThaiDuAnId",
        //         table: "DuAn",
        //         column: "TrangThaiDuAnId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DuAn_TrangThaiHienTaiId",
        //         table: "DuAn",
        //         column: "TrangThaiHienTaiId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DuAnBuoc_BuocId",
        //         table: "DuAnBuoc",
        //         column: "BuocId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DuAnBuoc_DuAnId",
        //         table: "DuAnBuoc",
        //         column: "DuAnId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DuAnBuoc_Index",
        //         table: "DuAnBuoc",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DuAnBuocManHinh_ManHinhId",
        //         table: "DuAnBuocManHinh",
        //         column: "ManHinhId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DuAnNguonVon_NguonVonId",
        //         table: "DuAnNguonVon",
        //         column: "NguonVonId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DuToan_DuAnId",
        //         table: "DuToan",
        //         column: "DuAnId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_DuToan_Index",
        //         table: "DuToan",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_E_LoaiVanBanQuyetDinh_Ma",
        //         table: "E_LoaiVanBanQuyetDinh",
        //         column: "Ma",
        //         unique: true,
        //         filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_E_ManHinh_Ma",
        //         table: "E_ManHinh",
        //         column: "Ma",
        //         unique: true,
        //         filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_E_ManHinh_Ten",
        //         table: "E_ManHinh",
        //         column: "Ten",
        //         unique: true,
        //         filter: "[Ten] IS NOT NULL");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_GoiThau_DuAnBuocId",
        //         table: "GoiThau",
        //         column: "DuAnBuocId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_GoiThau_DuAnId",
        //         table: "GoiThau",
        //         column: "DuAnId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_GoiThau_HinhThucLuaChonNhaThauId",
        //         table: "GoiThau",
        //         column: "HinhThucLuaChonNhaThauId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_GoiThau_Index",
        //         table: "GoiThau",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_GoiThau_KeHoachLuaChonNhaThauId",
        //         table: "GoiThau",
        //         column: "KeHoachLuaChonNhaThauId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_GoiThau_LoaiHopDongId",
        //         table: "GoiThau",
        //         column: "LoaiHopDongId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_GoiThau_NguonVonId",
        //         table: "GoiThau",
        //         column: "NguonVonId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_GoiThau_PhuongThucLuaChonNhaThauId",
        //         table: "GoiThau",
        //         column: "PhuongThucLuaChonNhaThauId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_HopDong_BuocId",
        //         table: "HopDong",
        //         column: "BuocId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_HopDong_DonViThucHienId",
        //         table: "HopDong",
        //         column: "DonViThucHienId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_HopDong_DuAnId",
        //         table: "HopDong",
        //         column: "DuAnId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_HopDong_GoiThauId",
        //         table: "HopDong",
        //         column: "GoiThauId",
        //         unique: true,
        //         filter: "[IsDeleted] = 0");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_HopDong_Index",
        //         table: "HopDong",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_HopDong_LoaiHopDongId",
        //         table: "HopDong",
        //         column: "LoaiHopDongId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_HoSoDeXuatCapDoCntt_CapDoId",
        //         table: "HoSoDeXuatCapDoCntt",
        //         column: "CapDoId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_HoSoDeXuatCapDoCntt_DuAnId",
        //         table: "HoSoDeXuatCapDoCntt",
        //         column: "DuAnId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_HoSoDeXuatCapDoCntt_Index",
        //         table: "HoSoDeXuatCapDoCntt",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_HoSoDeXuatCapDoCntt_TrangThaiId",
        //         table: "HoSoDeXuatCapDoCntt",
        //         column: "TrangThaiId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_HoSoMoiThauDienTu_BuocId",
        //         table: "HoSoMoiThauDienTu",
        //         column: "BuocId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_HoSoMoiThauDienTu_DuAnId",
        //         table: "HoSoMoiThauDienTu",
        //         column: "DuAnId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_HoSoMoiThauDienTu_GoiThauId",
        //         table: "HoSoMoiThauDienTu",
        //         column: "GoiThauId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_HoSoMoiThauDienTu_HinhThucLuaChonNhaThauId",
        //         table: "HoSoMoiThauDienTu",
        //         column: "HinhThucLuaChonNhaThauId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_HoSoMoiThauDienTu_Index",
        //         table: "HoSoMoiThauDienTu",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_HoSoMoiThauDienTu_TrangThaiId",
        //         table: "HoSoMoiThauDienTu",
        //         column: "TrangThaiId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_KeHoachVon_DuAnId",
        //         table: "KeHoachVon",
        //         column: "DuAnId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_KeHoachVon_Index",
        //         table: "KeHoachVon",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_KeHoachVon_NguonVonId",
        //         table: "KeHoachVon",
        //         column: "NguonVonId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_KetQuaTrungThau_BuocId",
        //         table: "KetQuaTrungThau",
        //         column: "BuocId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_KetQuaTrungThau_DonViTrungThauId",
        //         table: "KetQuaTrungThau",
        //         column: "DonViTrungThauId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_KetQuaTrungThau_DuAnId",
        //         table: "KetQuaTrungThau",
        //         column: "DuAnId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_KetQuaTrungThau_GoiThauId",
        //         table: "KetQuaTrungThau",
        //         column: "GoiThauId",
        //         unique: true,
        //         filter: "[IsDeleted] = 0");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_KetQuaTrungThau_Index",
        //         table: "KetQuaTrungThau",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_KetQuaTrungThau_LoaiGoiThauId",
        //         table: "KetQuaTrungThau",
        //         column: "LoaiGoiThauId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_KySo_ChucVuId",
        //         table: "KySo",
        //         column: "ChucVuId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_KySo_Index",
        //         table: "KySo",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_KySo_PhuongThucKySoId",
        //         table: "KySo",
        //         column: "PhuongThucKySoId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_NghiemThu_DuAnBuocId",
        //         table: "NghiemThu",
        //         column: "DuAnBuocId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_NghiemThu_DuAnId",
        //         table: "NghiemThu",
        //         column: "DuAnId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_NghiemThu_HopDongId",
        //         table: "NghiemThu",
        //         column: "HopDongId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_NghiemThu_Index",
        //         table: "NghiemThu",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_NghiemThuPhuLucHopDong_PhuLucHopDongId",
        //         table: "NghiemThuPhuLucHopDong",
        //         column: "PhuLucHopDongId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_NhaThauNguoiDung_Index",
        //         table: "NhaThauNguoiDung",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_NhaThauNguoiDung_NhaThauId_NguoiDungId",
        //         table: "NhaThauNguoiDung",
        //         columns: new[] { "NhaThauId", "NguoiDungId" },
        //         unique: true);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_PhanKhaiKinhPhi_DuAnId",
        //         table: "PhanKhaiKinhPhi",
        //         column: "DuAnId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_PhanKhaiKinhPhi_NguonVonId",
        //         table: "PhanKhaiKinhPhi",
        //         column: "NguonVonId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_PhanKhaiKinhPhi_TrangThaiId",
        //         table: "PhanKhaiKinhPhi",
        //         column: "TrangThaiId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_PheDuyetDuToan_ChucVuId",
        //         table: "PheDuyetDuToan",
        //         column: "ChucVuId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_PheDuyetDuToan_TrangThaiId",
        //         table: "PheDuyetDuToan",
        //         column: "TrangThaiId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_PheDuyetHistory_DuAnId",
        //         table: "PheDuyetHistory",
        //         column: "DuAnId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_PheDuyetHistory_EntityName_EntityId",
        //         table: "PheDuyetHistory",
        //         columns: new[] { "EntityName", "EntityId" });

        //     migrationBuilder.CreateIndex(
        //         name: "IX_PheDuyetHistory_Index",
        //         table: "PheDuyetHistory",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_PheDuyetHistory_TrangThaiId",
        //         table: "PheDuyetHistory",
        //         column: "TrangThaiId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_PhuLucHopDong_BuocId",
        //         table: "PhuLucHopDong",
        //         column: "BuocId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_PhuLucHopDong_DuAnId",
        //         table: "PhuLucHopDong",
        //         column: "DuAnId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_PhuLucHopDong_HopDongId",
        //         table: "PhuLucHopDong",
        //         column: "HopDongId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_PhuLucHopDong_Index",
        //         table: "PhuLucHopDong",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_QuyetDinhDieuChinh_DuAnId",
        //         table: "QuyetDinhDieuChinh",
        //         column: "DuAnId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_QuyetDinhDieuChinh_LoaiDieuChinhId",
        //         table: "QuyetDinhDieuChinh",
        //         column: "LoaiDieuChinhId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_QuyetDinhDieuChinh_PheDuyetEntityName_PheDuyetEntityId",
        //         table: "QuyetDinhDieuChinh",
        //         columns: new[] { "PheDuyetEntityName", "PheDuyetEntityId" });

        //     migrationBuilder.CreateIndex(
        //         name: "IX_QuyetDinhDieuChinh_TrangThaiId",
        //         table: "QuyetDinhDieuChinh",
        //         column: "TrangThaiId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_QuyetDinhDuyetDuAnHangMuc_Index",
        //         table: "QuyetDinhDuyetDuAnHangMuc",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_QuyetDinhDuyetDuAnHangMuc_QuyetDinhDuyetDuAnNguonVonId",
        //         table: "QuyetDinhDuyetDuAnHangMuc",
        //         column: "QuyetDinhDuyetDuAnNguonVonId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_QuyetDinhDuyetDuAnNguonVon_Index",
        //         table: "QuyetDinhDuyetDuAnNguonVon",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_QuyetDinhDuyetDuAnNguonVon_QuyetDinhDuyetDuAnId",
        //         table: "QuyetDinhDuyetDuAnNguonVon",
        //         column: "QuyetDinhDuyetDuAnId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_QuyetDinhDuyetKHLCNT_KeHoachLuaChonNhaThauId",
        //         table: "QuyetDinhDuyetKHLCNT",
        //         column: "KeHoachLuaChonNhaThauId",
        //         unique: true,
        //         filter: "[KeHoachLuaChonNhaThauId] IS NOT NULL");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_TamUng_BuocId",
        //         table: "TamUng",
        //         column: "BuocId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_TamUng_DuAnId",
        //         table: "TamUng",
        //         column: "DuAnId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_TamUng_HopDongId",
        //         table: "TamUng",
        //         column: "HopDongId",
        //         unique: true,
        //         filter: "[IsDeleted] = 0");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_TamUng_Index",
        //         table: "TamUng",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_TepDinhKem_Index",
        //         table: "TepDinhKem",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_ThanhToan_BuocId",
        //         table: "ThanhToan",
        //         column: "BuocId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_ThanhToan_DuAnId",
        //         table: "ThanhToan",
        //         column: "DuAnId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_ThanhToan_Index",
        //         table: "ThanhToan",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_ThanhToan_NghiemThuId",
        //         table: "ThanhToan",
        //         column: "NghiemThuId",
        //         unique: true,
        //         filter: "[IsDeleted] = 0");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_ThanhVienBanQLDA_Index",
        //         table: "ThanhVienBanQLDA",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_ThanhVienBanQLDA_QuyetDinhId",
        //         table: "ThanhVienBanQLDA",
        //         column: "QuyetDinhId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_ThongTinDieuChinhChiPhi_QuyetDinhDieuChinhId",
        //         table: "ThongTinDieuChinhChiPhi",
        //         column: "QuyetDinhDieuChinhId",
        //         unique: true);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_ToTrinhKeHoach_DuAnId",
        //         table: "ToTrinhKeHoach",
        //         column: "DuAnId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_ToTrinhKeHoach_Index",
        //         table: "ToTrinhKeHoach",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.CreateIndex(
        //         name: "IX_ToTrinhKeHoach_TrangThaiId",
        //         table: "ToTrinhKeHoach",
        //         column: "TrangThaiId");

        //     migrationBuilder.CreateIndex(
        //         name: "IDX_USER_MASTER_01",
        //         table: "USER_MASTER",
        //         columns: new[] { "DonViID", "User_PortalID", "Used" });

        //     migrationBuilder.CreateIndex(
        //         name: "IDX_USER_MASTER_02",
        //         table: "USER_MASTER",
        //         column: "User_PortalID");

        //     migrationBuilder.CreateIndex(
        //         name: "IDX_USER_MASTER_03",
        //         table: "USER_MASTER",
        //         columns: new[] { "PhongBanID", "DonViID", "User_PortalID" });

        //     migrationBuilder.CreateIndex(
        //         name: "IDX_USER_MASTER_04",
        //         table: "USER_MASTER",
        //         columns: new[] { "DonViID", "User_PortalID" });

        //     migrationBuilder.CreateIndex(
        //         name: "IDX_USER_MASTER_05",
        //         table: "USER_MASTER",
        //         column: "User_PortalID");

        //     migrationBuilder.CreateIndex(
        //         name: "IDX_USER_MASTER_06",
        //         table: "USER_MASTER",
        //         column: "DonViID");

        //     migrationBuilder.CreateIndex(
        //         name: "IDX_USER_MASTER_07",
        //         table: "USER_MASTER",
        //         columns: new[] { "DonViID", "User_PortalID" });

        //     migrationBuilder.CreateIndex(
        //         name: "IX_USER_MASTER_122_121",
        //         table: "USER_MASTER",
        //         column: "Used");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_USER_MASTER_CanBoID_Used",
        //         table: "USER_MASTER",
        //         columns: new[] { "CanBoID", "Used" });

        //     migrationBuilder.CreateIndex(
        //         name: "IX_VanBanChuTruong_ChucVuId",
        //         table: "VanBanChuTruong",
        //         column: "ChucVuId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_VanBanChuTruong_LoaiVanBanId",
        //         table: "VanBanChuTruong",
        //         column: "LoaiVanBanId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_VanBanPhapLy_ChucVuId",
        //         table: "VanBanPhapLy",
        //         column: "ChucVuId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_VanBanPhapLy_ChuDauTuId",
        //         table: "VanBanPhapLy",
        //         column: "ChuDauTuId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_VanBanPhapLy_LoaiVanBanId",
        //         table: "VanBanPhapLy",
        //         column: "LoaiVanBanId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_VanBanQuyetDinh_BuocId",
        //         table: "VanBanQuyetDinh",
        //         column: "BuocId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_VanBanQuyetDinh_DuAnId",
        //         table: "VanBanQuyetDinh",
        //         column: "DuAnId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_VanBanQuyetDinh_Index",
        //         table: "VanBanQuyetDinh",
        //         column: "Index")
        //         .Annotation("SqlServer:Clustered", false);

        //     migrationBuilder.AddForeignKey(
        //         name: "FK_BanGiaoHoSo_DuAnBuoc_BuocId",
        //         table: "BanGiaoHoSo",
        //         column: "BuocId",
        //         principalTable: "DuAnBuoc",
        //         principalColumn: "Id",
        //         onDelete: ReferentialAction.SetNull);

        //     migrationBuilder.AddForeignKey(
        //         name: "FK_BanGiaoHoSo_DuAn_DuAnId",
        //         table: "BanGiaoHoSo",
        //         column: "DuAnId",
        //         principalTable: "DuAn",
        //         principalColumn: "Id",
        //         onDelete: ReferentialAction.SetNull);

        //     migrationBuilder.AddForeignKey(
        //         name: "FK_BaoCao_DuAnBuoc_BuocId",
        //         table: "BaoCao",
        //         column: "BuocId",
        //         principalTable: "DuAnBuoc",
        //         principalColumn: "Id",
        //         onDelete: ReferentialAction.Restrict);

        //     migrationBuilder.AddForeignKey(
        //         name: "FK_BaoCao_DuAn_DuAnId",
        //         table: "BaoCao",
        //         column: "DuAnId",
        //         principalTable: "DuAn",
        //         principalColumn: "Id",
        //         onDelete: ReferentialAction.Restrict);

        //     migrationBuilder.AddForeignKey(
        //         name: "FK_DangTaiKeHoachLcntLenMang_DuAnBuoc_BuocId",
        //         table: "DangTaiKeHoachLcntLenMang",
        //         column: "BuocId",
        //         principalTable: "DuAnBuoc",
        //         principalColumn: "Id",
        //         onDelete: ReferentialAction.Restrict);

        //     migrationBuilder.AddForeignKey(
        //         name: "FK_DangTaiKeHoachLcntLenMang_KeHoachLuaChonNhaThau_KeHoachLuaChonNhaThauId",
        //         table: "DangTaiKeHoachLcntLenMang",
        //         column: "KeHoachLuaChonNhaThauId",
        //         principalTable: "KeHoachLuaChonNhaThau",
        //         principalColumn: "Id",
        //         onDelete: ReferentialAction.Cascade);

        //     migrationBuilder.AddForeignKey(
        //         name: "FK_DeXuatChuTruongMoi_DuAn_DuAnId",
        //         table: "DeXuatChuTruongMoi",
        //         column: "DuAnId",
        //         principalTable: "DuAn",
        //         principalColumn: "Id",
        //         onDelete: ReferentialAction.Restrict);

        //     migrationBuilder.AddForeignKey(
        //         name: "FK_DeXuatChuyenTiep_DuAn_DuAnId",
        //         table: "DeXuatChuyenTiep",
        //         column: "DuAnId",
        //         principalTable: "DuAn",
        //         principalColumn: "Id",
        //         onDelete: ReferentialAction.Restrict);

        //     migrationBuilder.AddForeignKey(
        //         name: "FK_DeXuatNhuCauKinhPhi_DuAn_DuAnId",
        //         table: "DeXuatNhuCauKinhPhi",
        //         column: "DuAnId",
        //         principalTable: "DuAn",
        //         principalColumn: "Id",
        //         onDelete: ReferentialAction.Restrict);

        //     migrationBuilder.AddForeignKey(
        //         name: "FK_DuAn_DuAnBuoc_BuocHienTaiId",
        //         table: "DuAn",
        //         column: "BuocHienTaiId",
        //         principalTable: "DuAnBuoc",
        //         principalColumn: "Id");
        // }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DuAn_DuAnBuoc_BuocHienTaiId",
                table: "DuAn");

            migrationBuilder.DropTable(
                name: "AuditLog");

            migrationBuilder.DropTable(
                name: "BanGiaoHoSo");

            migrationBuilder.DropTable(
                name: "BaoCaoBanGiaoSanPham");

            migrationBuilder.DropTable(
                name: "BaoCaoBaoHanhSanPham");

            migrationBuilder.DropTable(
                name: "BaoCaoKhoKhanVuongMac");

            migrationBuilder.DropTable(
                name: "BaoCaoTienDo");

            migrationBuilder.DropTable(
                name: "CauHinhVaiTroQuyen");

            migrationBuilder.DropTable(
                name: "DangTaiKeHoachLcntLenMang");

            migrationBuilder.DropTable(
                name: "DeXuatChuyenTiep");

            migrationBuilder.DropTable(
                name: "DeXuatDonViXuLy");

            migrationBuilder.DropTable(
                name: "DeXuatNhuCauKinhPhi");

            migrationBuilder.DropTable(
                name: "DmBuocManHinh");

            migrationBuilder.DropTable(
                name: "DmBuocTrangThaiTienDo");

            migrationBuilder.DropTable(
                name: "DmTinhTrangThucHienLcnt");

            migrationBuilder.DropTable(
                name: "DuAnBuocManHinh");

            migrationBuilder.DropTable(
                name: "DuAnBuocPhongBanPhoiHop");

            migrationBuilder.DropTable(
                name: "DuAnChiuTrachNhiemXuLy");

            migrationBuilder.DropTable(
                name: "DuAnCongViec");

            migrationBuilder.DropTable(
                name: "DuAnNguonVon");

            migrationBuilder.DropTable(
                name: "DuToan");

            migrationBuilder.DropTable(
                name: "E_LoaiVanBanQuyetDinh");

            migrationBuilder.DropTable(
                name: "HoSoDeXuatCapDoCntt");

            migrationBuilder.DropTable(
                name: "HoSoMoiThauDienTu");

            migrationBuilder.DropTable(
                name: "KeHoachVon");

            migrationBuilder.DropTable(
                name: "KetQuaTrungThau");

            migrationBuilder.DropTable(
                name: "KySo");

            migrationBuilder.DropTable(
                name: "NghiemThuPhuLucHopDong");

            migrationBuilder.DropTable(
                name: "NhaThauNguoiDung");

            migrationBuilder.DropTable(
                name: "PhanKhaiKinhPhi");

            migrationBuilder.DropTable(
                name: "PheDuyetDuToan");

            migrationBuilder.DropTable(
                name: "PheDuyetHistory");

            migrationBuilder.DropTable(
                name: "QuyetDinhDuyetDuAnHangMuc");

            migrationBuilder.DropTable(
                name: "QuyetDinhDuyetKHLCNT");

            migrationBuilder.DropTable(
                name: "QuyetDinhDuyetQuyetToan");

            migrationBuilder.DropTable(
                name: "QuyetDinhLapBenMoiThau");

            migrationBuilder.DropTable(
                name: "QuyetDinhLapHoiDongThamDinh");

            migrationBuilder.DropTable(
                name: "TamUng");

            migrationBuilder.DropTable(
                name: "TepDinhKem");

            migrationBuilder.DropTable(
                name: "ThanhToan");

            migrationBuilder.DropTable(
                name: "ThanhVienBanQLDA");

            migrationBuilder.DropTable(
                name: "ThongTinDieuChinhChiPhi");

            migrationBuilder.DropTable(
                name: "ToTrinhKeHoach");

            migrationBuilder.DropTable(
                name: "USER_MASTER");

            migrationBuilder.DropTable(
                name: "VanBanChuTruong");

            migrationBuilder.DropTable(
                name: "VanBanPhapLy");

            migrationBuilder.DropTable(
                name: "DmMucDoKhoKhan");

            migrationBuilder.DropTable(
                name: "DmTinhTrangKhoKhan");

            migrationBuilder.DropTable(
                name: "BaoCao");

            migrationBuilder.DropTable(
                name: "DmQuyen");

            migrationBuilder.DropTable(
                name: "DeXuatChuTruongMoi");

            migrationBuilder.DropTable(
                name: "E_ManHinh");

            migrationBuilder.DropTable(
                name: "DmCapDoCntt");

            migrationBuilder.DropTable(
                name: "DmLoaiGoiThau");

            migrationBuilder.DropTable(
                name: "DmPhuongThucKySo");

            migrationBuilder.DropTable(
                name: "PhuLucHopDong");

            migrationBuilder.DropTable(
                name: "QuyetDinhDuyetDuAnNguonVon");

            migrationBuilder.DropTable(
                name: "NghiemThu");

            migrationBuilder.DropTable(
                name: "QuyetDinhLapBanQLDA");

            migrationBuilder.DropTable(
                name: "QuyetDinhDieuChinh");

            migrationBuilder.DropTable(
                name: "DmChucVu");

            migrationBuilder.DropTable(
                name: "DmLoaiVanBan");

            migrationBuilder.DropTable(
                name: "QuyetDinhDuyetDuAn");

            migrationBuilder.DropTable(
                name: "HopDong");

            migrationBuilder.DropTable(
                name: "DanhMucLoaiDieuChinh");

            migrationBuilder.DropTable(
                name: "DmTrangThaiPheDuyet");

            migrationBuilder.DropTable(
                name: "DmNhaThau");

            migrationBuilder.DropTable(
                name: "GoiThau");

            migrationBuilder.DropTable(
                name: "DmHinhThucLuaChonNhaThau");

            migrationBuilder.DropTable(
                name: "DmLoaiHopDong");

            migrationBuilder.DropTable(
                name: "DmNguonVon");

            migrationBuilder.DropTable(
                name: "DmPhuongThucLuaChonNhaThau");

            migrationBuilder.DropTable(
                name: "KeHoachLuaChonNhaThau");

            migrationBuilder.DropTable(
                name: "VanBanQuyetDinh");

            migrationBuilder.DropTable(
                name: "DuAnBuoc");

            migrationBuilder.DropTable(
                name: "DmBuoc");

            migrationBuilder.DropTable(
                name: "DuAn");

            migrationBuilder.DropTable(
                name: "DmChuDauTu");

            migrationBuilder.DropTable(
                name: "DmGiaiDoan");

            migrationBuilder.DropTable(
                name: "DmHinhThucDauTu");

            migrationBuilder.DropTable(
                name: "DmHinhThucQuanLy");

            migrationBuilder.DropTable(
                name: "DmLinhVuc");

            migrationBuilder.DropTable(
                name: "DmLoaiDuAnTheoNam");

            migrationBuilder.DropTable(
                name: "DmLoaiDuAn");

            migrationBuilder.DropTable(
                name: "DmNhomDuAn");

            migrationBuilder.DropTable(
                name: "DmQuyTrinh");

            migrationBuilder.DropTable(
                name: "DmTrangThaiDuAn");

            migrationBuilder.DropTable(
                name: "DmTrangThaiTienDo");
        }
    }
}
