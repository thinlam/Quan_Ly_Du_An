using QLDA.Domain.Entities;

namespace QLDA.FakeDataTool.Fakers;

/// <summary>
/// Bogus faker for DuAn entity with realistic Vietnamese project data.
/// </summary>
public class DuAnFaker : EntityFakerBase<DuAn>
{
    private static readonly string[] ProjectNames =
    [
        "Hệ thống quản lý văn bản điện tử",
        "Cổng thông tin điện tử thành phố",
        "Hệ thống quản lý hồ sơ điện tử",
        "Phần mềm quản lý nhân sự",
        "Hệ thống thanh toán điện tử",
        "Cơ sở dữ liệu quốc gia về dân cư",
        "Hệ thống giám sát an ninh mạng",
        "Phần mềm quản lý tài sản công",
        "Hệ thống báo cáo thống kê",
        "Mạng kết nối các cơ quan nhà nước",
        "Hệ thống chữ ký số",
        "Phần mềm quản lý dự án đầu tư"
    ];

    public DuAnFaker() : base()
    {
        RuleFor(e => e.TenDuAn, f => f.PickRandom(ProjectNames));
        RuleFor(e => e.MaDuAn, f => $"DA-{f.Random.Number(1000, 9999)}");
        RuleFor(e => e.DiaDiem, f => f.Address.City());
        RuleFor(e => e.MaNganSach, f => $"NS-{f.Random.Number(100, 999)}");
        RuleFor(e => e.DuAnTrongDiem, f => f.Random.Bool(0.3f));
        RuleFor(e => e.ThoiGianKhoiCong, f => 2024 + f.Random.Int(0, 3));
        RuleFor(e => e.ThoiGianHoanThanh, f => 2026 + f.Random.Int(0, 4));
        RuleFor(e => e.TongMucDauTu, f => f.Random.Long(1_000_000_000, 100_000_000_000));
        RuleFor(e => e.NgayBatDau, f => f.Date.PastOffset(2));
        RuleFor(e => e.NangLucThietKe, f => f.Commerce.ProductAdjective());
        RuleFor(e => e.QuyMoDuAn, f => f.PickRandom("Nhỏ", "Vừa", "Lớn", "Rất lớn"));
        RuleFor(e => e.KhaiToanKinhPhi, f => f.Random.Decimal(100_000_000, 50_000_000_000));
        RuleFor(e => e.GhiChu, f => f.Lorem.Sentence());

        // Base entity fields (MaterializedPathEntity)
        RuleFor(e => e.CreatedAt, f => DateTimeOffset.UtcNow);
        RuleFor(e => e.CreatedBy, "fake-tool");
        RuleFor(e => e.IsDeleted, false);
        RuleFor(e => e.Index, f => f.Random.Long(1_000_000));
        RuleFor(e => e.ParentId, (Guid?)null);
        RuleFor(e => e.Path, (_, e) => $"/{e.Id}/");
        RuleFor(e => e.Level, 0);
    }

    public DuAnFaker WithTrangThai(int trangThaiId)
    {
        RuleFor(e => e.TrangThaiDuAnId, trangThaiId);
        return this;
    }

    public DuAnFaker WithLoaiDuAn(int loaiDuAnId)
    {
        RuleFor(e => e.LoaiDuAnId, loaiDuAnId);
        return this;
    }
}