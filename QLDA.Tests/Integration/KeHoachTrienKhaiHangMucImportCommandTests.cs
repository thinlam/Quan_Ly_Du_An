using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QLDA.Application.KeHoachTrienKhaiHangMucs.Commands;
using QLDA.Application.KeHoachTrienKhaiHangMucs.DTOs;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;
using QLDA.Persistence;
using QLDA.Tests.Fixtures;
using Xunit;

namespace QLDA.Tests.Integration;

[Collection("WebApi")]
public class KeHoachTrienKhaiHangMucImportCommandTests(WebApiFixture fixture) {
    [Fact]
    public async Task Import_StoresCanBoIdsAsUserPortalId_NotUserMasterPk() {
        await using var scope = fixture.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await EnsureLegacyUserTablesAsync(db);

        const long chuTriMasterId = 10_001;
        const long chuTriPortalId = 50_001;
        const long phoiHopMasterId = 10_002;
        const long phoiHopPortalId = 60_002;
        const string chuTriHoTen = "Đào Thị Bích Tuyền";
        const string phoiHopHoTen = "Đặng Trung Nghĩa";
        const string tenDonVi = "Phòng Import KH";
        const string tenGiaiDoan = "Giai đoạn import test";

        var hasTrangThai = await db.Set<DanhMucTrangThaiPheDuyet>()
            .AnyAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatMacDinh.DuThao
                           && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt);
        if (!hasTrangThai) {
            db.Set<DanhMucTrangThaiPheDuyet>().Add(new DanhMucTrangThaiPheDuyet {
                Ma = TrangThaiPheDuyetCodes.DeXuatMacDinh.DuThao,
                Ten = "Dự thảo",
                Loai = PheDuyetEntityNames.DeXuatMacDinhStt,
                Used = true,
            });
            await db.SaveChangesAsync();
        }

        var giaiDoan = new DanhMucGiaiDoan {
            Ma = "GD_KH_IMP",
            Ten = "Giai doan KH import",
            Used = true,
            Stt = 1,
        };
        db.Set<DanhMucGiaiDoan>().Add(giaiDoan);
        await db.SaveChangesAsync();

        var buoc = new DanhMucBuoc {
            Ma = "BC_KH_IMP",
            Ten = tenGiaiDoan,
            QuyTrinhId = 1,
            GiaiDoanId = giaiDoan.Id,
            ParentId = null,
            Path = "/1/",
            Level = 0,
            Stt = 1,
            Used = true,
        };
        db.Set<DanhMucBuoc>().Add(buoc);
        await db.SaveChangesAsync();

        var duAn = new DuAn {
            TenDuAn = "Dự án import KH hạng mục",
            MaDuAn = "KH_HM_IMP",
            LoaiDuAnId = 1,
            TrangThaiDuAnId = 1,
            CreatedAt = DateTimeOffset.UtcNow,
            IsDeleted = false,
            Level = 0,
            Path = "",
            NgayBatDau = DateTime.UtcNow,
            LanhDaoPhuTrachId = 1,
        };
        db.Set<DuAn>().Add(duAn);
        await db.SaveChangesAsync();

        db.Set<DuAnBuoc>().Add(new DuAnBuoc {
            BuocId = buoc.Id,
            DuAnId = duAn.Id,
            TenBuoc = tenGiaiDoan,
            Used = true,
        });
        await db.SaveChangesAsync();

        await db.Database.ExecuteSqlRawAsync(
            "INSERT INTO DM_DONVI (DonViID, TenDonVi, DonViCapChaID, Used) VALUES ({0}, {1}, {2}, 1)",
            9_001,
            tenDonVi,
            1L);
        await db.Database.ExecuteSqlRawAsync(
            "INSERT INTO USER_MASTER (User_MasterID, User_PortalID, HoTen, DonViID, LaDonViChinh, Used) VALUES ({0}, {1}, {2}, 1, 1, 1)",
            chuTriMasterId,
            chuTriPortalId,
            chuTriHoTen);
        await db.Database.ExecuteSqlRawAsync(
            "INSERT INTO USER_MASTER (User_MasterID, User_PortalID, HoTen, DonViID, LaDonViChinh, Used) VALUES ({0}, {1}, {2}, 1, 1, 1)",
            phoiHopMasterId,
            phoiHopPortalId,
            phoiHopHoTen);

        var result = await mediator.Send(new KeHoachTrienKhaiHangMucImportRangeCommand([
            new KeHoachTrienKhaiHangMucImportDto {
                TenDuAn = duAn.TenDuAn,
                TenHangMuc = "Hạng mục import test",
                TenGiaiDoan = tenGiaiDoan,
                TenDonViChuTri = tenDonVi,
                TenCanBoChuTri = chuTriHoTen,
                TenCanBoPhoiHop = phoiHopHoTen,
                ExcelRowNumber = 7,
            },
        ], Guid.Empty, 0));

        result.SuccessCount.Should().Be(1);
        result.ErrorCount.Should().Be(0);
        result.Id.Should().NotBeNull();

        var hangMuc = (await db.Set<HangMucKeHoach>()
            .Where(h => h.KeHoachId == result.Id)
            .ToListAsync())
            .Single();

        hangMuc.CanBoChuTriId.Should().Be(chuTriPortalId);
        hangMuc.CanBoChuTriId.Should().NotBe(chuTriMasterId);
        hangMuc.CanBoPhoiHopIds.Should().Equal([phoiHopPortalId]);
        hangMuc.CanBoPhoiHopIds.Should().NotContain(phoiHopMasterId);
    }

    private static async Task EnsureLegacyUserTablesAsync(AppDbContext db) {
        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS DM_DONVI (
                DonViID INTEGER NOT NULL PRIMARY KEY,
                TenDonVi TEXT,
                DonViCapChaID INTEGER,
                Used INTEGER
            );
            CREATE TABLE IF NOT EXISTS USER_MASTER (
                User_MasterID INTEGER NOT NULL PRIMARY KEY,
                User_PortalID INTEGER,
                HoTen TEXT,
                DonViID INTEGER,
                LaDonViChinh INTEGER,
                Used INTEGER
            );
            """);
    }
}
