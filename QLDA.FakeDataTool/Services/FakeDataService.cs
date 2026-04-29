using Microsoft.EntityFrameworkCore;
using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;
using QLDA.Persistence;
using QLDA.FakeDataTool.Fakers;

namespace QLDA.FakeDataTool.Services;

/// <summary>
/// Service for seeding fake data with auto-seed FK chain logic.
/// Handles: DanhMuc → DuAn → GoiThau → HopDong dependency chain.
/// </summary>
public class FakeDataService
{
    private readonly AppDbContext _db;

    public FakeDataService(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Seed DuAn entities with auto-seed DanhMucLoaiDuAn.
    /// </summary>
    public async Task<int> SeedDuAnAsync(int count, CancellationToken ct)
    {
        await EnsureDanhMucLoaiDuAnAsync(ct);

        var faker = new DuAnFaker()
            .WithTrangThai(FKReferenceData.TrangThaiMoi)
            .WithLoaiDuAn(FKReferenceData.LoaiDuAnCoSoHaTan);

        var entities = faker.Generate(count);
        _db.Set<DuAn>().AddRange(entities);
        await _db.SaveChangesAsync(ct);

        Console.WriteLine($"Seeded {count} DuAn");
        return count;
    }

    /// <summary>
    /// Seed GoiThau entities with auto-seed DuAn if needed.
    /// </summary>
    public async Task<int> SeedGoiThauAsync(int count, CancellationToken ct)
    {
        var duAnIds = await EnsureDuAnExistsAsync(count, ct);

        var entities = new List<GoiThau>();
        for (int i = 0; i < count; i++)
        {
            var duAnId = duAnIds[i % duAnIds.Count];
            entities.Add(new GoiThauFaker(duAnId).Generate());
        }

        _db.Set<GoiThau>().AddRange(entities);
        await _db.SaveChangesAsync(ct);

        Console.WriteLine($"Seeded {count} GoiThau");
        return count;
    }

    /// <summary>
    /// Seed HopDong entities with auto-seed GoiThau (and DuAn) if needed.
    /// Handles one-to-one GoiThau-HopDong relationship.
    /// </summary>
    public async Task<int> SeedHopDongAsync(int count, CancellationToken ct)
    {
        // Get available GoiThau without HopDong (for 1:1 relationship)
        var availableGoiThaus = await _db.Set<GoiThau>()
            .Where(g => !_db.Set<HopDong>().Any(h => h.GoiThauId == g.Id))
            .Select(g => new { g.Id, g.DuAnId })
            .ToListAsync(ct);

        // Auto-seed more GoiThau if needed
        if (availableGoiThaus.Count < count)
        {
            var neededCount = count - availableGoiThaus.Count;
            await SeedGoiThauAsync(neededCount, ct);

            // Refresh available GoiThaus
            availableGoiThaus = await _db.Set<GoiThau>()
                .Where(g => !_db.Set<HopDong>().Any(h => h.GoiThauId == g.Id))
                .Select(g => new { g.Id, g.DuAnId })
                .ToListAsync(ct);
        }

        var entities = new List<HopDong>();
        for (int i = 0; i < count && i < availableGoiThaus.Count; i++)
        {
            var gt = availableGoiThaus[i];
            entities.Add(new HopDongFaker(gt.DuAnId, gt.Id).Generate());
        }

        _db.Set<HopDong>().AddRange(entities);
        await _db.SaveChangesAsync(ct);

        Console.WriteLine($"Seeded {count} HopDong");
        return entities.Count;
    }

    /// <summary>
    /// Seed all entities in dependency order: DanhMuc → DuAn → GoiThau → HopDong.
    /// Creates linked chain (N DuAn → N GoiThau → N HopDong).
    /// </summary>
    public async Task<int> SeedAllAsync(int count, CancellationToken ct)
    {
        await EnsureDanhMucLoaiDuAnAsync(ct);

        // Seed DuAn
        var duAns = new DuAnFaker()
            .WithTrangThai(FKReferenceData.TrangThaiMoi)
            .WithLoaiDuAn(FKReferenceData.LoaiDuAnCoSoHaTan)
            .Generate(count);
        _db.Set<DuAn>().AddRange(duAns);
        await _db.SaveChangesAsync(ct);

        // Seed GoiThau (linked to DuAn)
        var goiThaus = duAns.Select(da => new GoiThauFaker(da.Id).Generate()).ToList();
        _db.Set<GoiThau>().AddRange(goiThaus);
        await _db.SaveChangesAsync(ct);

        // Seed HopDong (linked to DuAn + GoiThau, respecting 1:1)
        var hopDongs = duAns.Zip(goiThaus, (da, gt) => new HopDongFaker(da.Id, gt.Id).Generate()).ToList();
        _db.Set<HopDong>().AddRange(hopDongs);
        await _db.SaveChangesAsync(ct);

        var total = count * 3;
        Console.WriteLine($"Seeded {count} DuAn, {count} GoiThau, {count} HopDong (total: {total})");
        return total;
    }

    // === Auto-seed helpers ===

    private async Task EnsureDanhMucLoaiDuAnAsync(CancellationToken ct)
    {
        if (!await _db.Set<DanhMucLoaiDuAn>().AnyAsync(ct))
        {
            var ts = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
            _db.Set<DanhMucLoaiDuAn>().AddRange(
                new DanhMucLoaiDuAn { Id = 1, Ma = "CDS", Ten = "Cơ sở hạ tầng", CreatedAt = ts, Used = true },
                new DanhMucLoaiDuAn { Id = 2, Ma = "DA06", Ten = "Đề án 06", CreatedAt = ts, Used = true },
                new DanhMucLoaiDuAn { Id = 3, Ma = "KHTP", Ten = "Kế hoạch TP HCM", CreatedAt = ts, Used = true }
            );
            await _db.SaveChangesAsync(ct);
            Console.WriteLine("Auto-seeded DanhMucLoaiDuAn");
        }
    }

    private async Task<List<Guid>> EnsureDuAnExistsAsync(int minCount, CancellationToken ct)
    {
        var ids = await _db.Set<DuAn>().Select(d => d.Id).ToListAsync(ct);
        if (ids.Count < minCount)
        {
            var needed = minCount - ids.Count;
            await SeedDuAnAsync(needed, ct);
            ids = await _db.Set<DuAn>().Select(d => d.Id).ToListAsync(ct);
        }
        return ids;
    }
}