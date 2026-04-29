using QLDA.Domain.Entities;

namespace QLDA.FakeDataTool.Fakers;

/// <summary>
/// Bogus faker for GoiThau entity with realistic Vietnamese bid package data.
/// </summary>
public class GoiThauFaker : EntityFakerBase<GoiThau>
{
    public GoiThauFaker(Guid? duAnId = null) : base()
    {
        RuleFor(e => e.DuAnId, duAnId ?? Guid.NewGuid());
        RuleFor(e => e.Ten, f => $"Gói thầu {f.Commerce.ProductName()}");
        RuleFor(e => e.GiaTri, f => f.Random.Long(100_000_000, 50_000_000_000));
        RuleFor(e => e.DaDuyet, false);
        RuleFor(e => e.ThoiGianThucHienGoiThau, f => $"{f.Random.Int(6, 36)} tháng");
        RuleFor(e => e.TomTatCongViecChinhGoiThau, f => f.Lorem.Sentence());

        // Base entity fields
        RuleFor(e => e.CreatedAt, f => DateTimeOffset.UtcNow);
        RuleFor(e => e.CreatedBy, "fake-tool");
        RuleFor(e => e.IsDeleted, false);
        RuleFor(e => e.Index, f => f.Random.Long(1_000_000));
    }

    public GoiThauFaker WithDuAn(Guid duAnId)
    {
        RuleFor(e => e.DuAnId, duAnId);
        return this;
    }

    public GoiThauFaker WithDaDuyet(bool daDuyet)
    {
        RuleFor(e => e.DaDuyet, daDuyet);
        return this;
    }
}