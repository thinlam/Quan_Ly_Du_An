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
        // protected override void Up(MigrationBuilder migrationBuilder){}

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CANBO",
                columns: table => new
                {
                    CanBoID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NgheNghiep = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    MaSoCanBo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SoBHXH = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ConNguoiID = table.Column<long>(type: "bigint", nullable: true),
                    ChuyenMon = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ChucDanhID = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CANBO", x => x.CanBoID);
                });

            migrationBuilder.CreateTable(
                name: "DM_DONVI",
                columns: table => new
                {
                    DonViID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaDonVi = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TenDonVi = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    TenVietTat = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DonViCapChaID = table.Column<long>(type: "bigint", nullable: true),
                    Cap = table.Column<int>(type: "int", nullable: true),
                    CapDonViID = table.Column<long>(type: "bigint", nullable: true),
                    LoaiDonViID = table.Column<long>(type: "bigint", nullable: true),
                    SoNha = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DuongID = table.Column<long>(type: "bigint", nullable: true),
                    TinhThanhID = table.Column<long>(type: "bigint", nullable: true),
                    QuanHuyenID = table.Column<long>(type: "bigint", nullable: true),
                    PhuongXaID = table.Column<long>(type: "bigint", nullable: true),
                    DiaChiDayDu = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DienThoai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Fax = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Website = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Used = table.Column<bool>(type: "bit", nullable: true),
                    Latitude = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Longitude = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DM_DONVI__1CB88576D84B4D4C", x => x.DonViID)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "DM_DUONG",
                columns: table => new
                {
                    DuongID = table.Column<long>(type: "bigint", nullable: false),
                    TenVietTat = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TenDuong = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Used = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "DM_PHUONGXA",
                columns: table => new
                {
                    PhuongXaID = table.Column<long>(type: "bigint", nullable: false),
                    MaPhuongXa = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    TenPhuongXa = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    QuanHuyenID = table.Column<long>(type: "bigint", nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Used = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "DM_QUANHUYEN",
                columns: table => new
                {
                    QuanHuyenID = table.Column<long>(type: "bigint", nullable: false),
                    MaQuanHuyen = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    TenQuanHuyen = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TinhThanhID = table.Column<long>(type: "bigint", nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Used = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "DM_TINHTHANH",
                columns: table => new
                {
                    TinhThanhID = table.Column<long>(type: "bigint", nullable: false),
                    QuocGiaID = table.Column<long>(type: "bigint", nullable: false),
                    MaTinhThanh = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    TenTinhThanh = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Used = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "DmChucVu",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Stt = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
                    Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Used = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DmChucVu", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DmChuDauTu",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Stt = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
                    Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Used = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DmChuDauTu", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DmGiaiDoan",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Stt = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
                    Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Used = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DmGiaiDoan", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DmHinhThucDauTu",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Stt = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
                    Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Used = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DmHinhThucDauTu", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DmHinhThucLuaChonNhaThau",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Stt = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
                    Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Used = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DmHinhThucLuaChonNhaThau", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DmHinhThucQuanLy",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Stt = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
                    Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Used = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DmHinhThucQuanLy", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DmLinhVuc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Stt = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
                    Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Used = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DmLinhVuc", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DmLoaiDuAn",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Stt = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
                    Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Used = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DmLoaiDuAn", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DmLoaiDuAnTheoNam",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Stt = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
                    Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Used = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DmLoaiDuAnTheoNam", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DmLoaiGoiThau",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Stt = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
                    Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Used = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DmLoaiGoiThau", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DmLoaiHopDong",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Stt = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
                    Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Used = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DmLoaiHopDong", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DmLoaiVanBan",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Stt = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
                    Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Used = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DmLoaiVanBan", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DmMucDoKhoKhan",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Stt = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
                    Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Used = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DmMucDoKhoKhan", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DmNguonVon",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Stt = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
                    Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Used = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DmNguonVon", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DmNhaThau",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    Stt = table.Column<int>(type: "int", nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaSoThue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SoDienThoai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NguoiDaiDien = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
                    Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Used = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DmNhaThau", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DmNhomDuAn",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Stt = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
                    Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Used = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DmNhomDuAn", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DmPhuongThucLuaChonNhaThau",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Stt = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
                    Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Used = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DmPhuongThucLuaChonNhaThau", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DmQuyTrinh",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Stt = table.Column<int>(type: "int", nullable: true),
                    MacDinh = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
                    Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Used = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DmQuyTrinh", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DmTinhTrangKhoKhan",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Stt = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
                    Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Used = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DmTinhTrangKhoKhan", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DmTinhTrangThucHienLcnt",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Stt = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
                    Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Used = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DmTinhTrangThucHienLcnt", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DmTrangThaiDuAn",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Stt = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
                    Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Used = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DmTrangThaiDuAn", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DmTrangThaiTienDo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Stt = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
                    Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Used = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DmTrangThaiTienDo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DUONG_PHUONG_QUAN",
                columns: table => new
                {
                    DuongPhuongQuanID = table.Column<long>(type: "bigint", nullable: false),
                    DuongID = table.Column<long>(type: "bigint", nullable: true),
                    PhuongXaID = table.Column<long>(type: "bigint", nullable: true),
                    QuanHuyenID = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "E_CAPDONVI",
                columns: table => new
                {
                    CapDonViID = table.Column<long>(type: "bigint", nullable: false),
                    TenCapDonVi = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ThuTuHienThi = table.Column<int>(type: "int", nullable: true),
                    Used = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_E_CAPDONVI", x => x.CapDonViID);
                });

            migrationBuilder.CreateTable(
                name: "E_LoaiVanBanQuyetDinh",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Used = table.Column<bool>(type: "bit", nullable: false),
                    Ma = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_E_LoaiVanBanQuyetDinh", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "E_ManHinh",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Used = table.Column<bool>(type: "bit", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Stt = table.Column<int>(type: "int", nullable: true),
                    Ma = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_E_ManHinh", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "E_VAITROCHUCVU",
                columns: table => new
                {
                    VaiTro = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChucVu = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Cap = table.Column<int>(type: "int", nullable: true),
                    Used = table.Column<bool>(type: "bit", nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_E_VAITROCHUCVU", x => x.VaiTro);
                });

            migrationBuilder.CreateTable(
                name: "TepDinhKem",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GroupId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GroupType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OriginalName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Path = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TepDinhKem", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "USER_MASTER",
                columns: table => new
                {
                    User_MasterID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    HoTen = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PhongBanID = table.Column<long>(type: "bigint", nullable: true),
                    DonViID = table.Column<long>(type: "bigint", nullable: false),
                    User_PortalID = table.Column<long>(type: "bigint", nullable: true),
                    CanBoID = table.Column<long>(type: "bigint", nullable: true),
                    LaDonViChinh = table.Column<bool>(type: "bit", nullable: true),
                    Used = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__USER_MAS__CA9BC5E270CE69C2", x => x.User_MasterID);
                });

            migrationBuilder.CreateTable(
                name: "UserSession",
                columns: table => new
                {
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UserName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Platform = table.Column<int>(type: "int", nullable: false),
                    DeviceName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RefreshToken = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RefreshTokenExpiresAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsRemembered = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    LastActivityAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IpAddress = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    UserInfoJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserAuthInfoJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSession", x => x.SessionId);
                });

            migrationBuilder.CreateTable(
                name: "CANBO_DONVI",
                columns: table => new
                {
                    CanBoDonViID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CanBoID = table.Column<long>(type: "bigint", nullable: true),
                    DonViID = table.Column<long>(type: "bigint", nullable: true),
                    ChucVuID = table.Column<long>(type: "bigint", nullable: true),
                    LaChucVuChinh = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CANBO_DONVI", x => x.CanBoDonViID);
                    table.ForeignKey(
                        name: "FK_CANBO_DONVI_CANBO_CanBoID",
                        column: x => x.CanBoID,
                        principalTable: "CANBO",
                        principalColumn: "CanBoID");
                    table.ForeignKey(
                        name: "FK_CANBO_DONVI_DM_DONVI_DonViID",
                        column: x => x.DonViID,
                        principalTable: "DM_DONVI",
                        principalColumn: "DonViID");
                });

            migrationBuilder.CreateTable(
                name: "NhaThauNguoiDung",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NhaThauId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguoiDungId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhaThauNguoiDung", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NhaThauNguoiDung_DmNhaThau_NhaThauId",
                        column: x => x.NhaThauId,
                        principalTable: "DmNhaThau",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DmBuoc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuyTrinhId = table.Column<int>(type: "int", nullable: false),
                    GiaiDoanId = table.Column<int>(type: "int", nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    Stt = table.Column<int>(type: "int", nullable: true),
                    PartialView = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SoNgayThucHien = table.Column<int>(type: "int", nullable: false, defaultValueSql: "1"),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
                    Path = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Level = table.Column<int>(type: "int", nullable: false),
                    Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Used = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DmBuoc", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DmBuoc_DmGiaiDoan_GiaiDoanId",
                        column: x => x.GiaiDoanId,
                        principalTable: "DmGiaiDoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DmBuoc_DmQuyTrinh_QuyTrinhId",
                        column: x => x.QuyTrinhId,
                        principalTable: "DmQuyTrinh",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DmBuocManHinh",
                columns: table => new
                {
                    BuocId = table.Column<int>(type: "int", nullable: false),
                    ManHinhId = table.Column<int>(type: "int", nullable: false),
                    Stt = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DmBuocManHinh", x => new { x.BuocId, x.ManHinhId });
                    table.ForeignKey(
                        name: "FK_DmBuocManHinh_DmBuoc_BuocId",
                        column: x => x.BuocId,
                        principalTable: "DmBuoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DmBuocManHinh_E_ManHinh_ManHinhId",
                        column: x => x.ManHinhId,
                        principalTable: "E_ManHinh",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DmBuocTrangThaiTienDo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BuocId = table.Column<int>(type: "int", nullable: false),
                    TrangThaiId = table.Column<int>(type: "int", nullable: false),
                    Stt = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
                    Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Used = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DmBuocTrangThaiTienDo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DmBuocTrangThaiTienDo_DmBuoc_BuocId",
                        column: x => x.BuocId,
                        principalTable: "DmBuoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DmBuocTrangThaiTienDo_DmTrangThaiTienDo_TrangThaiId",
                        column: x => x.TrangThaiId,
                        principalTable: "DmTrangThaiTienDo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BaoCao",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuocId = table.Column<int>(type: "int", nullable: true),
                    Ngay = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Loai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaoCao", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BaoCaoBanGiaoSanPham",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DonViBanGiaoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DonViNhanBanGiaoId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaoCaoBanGiaoSanPham", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BaoCaoBanGiaoSanPham_BaoCao_Id",
                        column: x => x.Id,
                        principalTable: "BaoCao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BaoCaoBanGiaoSanPham_DmNhaThau_DonViBanGiaoId",
                        column: x => x.DonViBanGiaoId,
                        principalTable: "DmNhaThau",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BaoCaoBaoHanhSanPham",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    LanhDaoPhuTrachId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaoCaoBaoHanhSanPham", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BaoCaoBaoHanhSanPham_BaoCao_Id",
                        column: x => x.Id,
                        principalTable: "BaoCao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BaoCaoKhoKhanVuongMac",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    MucDoKhoKhanId = table.Column<int>(type: "int", nullable: true),
                    TinhTrangId = table.Column<int>(type: "int", nullable: true),
                    HuongXuLy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KetQuaXuLy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayXuLy = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaoCaoKhoKhanVuongMac", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BaoCaoKhoKhanVuongMac_BaoCao_Id",
                        column: x => x.Id,
                        principalTable: "BaoCao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BaoCaoKhoKhanVuongMac_DmMucDoKhoKhan_MucDoKhoKhanId",
                        column: x => x.MucDoKhoKhanId,
                        principalTable: "DmMucDoKhoKhan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BaoCaoKhoKhanVuongMac_DmTinhTrangKhoKhan_TinhTrangId",
                        column: x => x.TinhTrangId,
                        principalTable: "DmTinhTrangKhoKhan",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BaoCaoTienDo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaoCaoTienDo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BaoCaoTienDo_BaoCao_Id",
                        column: x => x.Id,
                        principalTable: "BaoCao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DangTaiKeHoachLcntLenMang",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuocId = table.Column<int>(type: "int", nullable: true),
                    KeHoachLuaChonNhaThauId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NgayEHSMT = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    TrangThaiId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DangTaiKeHoachLcntLenMang", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DuAn",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenDuAn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiaDiem = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChuDauTuId = table.Column<int>(type: "int", nullable: true),
                    ThoiGianKhoiCong = table.Column<int>(type: "int", nullable: true),
                    ThoiGianHoanThanh = table.Column<int>(type: "int", nullable: true),
                    MaDuAn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaNganSach = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DuAnTrongDiem = table.Column<bool>(type: "bit", nullable: false),
                    LinhVucId = table.Column<int>(type: "int", nullable: true),
                    NhomDuAnId = table.Column<int>(type: "int", nullable: true),
                    NangLucThietKe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QuyMoDuAn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HinhThucQuanLyDuAnId = table.Column<int>(type: "int", nullable: true),
                    HinhThucDauTuId = table.Column<int>(type: "int", nullable: true),
                    LoaiDuAnId = table.Column<int>(type: "int", nullable: true),
                    TongMucDauTu = table.Column<long>(type: "bigint", nullable: true),
                    SoDuToan = table.Column<long>(type: "bigint", nullable: false),
                    NamDuToan = table.Column<int>(type: "int", nullable: false),
                    SoQuyetDinhDuToan = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NgayKyDuToan = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    NgayQuyetDinhDuToan = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DuToanHienTaiId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    KhaiToanKinhPhi = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SoDuToanCuoiCung = table.Column<long>(type: "bigint", nullable: true),
                    QuyTrinhId = table.Column<int>(type: "int", nullable: true),
                    BuocHienTaiId = table.Column<int>(type: "int", nullable: true),
                    GiaiDoanHienTaiId = table.Column<int>(type: "int", nullable: true),
                    TrangThaiHienTaiId = table.Column<int>(type: "int", nullable: true),
                    TrangThaiDuAnId = table.Column<int>(type: "int", nullable: true),
                    LoaiDuAnTheoNamId = table.Column<int>(type: "int", nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayBatDau = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LanhDaoPhuTrachId = table.Column<long>(type: "bigint", nullable: true),
                    DonViPhuTrachChinhId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
                    Path = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Level = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DuAn", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DuAn_DmChuDauTu_ChuDauTuId",
                        column: x => x.ChuDauTuId,
                        principalTable: "DmChuDauTu",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DuAn_DmGiaiDoan_GiaiDoanHienTaiId",
                        column: x => x.GiaiDoanHienTaiId,
                        principalTable: "DmGiaiDoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DuAn_DmHinhThucDauTu_HinhThucDauTuId",
                        column: x => x.HinhThucDauTuId,
                        principalTable: "DmHinhThucDauTu",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DuAn_DmHinhThucQuanLy_HinhThucQuanLyDuAnId",
                        column: x => x.HinhThucQuanLyDuAnId,
                        principalTable: "DmHinhThucQuanLy",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DuAn_DmLinhVuc_LinhVucId",
                        column: x => x.LinhVucId,
                        principalTable: "DmLinhVuc",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DuAn_DmLoaiDuAnTheoNam_LoaiDuAnTheoNamId",
                        column: x => x.LoaiDuAnTheoNamId,
                        principalTable: "DmLoaiDuAnTheoNam",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DuAn_DmLoaiDuAn_LoaiDuAnId",
                        column: x => x.LoaiDuAnId,
                        principalTable: "DmLoaiDuAn",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DuAn_DmNhomDuAn_NhomDuAnId",
                        column: x => x.NhomDuAnId,
                        principalTable: "DmNhomDuAn",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DuAn_DmQuyTrinh_QuyTrinhId",
                        column: x => x.QuyTrinhId,
                        principalTable: "DmQuyTrinh",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DuAn_DmTrangThaiDuAn_TrangThaiDuAnId",
                        column: x => x.TrangThaiDuAnId,
                        principalTable: "DmTrangThaiDuAn",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DuAn_DmTrangThaiTienDo_TrangThaiHienTaiId",
                        column: x => x.TrangThaiHienTaiId,
                        principalTable: "DmTrangThaiTienDo",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DuAnBuoc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BuocId = table.Column<int>(type: "int", nullable: false),
                    DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenBuoc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PartialView = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Used = table.Column<bool>(type: "bit", nullable: false),
                    TrangThaiId = table.Column<int>(type: "int", nullable: true),
                    NgayDuKienBatDau = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    NgayDuKienKetThuc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    NgayThucTeBatDau = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    NgayThucTeKetThuc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrachNhiemThucHien = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsKetThuc = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DuAnBuoc", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DuAnBuoc_DmBuoc_BuocId",
                        column: x => x.BuocId,
                        principalTable: "DmBuoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DuAnBuoc_DuAn_DuAnId",
                        column: x => x.DuAnId,
                        principalTable: "DuAn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DuAnChiuTrachNhiemXuLy",
                columns: table => new
                {
                    DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChiuTrachNhiemXuLyId = table.Column<long>(type: "bigint", nullable: false),
                    Loai = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DuAnChiuTrachNhiemXuLy", x => new { x.DuAnId, x.ChiuTrachNhiemXuLyId });
                    table.ForeignKey(
                        name: "FK_DuAnChiuTrachNhiemXuLy_DuAn_DuAnId",
                        column: x => x.DuAnId,
                        principalTable: "DuAn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DuAnCongViec",
                columns: table => new
                {
                    DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CongViecId = table.Column<long>(type: "bigint", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsHoanThanh = table.Column<bool>(type: "bit", nullable: true),
                    NguoiPhuTrachChinhId = table.Column<long>(type: "bigint", nullable: true),
                    NguoiTaoId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DuAnCongViec", x => new { x.DuAnId, x.CongViecId });
                    table.ForeignKey(
                        name: "FK_DuAnCongViec_DuAn_DuAnId",
                        column: x => x.DuAnId,
                        principalTable: "DuAn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DuAnNguonVon",
                columns: table => new
                {
                    DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguonVonId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DuAnNguonVon", x => new { x.DuAnId, x.NguonVonId });
                    table.ForeignKey(
                        name: "FK_DuAnNguonVon_DmNguonVon_NguonVonId",
                        column: x => x.NguonVonId,
                        principalTable: "DmNguonVon",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DuAnNguonVon_DuAn_DuAnId",
                        column: x => x.DuAnId,
                        principalTable: "DuAn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DuToan",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SoDuToan = table.Column<long>(type: "bigint", nullable: false),
                    NamDuToan = table.Column<int>(type: "int", nullable: false),
                    SoQuyetDinhDuToan = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NgayKyDuToan = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DuToan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DuToan_DuAn_DuAnId",
                        column: x => x.DuAnId,
                        principalTable: "DuAn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KeHoachVon",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguonVonId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Nam = table.Column<int>(type: "int", nullable: false),
                    SoVon = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SoVonDieuChinh = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SoQuyetDinh = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NgayKy = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeHoachVon", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KeHoachVon_DuAn_DuAnId",
                        column: x => x.DuAnId,
                        principalTable: "DuAn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DuAnBuocManHinh",
                columns: table => new
                {
                    BuocId = table.Column<int>(type: "int", nullable: false),
                    ManHinhId = table.Column<int>(type: "int", nullable: false),
                    Stt = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DuAnBuocManHinh", x => new { x.BuocId, x.ManHinhId });
                    table.ForeignKey(
                        name: "FK_DuAnBuocManHinh_DuAnBuoc_BuocId",
                        column: x => x.BuocId,
                        principalTable: "DuAnBuoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DuAnBuocManHinh_E_ManHinh_ManHinhId",
                        column: x => x.ManHinhId,
                        principalTable: "E_ManHinh",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VanBanQuyetDinh",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuocId = table.Column<int>(type: "int", nullable: true),
                    So = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ngay = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    TrichYeu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NguoiKy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayKy = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Loai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VanBanQuyetDinh", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VanBanQuyetDinh_DuAnBuoc_BuocId",
                        column: x => x.BuocId,
                        principalTable: "DuAnBuoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VanBanQuyetDinh_DuAn_DuAnId",
                        column: x => x.DuAnId,
                        principalTable: "DuAn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KeHoachLuaChonNhaThau",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    Ten = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeHoachLuaChonNhaThau", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KeHoachLuaChonNhaThau_VanBanQuyetDinh_Id",
                        column: x => x.Id,
                        principalTable: "VanBanQuyetDinh",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PheDuyetDuToan",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    ChucVuId = table.Column<int>(type: "int", nullable: true),
                    GiaTriDuThau = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PheDuyetDuToan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PheDuyetDuToan_DmChucVu_ChucVuId",
                        column: x => x.ChucVuId,
                        principalTable: "DmChucVu",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PheDuyetDuToan_VanBanQuyetDinh_Id",
                        column: x => x.Id,
                        principalTable: "VanBanQuyetDinh",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuyetDinhDuyetDuAn",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CoQuanQuyetDinhDauTu = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuyetDinhDuyetDuAn", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuyetDinhDuyetDuAn_VanBanQuyetDinh_Id",
                        column: x => x.Id,
                        principalTable: "VanBanQuyetDinh",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuyetDinhDuyetQuyetToan",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CoQuanQuyetDinh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GiaTri = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuyetDinhDuyetQuyetToan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuyetDinhDuyetQuyetToan_VanBanQuyetDinh_Id",
                        column: x => x.Id,
                        principalTable: "VanBanQuyetDinh",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuyetDinhLapBanQLDA",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuyetDinhLapBanQLDA", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuyetDinhLapBanQLDA_VanBanQuyetDinh_Id",
                        column: x => x.Id,
                        principalTable: "VanBanQuyetDinh",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuyetDinhLapBenMoiThau",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuyetDinhLapBenMoiThau", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuyetDinhLapBenMoiThau_VanBanQuyetDinh_Id",
                        column: x => x.Id,
                        principalTable: "VanBanQuyetDinh",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuyetDinhLapHoiDongThamDinh",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuyetDinhLapHoiDongThamDinh", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuyetDinhLapHoiDongThamDinh_VanBanQuyetDinh_Id",
                        column: x => x.Id,
                        principalTable: "VanBanQuyetDinh",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VanBanChuTruong",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    LoaiVanBanId = table.Column<int>(type: "int", nullable: true),
                    ChucVuId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VanBanChuTruong", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VanBanChuTruong_DmChucVu_ChucVuId",
                        column: x => x.ChucVuId,
                        principalTable: "DmChucVu",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VanBanChuTruong_DmLoaiVanBan_LoaiVanBanId",
                        column: x => x.LoaiVanBanId,
                        principalTable: "DmLoaiVanBan",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VanBanChuTruong_VanBanQuyetDinh_Id",
                        column: x => x.Id,
                        principalTable: "VanBanQuyetDinh",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VanBanPhapLy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    LoaiVanBanId = table.Column<int>(type: "int", nullable: true),
                    ChucVuId = table.Column<int>(type: "int", nullable: true),
                    ChuDauTuId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VanBanPhapLy", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VanBanPhapLy_DmChuDauTu_ChuDauTuId",
                        column: x => x.ChuDauTuId,
                        principalTable: "DmChuDauTu",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VanBanPhapLy_DmChucVu_ChucVuId",
                        column: x => x.ChucVuId,
                        principalTable: "DmChucVu",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VanBanPhapLy_DmLoaiVanBan_LoaiVanBanId",
                        column: x => x.LoaiVanBanId,
                        principalTable: "DmLoaiVanBan",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VanBanPhapLy_VanBanQuyetDinh_Id",
                        column: x => x.Id,
                        principalTable: "VanBanQuyetDinh",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GoiThau",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuocId = table.Column<int>(type: "int", nullable: true),
                    KeHoachLuaChonNhaThauId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Ten = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GiaTri = table.Column<long>(type: "bigint", nullable: true),
                    LoaiHopDongId = table.Column<int>(type: "int", nullable: true),
                    HinhThucLuaChonNhaThauId = table.Column<int>(type: "int", nullable: true),
                    PhuongThucLuaChonNhaThauId = table.Column<int>(type: "int", nullable: true),
                    ThoiGianLuaNhaThau = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThoiGianHopDong = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NguonVonId = table.Column<int>(type: "int", nullable: true),
                    TomTatCongViecChinhGoiThau = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThoiGianBatDauToChucLuaChonNhaThau = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThoiGianThucHienGoiThau = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TuyChonMuaThem = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GiamSatHoatDongDauThau = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DaDuyet = table.Column<bool>(type: "bit", nullable: false),
                    DuAnBuocId = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoiThau", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GoiThau_DmHinhThucLuaChonNhaThau_HinhThucLuaChonNhaThauId",
                        column: x => x.HinhThucLuaChonNhaThauId,
                        principalTable: "DmHinhThucLuaChonNhaThau",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GoiThau_DmLoaiHopDong_LoaiHopDongId",
                        column: x => x.LoaiHopDongId,
                        principalTable: "DmLoaiHopDong",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GoiThau_DmNguonVon_NguonVonId",
                        column: x => x.NguonVonId,
                        principalTable: "DmNguonVon",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GoiThau_DmPhuongThucLuaChonNhaThau_PhuongThucLuaChonNhaThauId",
                        column: x => x.PhuongThucLuaChonNhaThauId,
                        principalTable: "DmPhuongThucLuaChonNhaThau",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GoiThau_DuAnBuoc_DuAnBuocId",
                        column: x => x.DuAnBuocId,
                        principalTable: "DuAnBuoc",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GoiThau_DuAn_DuAnId",
                        column: x => x.DuAnId,
                        principalTable: "DuAn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GoiThau_KeHoachLuaChonNhaThau_KeHoachLuaChonNhaThauId",
                        column: x => x.KeHoachLuaChonNhaThauId,
                        principalTable: "KeHoachLuaChonNhaThau",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuyetDinhDuyetKHLCNT",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    KeHoachLuaChonNhaThauId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CoQuanQuyetDinh = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuyetDinhDuyetKHLCNT", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuyetDinhDuyetKHLCNT_KeHoachLuaChonNhaThau_KeHoachLuaChonNhaThauId",
                        column: x => x.KeHoachLuaChonNhaThauId,
                        principalTable: "KeHoachLuaChonNhaThau",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_QuyetDinhDuyetKHLCNT_VanBanQuyetDinh_Id",
                        column: x => x.Id,
                        principalTable: "VanBanQuyetDinh",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuyetDinhDuyetDuAnNguonVon",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    QuyetDinhDuyetDuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguonVonId = table.Column<int>(type: "int", nullable: false),
                    GiaTri = table.Column<long>(type: "bigint", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuyetDinhDuyetDuAnNguonVon", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuyetDinhDuyetDuAnNguonVon_QuyetDinhDuyetDuAn_QuyetDinhDuyetDuAnId",
                        column: x => x.QuyetDinhDuyetDuAnId,
                        principalTable: "QuyetDinhDuyetDuAn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ThanhVienBanQLDA",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuyetDinhId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChucVu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VaiTro = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThanhVienBanQLDA", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThanhVienBanQLDA_QuyetDinhLapBanQLDA_QuyetDinhId",
                        column: x => x.QuyetDinhId,
                        principalTable: "QuyetDinhLapBanQLDA",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HopDong",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuocId = table.Column<int>(type: "int", nullable: true),
                    GoiThauId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SoHopDong = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayKy = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    GiaTri = table.Column<long>(type: "bigint", nullable: true),
                    NgayHieuLuc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    NgayDuKienKetThuc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LoaiHopDongId = table.Column<int>(type: "int", nullable: true),
                    DonViThucHienId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsBienBan = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HopDong", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HopDong_DmLoaiHopDong_LoaiHopDongId",
                        column: x => x.LoaiHopDongId,
                        principalTable: "DmLoaiHopDong",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HopDong_DmNhaThau_DonViThucHienId",
                        column: x => x.DonViThucHienId,
                        principalTable: "DmNhaThau",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HopDong_DuAnBuoc_BuocId",
                        column: x => x.BuocId,
                        principalTable: "DuAnBuoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HopDong_DuAn_DuAnId",
                        column: x => x.DuAnId,
                        principalTable: "DuAn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HopDong_GoiThau_GoiThauId",
                        column: x => x.GoiThauId,
                        principalTable: "GoiThau",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KetQuaTrungThau",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuocId = table.Column<int>(type: "int", nullable: true),
                    GoiThauId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GiaTriTrungThau = table.Column<long>(type: "bigint", nullable: false),
                    DonViTrungThauId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SoNgayTrienKhai = table.Column<long>(type: "bigint", nullable: true),
                    TrichYeu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LoaiGoiThauId = table.Column<int>(type: "int", nullable: true),
                    NgayEHSMT = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    NgayMoThau = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    SoQuyetDinh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayQuyetDinh = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KetQuaTrungThau", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KetQuaTrungThau_DmLoaiGoiThau_LoaiGoiThauId",
                        column: x => x.LoaiGoiThauId,
                        principalTable: "DmLoaiGoiThau",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_KetQuaTrungThau_DmNhaThau_DonViTrungThauId",
                        column: x => x.DonViTrungThauId,
                        principalTable: "DmNhaThau",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_KetQuaTrungThau_DuAnBuoc_BuocId",
                        column: x => x.BuocId,
                        principalTable: "DuAnBuoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KetQuaTrungThau_DuAn_DuAnId",
                        column: x => x.DuAnId,
                        principalTable: "DuAn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KetQuaTrungThau_GoiThau_GoiThauId",
                        column: x => x.GoiThauId,
                        principalTable: "GoiThau",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuyetDinhDuyetDuAnHangMuc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuyetDinhDuyetDuAnNguonVonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenHangMuc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QuyMoHangMuc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TongMucDauTu = table.Column<long>(type: "bigint", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuyetDinhDuyetDuAnHangMuc", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuyetDinhDuyetDuAnHangMuc_QuyetDinhDuyetDuAnNguonVon_QuyetDinhDuyetDuAnNguonVonId",
                        column: x => x.QuyetDinhDuyetDuAnNguonVonId,
                        principalTable: "QuyetDinhDuyetDuAnNguonVon",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NghiemThu",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuocId = table.Column<int>(type: "int", nullable: true),
                    HopDongId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SoBienBan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Dot = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ngay = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GiaTri = table.Column<long>(type: "bigint", nullable: false),
                    DuAnBuocId = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NghiemThu", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NghiemThu_DuAnBuoc_DuAnBuocId",
                        column: x => x.DuAnBuocId,
                        principalTable: "DuAnBuoc",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_NghiemThu_DuAn_DuAnId",
                        column: x => x.DuAnId,
                        principalTable: "DuAn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NghiemThu_HopDong_HopDongId",
                        column: x => x.HopDongId,
                        principalTable: "HopDong",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PhuLucHopDong",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuocId = table.Column<int>(type: "int", nullable: true),
                    Ten = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SoPhuLucHopDong = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ngay = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    HopDongId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GiaTri = table.Column<long>(type: "bigint", nullable: true),
                    NgayDuKienKetThuc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhuLucHopDong", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhuLucHopDong_DuAnBuoc_BuocId",
                        column: x => x.BuocId,
                        principalTable: "DuAnBuoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhuLucHopDong_DuAn_DuAnId",
                        column: x => x.DuAnId,
                        principalTable: "DuAn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhuLucHopDong_HopDong_HopDongId",
                        column: x => x.HopDongId,
                        principalTable: "HopDong",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TamUng",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuocId = table.Column<int>(type: "int", nullable: true),
                    HopDongId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SoPhieuChi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GiaTri = table.Column<long>(type: "bigint", nullable: true),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayTamUng = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    SoBaoLanh = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NgayBaoLanh = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    NgayKetThucBaoLanh = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TamUng", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TamUng_DuAnBuoc_BuocId",
                        column: x => x.BuocId,
                        principalTable: "DuAnBuoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TamUng_DuAn_DuAnId",
                        column: x => x.DuAnId,
                        principalTable: "DuAn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TamUng_HopDong_HopDongId",
                        column: x => x.HopDongId,
                        principalTable: "HopDong",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ThanhToan",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuocId = table.Column<int>(type: "int", nullable: true),
                    NghiemThuId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SoHoaDon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayHoaDon = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    GiaTri = table.Column<long>(type: "bigint", nullable: true),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThanhToan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThanhToan_DuAnBuoc_BuocId",
                        column: x => x.BuocId,
                        principalTable: "DuAnBuoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ThanhToan_DuAn_DuAnId",
                        column: x => x.DuAnId,
                        principalTable: "DuAn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ThanhToan_NghiemThu_NghiemThuId",
                        column: x => x.NghiemThuId,
                        principalTable: "NghiemThu",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NghiemThuPhuLucHopDong",
                columns: table => new
                {
                    NghiemThuId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PhuLucHopDongId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NghiemThuPhuLucHopDong", x => new { x.NghiemThuId, x.PhuLucHopDongId });
                    table.ForeignKey(
                        name: "FK_NghiemThuPhuLucHopDong_NghiemThu_NghiemThuId",
                        column: x => x.NghiemThuId,
                        principalTable: "NghiemThu",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NghiemThuPhuLucHopDong_PhuLucHopDong_PhuLucHopDongId",
                        column: x => x.PhuLucHopDongId,
                        principalTable: "PhuLucHopDong",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "DmLoaiDuAnTheoNam",
                columns: new[] { "Id", "CreatedBy", "IsDeleted", "Ma", "MoTa", "Stt", "Ten", "UpdatedAt", "UpdatedBy", "Used" },
                values: new object[,]
                {
                    { 1, "", false, "CBDT", null, null, "Chuẩn bị đầu tư", null, "", false },
                    { 2, "", false, "CT", null, null, "Chuyển tiếp", null, "", false },
                    { 3, "", false, "KCM", null, null, "Khởi công mới", null, "", false },
                    { 4, "", false, "KLTD", null, null, "Khối lượng tồn đọng", null, "", false }
                });

            migrationBuilder.InsertData(
                table: "DmTrangThaiDuAn",
                columns: new[] { "Id", "CreatedBy", "IsDeleted", "Ma", "MoTa", "Stt", "Ten", "UpdatedAt", "UpdatedBy", "Used" },
                values: new object[,]
                {
                    { 1, "", false, "DTH", null, null, "Đang thực hiện", null, "", false },
                    { 2, "", false, "PDDT", null, null, "Đã phê duyệt đầu tư", null, "", false },
                    { 3, "", false, "HT", null, null, "Đã hoàn thành", null, "", false },
                    { 4, "", false, "TD", null, null, "Tạm dừng", null, "", false }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BaoCao_BuocId",
                table: "BaoCao",
                column: "BuocId");

            migrationBuilder.CreateIndex(
                name: "IX_BaoCao_DuAnId",
                table: "BaoCao",
                column: "DuAnId");

            migrationBuilder.CreateIndex(
                name: "IX_BaoCao_Index",
                table: "BaoCao",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_BaoCaoBanGiaoSanPham_DonViBanGiaoId",
                table: "BaoCaoBanGiaoSanPham",
                column: "DonViBanGiaoId");

            migrationBuilder.CreateIndex(
                name: "IX_BaoCaoKhoKhanVuongMac_MucDoKhoKhanId",
                table: "BaoCaoKhoKhanVuongMac",
                column: "MucDoKhoKhanId");

            migrationBuilder.CreateIndex(
                name: "IX_BaoCaoKhoKhanVuongMac_TinhTrangId",
                table: "BaoCaoKhoKhanVuongMac",
                column: "TinhTrangId");

            migrationBuilder.CreateIndex(
                name: "IX_CANBO_DONVI_CanBoID",
                table: "CANBO_DONVI",
                column: "CanBoID");

            migrationBuilder.CreateIndex(
                name: "IX_CANBO_DONVI_DonViID",
                table: "CANBO_DONVI",
                column: "DonViID");

            migrationBuilder.CreateIndex(
                name: "IX_DangTaiKeHoachLcntLenMang_BuocId",
                table: "DangTaiKeHoachLcntLenMang",
                column: "BuocId");

            migrationBuilder.CreateIndex(
                name: "IX_DangTaiKeHoachLcntLenMang_Index",
                table: "DangTaiKeHoachLcntLenMang",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_DangTaiKeHoachLcntLenMang_KeHoachLuaChonNhaThauId",
                table: "DangTaiKeHoachLcntLenMang",
                column: "KeHoachLuaChonNhaThauId",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IDX_DM_DONVI_01",
                table: "DM_DONVI",
                column: "DonViCapChaID");

            migrationBuilder.CreateIndex(
                name: "IX_DmBuoc_GiaiDoanId",
                table: "DmBuoc",
                column: "GiaiDoanId");

            migrationBuilder.CreateIndex(
                name: "IX_DmBuoc_Index",
                table: "DmBuoc",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_DmBuoc_Ma",
                table: "DmBuoc",
                column: "Ma",
                unique: true,
                filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_DmBuoc_QuyTrinhId",
                table: "DmBuoc",
                column: "QuyTrinhId");

            migrationBuilder.CreateIndex(
                name: "IX_DmBuocManHinh_ManHinhId",
                table: "DmBuocManHinh",
                column: "ManHinhId");

            migrationBuilder.CreateIndex(
                name: "IX_DmBuocTrangThaiTienDo_BuocId",
                table: "DmBuocTrangThaiTienDo",
                column: "BuocId");

            migrationBuilder.CreateIndex(
                name: "IX_DmBuocTrangThaiTienDo_Index",
                table: "DmBuocTrangThaiTienDo",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_DmBuocTrangThaiTienDo_Ma",
                table: "DmBuocTrangThaiTienDo",
                column: "Ma",
                unique: true,
                filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_DmBuocTrangThaiTienDo_TrangThaiId",
                table: "DmBuocTrangThaiTienDo",
                column: "TrangThaiId");

            migrationBuilder.CreateIndex(
                name: "IX_DmChucVu_Index",
                table: "DmChucVu",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_DmChucVu_Ma",
                table: "DmChucVu",
                column: "Ma",
                unique: true,
                filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_DmChuDauTu_Index",
                table: "DmChuDauTu",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_DmChuDauTu_Ma",
                table: "DmChuDauTu",
                column: "Ma",
                unique: true,
                filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_DmGiaiDoan_Index",
                table: "DmGiaiDoan",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_DmGiaiDoan_Ma",
                table: "DmGiaiDoan",
                column: "Ma",
                unique: true,
                filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_DmHinhThucDauTu_Index",
                table: "DmHinhThucDauTu",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_DmHinhThucDauTu_Ma",
                table: "DmHinhThucDauTu",
                column: "Ma",
                unique: true,
                filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_DmHinhThucLuaChonNhaThau_Index",
                table: "DmHinhThucLuaChonNhaThau",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_DmHinhThucLuaChonNhaThau_Ma",
                table: "DmHinhThucLuaChonNhaThau",
                column: "Ma",
                unique: true,
                filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_DmHinhThucQuanLy_Index",
                table: "DmHinhThucQuanLy",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_DmHinhThucQuanLy_Ma",
                table: "DmHinhThucQuanLy",
                column: "Ma",
                unique: true,
                filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_DmLinhVuc_Index",
                table: "DmLinhVuc",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_DmLinhVuc_Ma",
                table: "DmLinhVuc",
                column: "Ma",
                unique: true,
                filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_DmLoaiDuAn_Index",
                table: "DmLoaiDuAn",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_DmLoaiDuAn_Ma",
                table: "DmLoaiDuAn",
                column: "Ma",
                unique: true,
                filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_DmLoaiDuAnTheoNam_Index",
                table: "DmLoaiDuAnTheoNam",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_DmLoaiDuAnTheoNam_Ma",
                table: "DmLoaiDuAnTheoNam",
                column: "Ma",
                unique: true,
                filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_DmLoaiGoiThau_Index",
                table: "DmLoaiGoiThau",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_DmLoaiGoiThau_Ma",
                table: "DmLoaiGoiThau",
                column: "Ma",
                unique: true,
                filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_DmLoaiHopDong_Index",
                table: "DmLoaiHopDong",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_DmLoaiHopDong_Ma",
                table: "DmLoaiHopDong",
                column: "Ma",
                unique: true,
                filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_DmLoaiVanBan_Index",
                table: "DmLoaiVanBan",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_DmLoaiVanBan_Ma",
                table: "DmLoaiVanBan",
                column: "Ma",
                unique: true,
                filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_DmMucDoKhoKhan_Index",
                table: "DmMucDoKhoKhan",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_DmMucDoKhoKhan_Ma",
                table: "DmMucDoKhoKhan",
                column: "Ma",
                unique: true,
                filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_DmNguonVon_Index",
                table: "DmNguonVon",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_DmNguonVon_Ma",
                table: "DmNguonVon",
                column: "Ma",
                unique: true,
                filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_DmNhaThau_Index",
                table: "DmNhaThau",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_DmNhaThau_Ma",
                table: "DmNhaThau",
                column: "Ma",
                unique: true,
                filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_DmNhomDuAn_Index",
                table: "DmNhomDuAn",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_DmNhomDuAn_Ma",
                table: "DmNhomDuAn",
                column: "Ma",
                unique: true,
                filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_DmPhuongThucLuaChonNhaThau_Index",
                table: "DmPhuongThucLuaChonNhaThau",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_DmPhuongThucLuaChonNhaThau_Ma",
                table: "DmPhuongThucLuaChonNhaThau",
                column: "Ma",
                unique: true,
                filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_DmQuyTrinh_Index",
                table: "DmQuyTrinh",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_DmQuyTrinh_Ma",
                table: "DmQuyTrinh",
                column: "Ma",
                unique: true,
                filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_DmQuyTrinh_MacDinh",
                table: "DmQuyTrinh",
                column: "MacDinh",
                unique: true,
                filter: "[MacDinh] = 1 AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_DmTinhTrangKhoKhan_Index",
                table: "DmTinhTrangKhoKhan",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_DmTinhTrangKhoKhan_Ma",
                table: "DmTinhTrangKhoKhan",
                column: "Ma",
                unique: true,
                filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_DmTinhTrangThucHienLcnt_Index",
                table: "DmTinhTrangThucHienLcnt",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_DmTinhTrangThucHienLcnt_Ma",
                table: "DmTinhTrangThucHienLcnt",
                column: "Ma",
                unique: true,
                filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_DmTrangThaiDuAn_Index",
                table: "DmTrangThaiDuAn",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_DmTrangThaiDuAn_Ma",
                table: "DmTrangThaiDuAn",
                column: "Ma",
                unique: true,
                filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_DmTrangThaiTienDo_Index",
                table: "DmTrangThaiTienDo",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_DmTrangThaiTienDo_Ma",
                table: "DmTrangThaiTienDo",
                column: "Ma",
                unique: true,
                filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_DuAn_BuocHienTaiId",
                table: "DuAn",
                column: "BuocHienTaiId",
                unique: true,
                filter: "[BuocHienTaiId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DuAn_ChuDauTuId",
                table: "DuAn",
                column: "ChuDauTuId");

            migrationBuilder.CreateIndex(
                name: "IX_DuAn_DuToanHienTaiId",
                table: "DuAn",
                column: "DuToanHienTaiId",
                unique: true,
                filter: "[DuToanHienTaiId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DuAn_GiaiDoanHienTaiId",
                table: "DuAn",
                column: "GiaiDoanHienTaiId");

            migrationBuilder.CreateIndex(
                name: "IX_DuAn_HinhThucDauTuId",
                table: "DuAn",
                column: "HinhThucDauTuId");

            migrationBuilder.CreateIndex(
                name: "IX_DuAn_HinhThucQuanLyDuAnId",
                table: "DuAn",
                column: "HinhThucQuanLyDuAnId");

            migrationBuilder.CreateIndex(
                name: "IX_DuAn_Index",
                table: "DuAn",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_DuAn_LinhVucId",
                table: "DuAn",
                column: "LinhVucId");

            migrationBuilder.CreateIndex(
                name: "IX_DuAn_LoaiDuAnId",
                table: "DuAn",
                column: "LoaiDuAnId");

            migrationBuilder.CreateIndex(
                name: "IX_DuAn_LoaiDuAnTheoNamId",
                table: "DuAn",
                column: "LoaiDuAnTheoNamId");

            migrationBuilder.CreateIndex(
                name: "IX_DuAn_NhomDuAnId",
                table: "DuAn",
                column: "NhomDuAnId");

            migrationBuilder.CreateIndex(
                name: "IX_DuAn_QuyTrinhId",
                table: "DuAn",
                column: "QuyTrinhId");

            migrationBuilder.CreateIndex(
                name: "IX_DuAn_TrangThaiDuAnId",
                table: "DuAn",
                column: "TrangThaiDuAnId");

            migrationBuilder.CreateIndex(
                name: "IX_DuAn_TrangThaiHienTaiId",
                table: "DuAn",
                column: "TrangThaiHienTaiId");

            migrationBuilder.CreateIndex(
                name: "IX_DuAnBuoc_BuocId",
                table: "DuAnBuoc",
                column: "BuocId");

            migrationBuilder.CreateIndex(
                name: "IX_DuAnBuoc_DuAnId",
                table: "DuAnBuoc",
                column: "DuAnId");

            migrationBuilder.CreateIndex(
                name: "IX_DuAnBuoc_Index",
                table: "DuAnBuoc",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_DuAnBuocManHinh_ManHinhId",
                table: "DuAnBuocManHinh",
                column: "ManHinhId");

            migrationBuilder.CreateIndex(
                name: "IX_DuAnNguonVon_NguonVonId",
                table: "DuAnNguonVon",
                column: "NguonVonId");

            migrationBuilder.CreateIndex(
                name: "IX_DuToan_DuAnId",
                table: "DuToan",
                column: "DuAnId");

            migrationBuilder.CreateIndex(
                name: "IX_DuToan_Index",
                table: "DuToan",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_E_LoaiVanBanQuyetDinh_Ma",
                table: "E_LoaiVanBanQuyetDinh",
                column: "Ma",
                unique: true,
                filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_E_ManHinh_Ma",
                table: "E_ManHinh",
                column: "Ma",
                unique: true,
                filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_E_ManHinh_Ten",
                table: "E_ManHinh",
                column: "Ten",
                unique: true,
                filter: "[Ten] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_GoiThau_DuAnBuocId",
                table: "GoiThau",
                column: "DuAnBuocId");

            migrationBuilder.CreateIndex(
                name: "IX_GoiThau_DuAnId",
                table: "GoiThau",
                column: "DuAnId");

            migrationBuilder.CreateIndex(
                name: "IX_GoiThau_HinhThucLuaChonNhaThauId",
                table: "GoiThau",
                column: "HinhThucLuaChonNhaThauId");

            migrationBuilder.CreateIndex(
                name: "IX_GoiThau_Index",
                table: "GoiThau",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_GoiThau_KeHoachLuaChonNhaThauId",
                table: "GoiThau",
                column: "KeHoachLuaChonNhaThauId");

            migrationBuilder.CreateIndex(
                name: "IX_GoiThau_LoaiHopDongId",
                table: "GoiThau",
                column: "LoaiHopDongId");

            migrationBuilder.CreateIndex(
                name: "IX_GoiThau_NguonVonId",
                table: "GoiThau",
                column: "NguonVonId");

            migrationBuilder.CreateIndex(
                name: "IX_GoiThau_PhuongThucLuaChonNhaThauId",
                table: "GoiThau",
                column: "PhuongThucLuaChonNhaThauId");

            migrationBuilder.CreateIndex(
                name: "IX_HopDong_BuocId",
                table: "HopDong",
                column: "BuocId");

            migrationBuilder.CreateIndex(
                name: "IX_HopDong_DonViThucHienId",
                table: "HopDong",
                column: "DonViThucHienId");

            migrationBuilder.CreateIndex(
                name: "IX_HopDong_DuAnId",
                table: "HopDong",
                column: "DuAnId");

            migrationBuilder.CreateIndex(
                name: "IX_HopDong_GoiThauId",
                table: "HopDong",
                column: "GoiThauId",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_HopDong_Index",
                table: "HopDong",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_HopDong_LoaiHopDongId",
                table: "HopDong",
                column: "LoaiHopDongId");

            migrationBuilder.CreateIndex(
                name: "IX_KeHoachVon_DuAnId",
                table: "KeHoachVon",
                column: "DuAnId");

            migrationBuilder.CreateIndex(
                name: "IX_KeHoachVon_Index",
                table: "KeHoachVon",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_KetQuaTrungThau_BuocId",
                table: "KetQuaTrungThau",
                column: "BuocId");

            migrationBuilder.CreateIndex(
                name: "IX_KetQuaTrungThau_DonViTrungThauId",
                table: "KetQuaTrungThau",
                column: "DonViTrungThauId");

            migrationBuilder.CreateIndex(
                name: "IX_KetQuaTrungThau_DuAnId",
                table: "KetQuaTrungThau",
                column: "DuAnId");

            migrationBuilder.CreateIndex(
                name: "IX_KetQuaTrungThau_GoiThauId",
                table: "KetQuaTrungThau",
                column: "GoiThauId",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_KetQuaTrungThau_Index",
                table: "KetQuaTrungThau",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_KetQuaTrungThau_LoaiGoiThauId",
                table: "KetQuaTrungThau",
                column: "LoaiGoiThauId");

            migrationBuilder.CreateIndex(
                name: "IX_NghiemThu_DuAnBuocId",
                table: "NghiemThu",
                column: "DuAnBuocId");

            migrationBuilder.CreateIndex(
                name: "IX_NghiemThu_DuAnId",
                table: "NghiemThu",
                column: "DuAnId");

            migrationBuilder.CreateIndex(
                name: "IX_NghiemThu_HopDongId",
                table: "NghiemThu",
                column: "HopDongId");

            migrationBuilder.CreateIndex(
                name: "IX_NghiemThu_Index",
                table: "NghiemThu",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_NghiemThuPhuLucHopDong_PhuLucHopDongId",
                table: "NghiemThuPhuLucHopDong",
                column: "PhuLucHopDongId");

            migrationBuilder.CreateIndex(
                name: "IX_NhaThauNguoiDung_Index",
                table: "NhaThauNguoiDung",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_NhaThauNguoiDung_NhaThauId_NguoiDungId",
                table: "NhaThauNguoiDung",
                columns: new[] { "NhaThauId", "NguoiDungId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PheDuyetDuToan_ChucVuId",
                table: "PheDuyetDuToan",
                column: "ChucVuId");

            migrationBuilder.CreateIndex(
                name: "IX_PhuLucHopDong_BuocId",
                table: "PhuLucHopDong",
                column: "BuocId");

            migrationBuilder.CreateIndex(
                name: "IX_PhuLucHopDong_DuAnId",
                table: "PhuLucHopDong",
                column: "DuAnId");

            migrationBuilder.CreateIndex(
                name: "IX_PhuLucHopDong_HopDongId",
                table: "PhuLucHopDong",
                column: "HopDongId");

            migrationBuilder.CreateIndex(
                name: "IX_PhuLucHopDong_Index",
                table: "PhuLucHopDong",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_QuyetDinhDuyetDuAnHangMuc_Index",
                table: "QuyetDinhDuyetDuAnHangMuc",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_QuyetDinhDuyetDuAnHangMuc_QuyetDinhDuyetDuAnNguonVonId",
                table: "QuyetDinhDuyetDuAnHangMuc",
                column: "QuyetDinhDuyetDuAnNguonVonId");

            migrationBuilder.CreateIndex(
                name: "IX_QuyetDinhDuyetDuAnNguonVon_Index",
                table: "QuyetDinhDuyetDuAnNguonVon",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_QuyetDinhDuyetDuAnNguonVon_QuyetDinhDuyetDuAnId",
                table: "QuyetDinhDuyetDuAnNguonVon",
                column: "QuyetDinhDuyetDuAnId");

            migrationBuilder.CreateIndex(
                name: "IX_QuyetDinhDuyetKHLCNT_KeHoachLuaChonNhaThauId",
                table: "QuyetDinhDuyetKHLCNT",
                column: "KeHoachLuaChonNhaThauId",
                unique: true,
                filter: "[KeHoachLuaChonNhaThauId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TamUng_BuocId",
                table: "TamUng",
                column: "BuocId");

            migrationBuilder.CreateIndex(
                name: "IX_TamUng_DuAnId",
                table: "TamUng",
                column: "DuAnId");

            migrationBuilder.CreateIndex(
                name: "IX_TamUng_HopDongId",
                table: "TamUng",
                column: "HopDongId",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_TamUng_Index",
                table: "TamUng",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_TepDinhKem_Index",
                table: "TepDinhKem",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_ThanhToan_BuocId",
                table: "ThanhToan",
                column: "BuocId");

            migrationBuilder.CreateIndex(
                name: "IX_ThanhToan_DuAnId",
                table: "ThanhToan",
                column: "DuAnId");

            migrationBuilder.CreateIndex(
                name: "IX_ThanhToan_Index",
                table: "ThanhToan",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_ThanhToan_NghiemThuId",
                table: "ThanhToan",
                column: "NghiemThuId",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ThanhVienBanQLDA_Index",
                table: "ThanhVienBanQLDA",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_ThanhVienBanQLDA_QuyetDinhId",
                table: "ThanhVienBanQLDA",
                column: "QuyetDinhId");

            migrationBuilder.CreateIndex(
                name: "IDX_USER_MASTER_01",
                table: "USER_MASTER",
                columns: new[] { "DonViID", "User_PortalID", "Used" });

            migrationBuilder.CreateIndex(
                name: "IDX_USER_MASTER_02",
                table: "USER_MASTER",
                column: "User_PortalID");

            migrationBuilder.CreateIndex(
                name: "IDX_USER_MASTER_03",
                table: "USER_MASTER",
                columns: new[] { "PhongBanID", "DonViID", "User_PortalID" });

            migrationBuilder.CreateIndex(
                name: "IDX_USER_MASTER_04",
                table: "USER_MASTER",
                columns: new[] { "DonViID", "User_PortalID" });

            migrationBuilder.CreateIndex(
                name: "IDX_USER_MASTER_05",
                table: "USER_MASTER",
                column: "User_PortalID");

            migrationBuilder.CreateIndex(
                name: "IDX_USER_MASTER_06",
                table: "USER_MASTER",
                column: "DonViID");

            migrationBuilder.CreateIndex(
                name: "IDX_USER_MASTER_07",
                table: "USER_MASTER",
                columns: new[] { "DonViID", "User_PortalID" });

            migrationBuilder.CreateIndex(
                name: "IX_USER_MASTER_122_121",
                table: "USER_MASTER",
                column: "Used");

            migrationBuilder.CreateIndex(
                name: "IX_USER_MASTER_CanBoID_Used",
                table: "USER_MASTER",
                columns: new[] { "CanBoID", "Used" });

            migrationBuilder.CreateIndex(
                name: "IX_UserSession_Platform",
                table: "UserSession",
                column: "Platform");

            migrationBuilder.CreateIndex(
                name: "IX_UserSession_UserName",
                table: "UserSession",
                column: "UserName");

            migrationBuilder.CreateIndex(
                name: "IX_UserSession_UserName_Platform",
                table: "UserSession",
                columns: new[] { "UserName", "Platform" });

            migrationBuilder.CreateIndex(
                name: "IX_VanBanChuTruong_ChucVuId",
                table: "VanBanChuTruong",
                column: "ChucVuId");

            migrationBuilder.CreateIndex(
                name: "IX_VanBanChuTruong_LoaiVanBanId",
                table: "VanBanChuTruong",
                column: "LoaiVanBanId");

            migrationBuilder.CreateIndex(
                name: "IX_VanBanPhapLy_ChucVuId",
                table: "VanBanPhapLy",
                column: "ChucVuId");

            migrationBuilder.CreateIndex(
                name: "IX_VanBanPhapLy_ChuDauTuId",
                table: "VanBanPhapLy",
                column: "ChuDauTuId");

            migrationBuilder.CreateIndex(
                name: "IX_VanBanPhapLy_LoaiVanBanId",
                table: "VanBanPhapLy",
                column: "LoaiVanBanId");

            migrationBuilder.CreateIndex(
                name: "IX_VanBanQuyetDinh_BuocId",
                table: "VanBanQuyetDinh",
                column: "BuocId");

            migrationBuilder.CreateIndex(
                name: "IX_VanBanQuyetDinh_DuAnId",
                table: "VanBanQuyetDinh",
                column: "DuAnId");

            migrationBuilder.CreateIndex(
                name: "IX_VanBanQuyetDinh_Index",
                table: "VanBanQuyetDinh",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.AddForeignKey(
                name: "FK_BaoCao_DuAnBuoc_BuocId",
                table: "BaoCao",
                column: "BuocId",
                principalTable: "DuAnBuoc",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BaoCao_DuAn_DuAnId",
                table: "BaoCao",
                column: "DuAnId",
                principalTable: "DuAn",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DangTaiKeHoachLcntLenMang_DuAnBuoc_BuocId",
                table: "DangTaiKeHoachLcntLenMang",
                column: "BuocId",
                principalTable: "DuAnBuoc",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DangTaiKeHoachLcntLenMang_KeHoachLuaChonNhaThau_KeHoachLuaChonNhaThauId",
                table: "DangTaiKeHoachLcntLenMang",
                column: "KeHoachLuaChonNhaThauId",
                principalTable: "KeHoachLuaChonNhaThau",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DuAn_DuAnBuoc_BuocHienTaiId",
                table: "DuAn",
                column: "BuocHienTaiId",
                principalTable: "DuAnBuoc",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DuAn_DuToan_DuToanHienTaiId",
                table: "DuAn",
                column: "DuToanHienTaiId",
                principalTable: "DuToan",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DuAn_DuAnBuoc_BuocHienTaiId",
                table: "DuAn");

            migrationBuilder.DropForeignKey(
                name: "FK_DuToan_DuAn_DuAnId",
                table: "DuToan");

            migrationBuilder.DropTable(
                name: "BaoCaoBanGiaoSanPham");

            migrationBuilder.DropTable(
                name: "BaoCaoBaoHanhSanPham");

            migrationBuilder.DropTable(
                name: "BaoCaoKhoKhanVuongMac");

            migrationBuilder.DropTable(
                name: "BaoCaoTienDo");

            migrationBuilder.DropTable(
                name: "CANBO_DONVI");

            migrationBuilder.DropTable(
                name: "DangTaiKeHoachLcntLenMang");

            migrationBuilder.DropTable(
                name: "DM_DUONG");

            migrationBuilder.DropTable(
                name: "DM_PHUONGXA");

            migrationBuilder.DropTable(
                name: "DM_QUANHUYEN");

            migrationBuilder.DropTable(
                name: "DM_TINHTHANH");

            migrationBuilder.DropTable(
                name: "DmBuocManHinh");

            migrationBuilder.DropTable(
                name: "DmBuocTrangThaiTienDo");

            migrationBuilder.DropTable(
                name: "DmTinhTrangThucHienLcnt");

            migrationBuilder.DropTable(
                name: "DuAnBuocManHinh");

            migrationBuilder.DropTable(
                name: "DuAnChiuTrachNhiemXuLy");

            migrationBuilder.DropTable(
                name: "DuAnCongViec");

            migrationBuilder.DropTable(
                name: "DuAnNguonVon");

            migrationBuilder.DropTable(
                name: "DUONG_PHUONG_QUAN");

            migrationBuilder.DropTable(
                name: "E_CAPDONVI");

            migrationBuilder.DropTable(
                name: "E_LoaiVanBanQuyetDinh");

            migrationBuilder.DropTable(
                name: "E_VAITROCHUCVU");

            migrationBuilder.DropTable(
                name: "KeHoachVon");

            migrationBuilder.DropTable(
                name: "KetQuaTrungThau");

            migrationBuilder.DropTable(
                name: "NghiemThuPhuLucHopDong");

            migrationBuilder.DropTable(
                name: "NhaThauNguoiDung");

            migrationBuilder.DropTable(
                name: "PheDuyetDuToan");

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
                name: "USER_MASTER");

            migrationBuilder.DropTable(
                name: "UserSession");

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
                name: "CANBO");

            migrationBuilder.DropTable(
                name: "DM_DONVI");

            migrationBuilder.DropTable(
                name: "E_ManHinh");

            migrationBuilder.DropTable(
                name: "DmLoaiGoiThau");

            migrationBuilder.DropTable(
                name: "PhuLucHopDong");

            migrationBuilder.DropTable(
                name: "QuyetDinhDuyetDuAnNguonVon");

            migrationBuilder.DropTable(
                name: "NghiemThu");

            migrationBuilder.DropTable(
                name: "QuyetDinhLapBanQLDA");

            migrationBuilder.DropTable(
                name: "DmChucVu");

            migrationBuilder.DropTable(
                name: "DmLoaiVanBan");

            migrationBuilder.DropTable(
                name: "QuyetDinhDuyetDuAn");

            migrationBuilder.DropTable(
                name: "HopDong");

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

            migrationBuilder.DropTable(
                name: "DuToan");
        }
    }
}
