using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QLDA.Application.PhanKhaiKinhPhis;
using QLDA.Application.PhanKhaiKinhPhis.Commands;
using QLDA.Application.PhanKhaiKinhPhis.DTOs;
using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;
using QLDA.Persistence;
using QLDA.Tests.Fixtures;
using Xunit;

namespace QLDA.Tests.Integration;

[Collection("WebApi")]
public class PhanKhaiKinhPhiImportCommandTests(WebApiFixture fixture) {
    [Fact]
    public async Task Import_ResolvesNguonVonByDuAn_WhenDisplayNamesMatch() {
        await using var scope = fixture.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var nguonVon = new DanhMucNguonVon {
            Ten = "Ngân sách thành phố",
            Stt = 99,
            Used = true,
        };
        db.Set<DanhMucNguonVon>().Add(nguonVon);
        await db.SaveChangesAsync();

        var duAnA = new DuAn {
            TenDuAn = "Dự án Import A",
            MaDuAn = "IMP_DA_A",
            LoaiDuAnId = 1,
            TrangThaiDuAnId = 1,
            CreatedAt = DateTimeOffset.UtcNow,
            IsDeleted = false,
            Level = 0,
            Path = "",
            NgayBatDau = DateTime.UtcNow,
            LanhDaoPhuTrachId = 1,
        };
        var duAnB = new DuAn {
            TenDuAn = "Dự án Import B",
            MaDuAn = "IMP_DA_B",
            LoaiDuAnId = 1,
            TrangThaiDuAnId = 1,
            CreatedAt = DateTimeOffset.UtcNow,
            IsDeleted = false,
            Level = 0,
            Path = "",
            NgayBatDau = DateTime.UtcNow,
            LanhDaoPhuTrachId = 1,
        };
        db.Set<DuAn>().AddRange(duAnA, duAnB);
        await db.SaveChangesAsync();

        db.Set<DuAnNguonVon>().AddRange(
            new DuAnNguonVon { LeftId = duAnA.Id, RightId = nguonVon.Id },
            new DuAnNguonVon { LeftId = duAnB.Id, RightId = nguonVon.Id });
        await db.SaveChangesAsync();

        var validDisplay = PhanKhaiKinhPhiImportDisplay.Format(
            nguonVon.Ten!, duAnA.TenDuAn!);
        var mismatchDisplay = PhanKhaiKinhPhiImportDisplay.Format(
            nguonVon.Ten!, duAnB.TenDuAn!);
        var legacyDisplay = nguonVon.Ten!;

        var result = await mediator.Send(new PhanKhaiKinhPhiImportRangeCommand([
            new PhanKhaiKinhPhiImportDto {
                TenDuAn = duAnA.TenDuAn,
                TenNguonVon = validDisplay,
                KinhPhiDeXuat = 1000,
                ExcelRowNumber = 7,
            },
            new PhanKhaiKinhPhiImportDto {
                TenDuAn = duAnA.TenDuAn,
                TenNguonVon = mismatchDisplay,
                KinhPhiDeXuat = 2000,
                ExcelRowNumber = 8,
            },
            new PhanKhaiKinhPhiImportDto {
                TenDuAn = duAnA.TenDuAn,
                TenNguonVon = legacyDisplay,
                KinhPhiDeXuat = 3000,
                ExcelRowNumber = 9,
            },
        ]));

        result.SuccessCount.Should().Be(1);
        result.ErrorCount.Should().Be(2);
        result.Errors.Should().Contain(e => e.Contains("Nguồn vốn không thuộc dự án đã chọn"));
        result.Errors.Should().Contain(e => e.Contains("Không tìm thấy nguồn vốn"));

        var inserted = await db.Set<PhanKhaiKinhPhi>()
            .Where(e => e.DuAnId == duAnA.Id && e.KinhPhiDeXuat == 1000)
            .SingleAsync();

        inserted.NguonVonId.Should().Be(nguonVon.Id);
    }
}
