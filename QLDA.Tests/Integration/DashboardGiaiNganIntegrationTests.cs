using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BuildingBlocks.Application.Common.DTOs;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QLDA.Domain.DTOs;
using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;
using QLDA.Persistence;
using QLDA.Tests.Fixtures;
using Xunit;

namespace QLDA.Tests.Integration;

[Collection("WebApi")]
public class DashboardGiaiNganIntegrationTests(WebApiFixture fixture)
{
    private HttpClient AuthedClient => fixture.CreateBgdClient();

    [Fact]
    public async Task GetGiaiNganTheoNguonVon_ReturnsOk_AndValidStructure()
    {
        var response = await AuthedClient.GetAsync("/api/thong-ke/giai-ngan-theo-nguon-von?nam=2026");
        var content = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK, because: content);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact]
    public async Task GetChiTietGiaiNgan_ReturnsOk_AndValidStructure()
    {
        var response = await AuthedClient.GetAsync("/api/thong-ke/chi-tiet-giai-ngan?nam=2026&nguonVonId=23");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact]
    public async Task GetChiTietGiaiNgan_And_GetGiaiNganTheoNguonVon_ConsistencyCheck()
    {
        // Setup mock data in DbContext to test zero-disbursement project
        using var scope = fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var testNguonVonId = 999;
        
        var duAn = await context.Set<DuAn>().FirstOrDefaultAsync(x => x.Id == fixture.SeededDuAnId);
        if (duAn != null)
        {
            duAn.LanhDaoPhuTrachId = 10;
            await context.SaveChangesAsync();
        }

        if (!await context.Set<DanhMucNguonVon>().AnyAsync(x => x.Id == testNguonVonId))
        {
            context.Set<DanhMucNguonVon>().Add(new DanhMucNguonVon
            {
                Id = testNguonVonId,
                Ma = "NV_TEST_999",
                Ten = "Nguồn Vốn Test 999",
                Used = true
            });
            await context.SaveChangesAsync();
        }

        // Thêm kế hoạch vốn năm 2026 cho SeededDuAnId nhưng KHÔNG tạo ThanhToan
        var keHoachVon = await context.Set<KeHoachVon>().FirstOrDefaultAsync(x => x.DuAnId == fixture.SeededDuAnId && x.Nam == 2026 && x.NguonVonId == testNguonVonId);
        if (keHoachVon == null)
        {
            context.Set<KeHoachVon>().Add(new KeHoachVon
            {
                Id = Guid.NewGuid(),
                DuAnId = fixture.SeededDuAnId,
                Nam = 2026,
                NguonVonId = testNguonVonId,
                SoVon = 5_000_000_000m // 5 tỷ VND = 5000 triệu VND
            });
            await context.SaveChangesAsync();
        }

        // 1. Gọi API Chi tiết
        var resChiTiet = await AuthedClient.GetAsync($"/api/thong-ke/chi-tiet-giai-ngan?nam=2026&nguonVonId={testNguonVonId}");
        var contentChiTiet = await resChiTiet.Content.ReadAsStringAsync();
        resChiTiet.StatusCode.Should().Be(HttpStatusCode.OK, because: contentChiTiet);
        var jsonDocChiTiet = JsonDocument.Parse(contentChiTiet);
        var dataResultChiTiet = jsonDocChiTiet.RootElement.GetProperty("dataResult");

        // Yêu cầu 2: Nguồn vốn có trong KeHoachVon nhưng chưa giải ngân thì vẫn phải ra kết quả
        dataResultChiTiet.GetArrayLength().Should().BeGreaterThanOrEqualTo(1);
        
        var matchingRow = dataResultChiTiet.EnumerateArray()
            .FirstOrDefault(x => x.GetProperty("tenDuAn").GetString() == "Test Dự án");
        
        matchingRow.ValueKind.Should().NotBe(JsonValueKind.Undefined);
        matchingRow.GetProperty("giaTriGiaiNgan").GetDecimal().Should().Be(0m);
        matchingRow.GetProperty("trangThaiGiaiNgan").GetBoolean().Should().BeFalse();

        // 2. Gọi API Danh sách
        var resDanhSach = await AuthedClient.GetAsync("/api/thong-ke/giai-ngan-theo-nguon-von?nam=2026");
        resDanhSach.StatusCode.Should().Be(HttpStatusCode.OK);
        var contentDanhSach = await resDanhSach.Content.ReadAsStringAsync();
        var jsonDocDanhSach = JsonDocument.Parse(contentDanhSach);
        var dataResultDanhSach = jsonDocDanhSach.RootElement.GetProperty("dataResult");

        var matchingGroup = dataResultDanhSach.EnumerateArray()
            .FirstOrDefault(x => x.GetProperty("nguonVonId").GetInt32() == testNguonVonId);

        // Yêu cầu 1: Cùng logic tính toán, số kế hoạch vốn khớp
        matchingGroup.ValueKind.Should().NotBe(JsonValueKind.Undefined);
        matchingGroup.GetProperty("tongKeHoachVon").GetDecimal().Should().Be(5000m);
        matchingGroup.GetProperty("giaTriGiaiNgan").GetDecimal().Should().Be(0m);
    }
}
