using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BuildingBlocks.Application.Common.DTOs;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.DuAns.DTOs;
using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;
using QLDA.Persistence;
using QLDA.Tests.Fakers;
using QLDA.Tests.Fixtures;
using Xunit;

namespace QLDA.Tests.Integration;

[Collection("WebApi")]
public class DuAnControllerTests(WebApiFixture fixture)
{
    private const string QtATen = "Issue182 QT A";
    private const string QtBTen = "Issue182 QT B";
    private const string BuocATen = "Issue182 Buoc A1";
    private const string BuocBTen = "Issue182 Buoc B1";

    private HttpClient AuthedClient => fixture.CreateAuthenticatedClient();

    [Fact]
    public async Task GetChiTiet_ExistingId_ReturnsOk()
    {
        var response = await AuthedClient.GetAsync($"/api/du-an/{fixture.SeededDuAnId}/chi-tiet");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact]
    public async Task Create_ValidDto_ReturnsOk()
    {
        var dto = new DuAnInsertDtoFaker().Generate();

        var response = await AuthedClient.PostAsJsonAsync("/api/du-an/them-moi", dto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
        result!.DataResult.Should().NotBeNull();
    }

    [Fact]
    public async Task Update_ExistingDuAn_ReturnsOk()
    {
        var dto = new DuAnUpdateModelFaker(fixture.SeededDuAnId).Generate();

        var response = await AuthedClient.PutAsJsonAsync("/api/du-an/cap-nhat", dto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact]
    public async Task SoftDelete_ExistingDuAn_ReturnsOk()
    {
        // Create a new DuAn to delete
        var createDto = new DuAnInsertDtoFaker().Generate();
        var createResponse = await AuthedClient.PostAsJsonAsync("/api/du-an/them-moi", createDto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var createResult = await createResponse.Content.ReadFromJsonAsync<ResultApi>();
        createResult.Should().NotBeNull();

        var idToDelete = createResult!.DataResult switch
        {
            JsonElement el => el.GetProperty("id").GetGuid(),
            Guid g => g,
            _ => throw new InvalidOperationException($"Unexpected DataResult type: {createResult.DataResult?.GetType()}")
        };

        var response = await AuthedClient.DeleteAsync($"/api/du-an/{idToDelete}/xoa-tam");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact]
    public async Task Update_SameQuyTrinh_NoProgress_DoesNotClone()
    {
        var (qtAId, _, _) = await SeedQuyTrinhAsync();
        var duAnId = await CreateDuAnAsync(qtAId);
        var before = await SnapshotBuocsAsync(duAnId);
        before.Should().NotBeEmpty();

        await PutCapNhatAsync(duAnId, qtAId);

        var after = await SnapshotBuocsAsync(duAnId);
        after.Should().BeEquivalentTo(before);
    }

    [Fact]
    public async Task Update_SameQuyTrinh_WithProgress_KeepsProgress()
    {
        var (qtAId, _, _) = await SeedQuyTrinhAsync();
        var duAnId = await CreateDuAnAsync(qtAId);
        var ngayThucTe = DateTimeOffset.UtcNow;
        await SetFirstBuocProgressAsync(duAnId, ngayThucTe);
        var before = await SnapshotBuocsAsync(duAnId);

        await PutCapNhatAsync(duAnId, qtAId);

        var after = await SnapshotBuocsAsync(duAnId);
        after.Should().BeEquivalentTo(before);
        after.Should().Contain(b => b.NgayThucTeBatDau != null);
    }

    [Fact]
    public async Task Update_ChangeQuyTrinh_NoProgress_ClonesNewSteps()
    {
        var (qtAId, qtBId, buocBIds) = await SeedQuyTrinhAsync();
        var duAnId = await CreateDuAnAsync(qtAId);
        var before = await SnapshotBuocsAsync(duAnId);
        before.Should().NotBeEmpty();
        before.Select(b => b.BuocId).Intersect(buocBIds).Should().BeEmpty();

        await PutCapNhatAsync(duAnId, qtBId);

        var after = await SnapshotBuocsAsync(duAnId);
        after.Select(b => b.BuocId).Should().BeEquivalentTo(buocBIds);
    }

    [Fact]
    public async Task Update_ChangeQuyTrinh_WithProgress_DoesNotClone()
    {
        var (qtAId, qtBId, _) = await SeedQuyTrinhAsync();
        var duAnId = await CreateDuAnAsync(qtAId);
        var ngayThucTe = DateTimeOffset.UtcNow;
        await SetFirstBuocProgressAsync(duAnId, ngayThucTe);
        var before = await SnapshotBuocsAsync(duAnId);

        await PutCapNhatAsync(duAnId, qtBId);

        var after = await SnapshotBuocsAsync(duAnId);
        after.Should().BeEquivalentTo(before);
        after.Should().Contain(b => b.NgayThucTeBatDau != null);
    }

    [Fact]
    public async Task Update_ChangeGhiChuOnly_DoesNotTouchDuAnBuoc()
    {
        var (qtAId, _, _) = await SeedQuyTrinhAsync();
        var duAnId = await CreateDuAnAsync(qtAId);
        var before = await SnapshotBuocsAsync(duAnId);
        before.Should().NotBeEmpty();

        await PutCapNhatAsync(duAnId, qtAId, ghiChu: "Issue182 ghi chu");

        var after = await SnapshotBuocsAsync(duAnId);
        after.Should().BeEquivalentTo(before);
    }

    private async Task<(int QtAId, int QtBId, List<int> BuocBIds)> SeedQuyTrinhAsync()
    {
        await using var db = CreateDb();
        await EnsureTepDinhKemTableAsync(db);

        var qtA = await db.Set<DanhMucQuyTrinh>().FirstOrDefaultAsync(e => e.Ten == QtATen);
        if (qtA == null)
        {
            var hasMacDinh = await db.Set<DanhMucQuyTrinh>().AnyAsync(e => e.MacDinh);
            qtA = new DanhMucQuyTrinh
            {
                Ten = QtATen,
                MacDinh = !hasMacDinh,
                Used = true,
                IsDeleted = false,
                CreatedAt = DateTimeOffset.UtcNow,
            };
            db.Set<DanhMucQuyTrinh>().Add(qtA);
            await db.SaveChangesAsync();
        }

        var qtB = await db.Set<DanhMucQuyTrinh>().FirstOrDefaultAsync(e => e.Ten == QtBTen);
        if (qtB == null)
        {
            qtB = new DanhMucQuyTrinh
            {
                Ten = QtBTen,
                MacDinh = false,
                Used = true,
                IsDeleted = false,
                CreatedAt = DateTimeOffset.UtcNow,
            };
            db.Set<DanhMucQuyTrinh>().Add(qtB);
            await db.SaveChangesAsync();
        }

        if (!await db.Set<DanhMucBuoc>().AnyAsync(e => e.Ten == BuocATen))
        {
            db.Set<DanhMucBuoc>().Add(new DanhMucBuoc
            {
                Ten = BuocATen,
                QuyTrinhId = qtA.Id,
                Used = true,
                IsDeleted = false,
                Path = "/",
                Level = 0,
                Stt = 1,
                SoNgayThucHien = 1,
                CreatedAt = DateTimeOffset.UtcNow,
            });
        }

        if (!await db.Set<DanhMucBuoc>().AnyAsync(e => e.Ten == BuocBTen))
        {
            db.Set<DanhMucBuoc>().Add(new DanhMucBuoc
            {
                Ten = BuocBTen,
                QuyTrinhId = qtB.Id,
                Used = true,
                IsDeleted = false,
                Path = "/",
                Level = 0,
                Stt = 1,
                SoNgayThucHien = 1,
                CreatedAt = DateTimeOffset.UtcNow,
            });
        }

        await db.SaveChangesAsync();

        var buocBIds = await db.Set<DanhMucBuoc>()
            .Where(e => e.QuyTrinhId == qtB.Id && !e.IsDeleted)
            .Select(e => e.Id)
            .ToListAsync();

        return (qtA.Id, qtB.Id, buocBIds);
    }

    private async Task<Guid> CreateDuAnAsync(int quyTrinhId)
    {
        var dto = new DuAnInsertDtoFaker().Generate();
        dto.QuyTrinhId = quyTrinhId;
        dto.LanhDaoPhuTrachId = 1;

        var response = await AuthedClient.PostAsJsonAsync("/api/du-an/them-moi", dto);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using var db = CreateDb();
        var entity = await db.Set<DuAn>().AsNoTracking()
            .FirstOrDefaultAsync(e => e.MaDuAn == dto.MaDuAn && !e.IsDeleted);
        entity.Should().NotBeNull($"them-moi phải lưu DuAn MaDuAn={dto.MaDuAn}");
        return entity!.Id;
    }

    private async Task PutCapNhatAsync(Guid duAnId, int? quyTrinhId, string? ghiChu = null)
    {
        var dto = new DuAnUpdateModelFaker(duAnId).Generate();
        dto.QuyTrinhId = quyTrinhId;
        dto.LanhDaoPhuTrachId = 1;
        if (ghiChu != null)
            dto.GhiChu = ghiChu;

        var response = await AuthedClient.PutAsJsonAsync("/api/du-an/cap-nhat", dto);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using var db = CreateDb();
        var entity = await db.Set<DuAn>().AsNoTracking().FirstAsync(e => e.Id == duAnId);
        entity.QuyTrinhId.Should().Be(quyTrinhId);
        if (ghiChu != null)
            entity.GhiChu.Should().Be(ghiChu);
    }

    private async Task SetFirstBuocProgressAsync(Guid duAnId, DateTimeOffset ngayThucTeBatDau)
    {
        await using var db = CreateDb();
        var buoc = await db.Set<DuAnBuoc>().FirstAsync(e => e.DuAnId == duAnId && !e.IsDeleted);
        buoc.NgayThucTeBatDau = ngayThucTeBatDau;
        await db.SaveChangesAsync();
    }

    private async Task<List<BuocSnapshot>> SnapshotBuocsAsync(Guid duAnId)
    {
        await using var db = CreateDb();
        return await db.Set<DuAnBuoc>()
            .AsNoTracking()
            .Where(e => e.DuAnId == duAnId && !e.IsDeleted)
            .OrderBy(e => e.Id)
            .Select(e => new BuocSnapshot(e.Id, e.BuocId, e.NgayThucTeBatDau, e.GhiChu, e.TenBuoc))
            .ToListAsync();
    }

    private static async Task EnsureTepDinhKemTableAsync(SqliteAppDbContext db)
    {
        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS TepDinhKem (
                Id TEXT NOT NULL PRIMARY KEY,
                ParentId TEXT,
                GroupId TEXT NOT NULL,
                GroupType TEXT NOT NULL,
                Type TEXT,
                FileName TEXT,
                OriginalName TEXT,
                Path TEXT,
                Size INTEGER NOT NULL DEFAULT 0,
                CreatedBy TEXT NOT NULL DEFAULT '',
                CreatedAt TEXT NOT NULL DEFAULT '',
                UpdatedBy TEXT NOT NULL DEFAULT '',
                UpdatedAt TEXT,
                IsDeleted INTEGER NOT NULL DEFAULT 0,
                "Index" INTEGER NOT NULL DEFAULT 0
            );
            """);
    }

    private SqliteAppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(fixture.GetSqliteConnection())
            .Options;
        return new SqliteAppDbContext(options);
    }

    private sealed record BuocSnapshot(
        int Id,
        int BuocId,
        DateTimeOffset? NgayThucTeBatDau,
        string? GhiChu,
        string? TenBuoc);
}
