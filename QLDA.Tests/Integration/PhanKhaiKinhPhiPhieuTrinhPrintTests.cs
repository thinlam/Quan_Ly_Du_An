using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.Application.Common.DTOs;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using QLDA.Domain.Entities;
using QLDA.Persistence;
using QLDA.Tests.Fixtures;
using Xunit;

namespace QLDA.Tests.Integration;

[Collection("WebApi")]
public class PhanKhaiKinhPhiPhieuTrinhPrintTests(WebApiFixture fixture) {
    /// <summary>CB.PKH-TC = QLDA_ChuyenVien; PhongBan 219 = KH-TC bypass FilterVisible.</summary>
    private HttpClient PrintClient => fixture.CreateChuyenVienClient(phongBanId: 219);

    [Fact]
    public async Task InPhieuTrinh_ExistingId_ReturnsDocx() {
        var id = await CreatePhanKhaiKinhPhiAsync();

        var response = await PrintClient.GetAsync(
            $"/api/print/phieu-trinh-phan-khai-kinh-phi?id={id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType
            .Should().Be("application/vnd.openxmlformats-officedocument.wordprocessingml.document");

        var fileName = response.Content.Headers.ContentDisposition?.FileName
            ?? response.Content.Headers.ContentDisposition?.FileNameStar
            ?? string.Empty;
        fileName.Should().Contain("to-trinh-phan-khai-kinh-phi");

        var bytes = await response.Content.ReadAsByteArrayAsync();
        bytes.Length.Should().BeGreaterThan(1000);
        bytes[0].Should().Be((byte)'P');
        bytes[1].Should().Be((byte)'K');
    }

    [Fact]
    public async Task InPhieuTrinh_UnknownId_ReturnsBusinessFailure() {
        var fakeId = Guid.NewGuid();

        var response = await PrintClient.GetAsync(
            $"/api/print/phieu-trinh-phan-khai-kinh-phi?id={fakeId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ResultApi>();
        body.Should().NotBeNull();
        body!.Result.Should().BeFalse();
        body.ErrorMessage.Should().Contain("Không tìm thấy dữ liệu phân khai kinh phí");
    }

    private async Task<Guid> CreatePhanKhaiKinhPhiAsync() {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(fixture.GetSqliteConnection())
            .Options;
        await using var db = new SqliteAppDbContext(options);

        var entity = new PhanKhaiKinhPhi {
            DuAnId = fixture.SeededDuAnId,
            SoToTrinh = $"TT_{Guid.NewGuid():N}",
            NgayToTrinh = DateTimeOffset.UtcNow,
            NguonVonId = 1,
            KinhPhiDeXuat = 1_000_000,
            KinhPhiPhanKhai = 800_000,
            TrichYeu = "Test trích yếu tờ trình",
            ThuyetMinh = "Test thuyết minh",
            CreatedAt = DateTimeOffset.UtcNow,
            IsDeleted = false,
        };
        db.Set<PhanKhaiKinhPhi>().Add(entity);
        await db.SaveChangesAsync();
        return entity.Id;
    }
}
