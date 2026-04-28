using QLDA.Domain.Entities;

namespace QLDA.FakeDataTool.Fakers;

/// <summary>
/// Bogus faker for HopDong entity with realistic Vietnamese contract data.
/// </summary>
public class HopDongFaker : EntityFakerBase<HopDong>
{
    public HopDongFaker(Guid? duAnId = null, Guid? goiThauId = null) : base()
    {
        RuleFor(e => e.DuAnId, duAnId ?? Guid.NewGuid());
        RuleFor(e => e.GoiThauId, goiThauId ?? Guid.NewGuid());
        RuleFor(e => e.Ten, f => $"Hợp đồng {f.Commerce.ProductName()}");
        RuleFor(e => e.SoHopDong, f => $"HD-{f.Random.Number(1000, 9999)}/{DateTime.UtcNow.Year}");
        RuleFor(e => e.NoiDung, f => f.Lorem.Paragraph());
        RuleFor(e => e.NgayKy, f => f.Date.PastOffset(1));
        RuleFor(e => e.GiaTri, f => f.Random.Long(100_000_000, 50_000_000_000));
        RuleFor(e => e.NgayHieuLuc, (f, e) => e.NgayKy?.AddDays(f.Random.Int(1, 30)));
        RuleFor(e => e.NgayDuKienKetThuc, (f, e) => e.NgayHieuLuc?.AddMonths(f.Random.Int(6, 24)));
        RuleFor(e => e.IsBienBan, false);

        // Base entity fields
        RuleFor(e => e.CreatedAt, f => DateTimeOffset.UtcNow);
        RuleFor(e => e.CreatedBy, "fake-tool");
        RuleFor(e => e.IsDeleted, false);
        RuleFor(e => e.Index, f => f.Random.Long(1_000_000));
    }

    public HopDongFaker WithDuAn(Guid duAnId)
    {
        RuleFor(e => e.DuAnId, duAnId);
        return this;
    }

    public HopDongFaker WithGoiThau(Guid goiThauId)
    {
        RuleFor(e => e.GoiThauId, goiThauId);
        return this;
    }

    public HopDongFaker WithIsBienBan(bool isBienBan)
    {
        RuleFor(e => e.IsBienBan, isBienBan);
        return this;
    }
}