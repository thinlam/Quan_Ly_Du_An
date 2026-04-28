using Bogus;
using QLDA.Application.DuAns.DTOs;
using QLDA.Application.GoiThaus.DTOs;
using QLDA.Application.HopDongs.DTOs;

namespace QLDA.Tests.Fakers;

public class DuAnInsertDtoFaker : Faker<DuAnInsertDto>
{
    public DuAnInsertDtoFaker()
    {
        RuleFor(x => x.TenDuAn, f => f.Company.CompanyName());
        RuleFor(x => x.MaDuAn, f => f.Random.AlphaNumeric(8).ToUpper());
        RuleFor(x => x.LoaiDuAnId, 1);
        RuleFor(x => x.TrangThaiDuAnId, 1);
        RuleFor(x => x.DuAnTrongDiem, false);
        RuleFor(x => x.ThoiGianKhoiCong, 2024);
        RuleFor(x => x.ThoiGianHoanThanh, 2026);
        RuleFor(x => x.DiaDiem, f => f.Address.City());
        RuleFor(x => x.TongMucDauTu, f => f.Random.Long(1_000_000_000, 100_000_000_000));
    }
}

public class DuAnUpdateModelFaker : Faker<DuAnUpdateModel>
{
    public DuAnUpdateModelFaker(Guid id)
    {
        RuleFor(x => x.Id, id);
        RuleFor(x => x.TenDuAn, f => f.Company.CompanyName() + " Updated");
        RuleFor(x => x.MaDuAn, f => f.Random.AlphaNumeric(8).ToUpper());
        RuleFor(x => x.LoaiDuAnId, 1);
        RuleFor(x => x.TrangThaiDuAnId, 1);
        RuleFor(x => x.DuAnTrongDiem, false);
    }
}

public class GoiThauInsertDtoFaker : Faker<GoiThauInsertDto>
{
    public GoiThauInsertDtoFaker(Guid duAnId)
    {
        RuleFor(x => x.DuAnId, duAnId);
        RuleFor(x => x.Ten, f => f.Commerce.ProductName());
        RuleFor(x => x.GiaTri, f => f.Random.Long(100_000_000, 10_000_000_000));
    }
}

public class GoiThauUpdateDtoFaker : Faker<GoiThauUpdateDto>
{
    public GoiThauUpdateDtoFaker(Guid id)
    {
        RuleFor(x => x.Id, id);
        RuleFor(x => x.Ten, f => f.Commerce.ProductName() + " Updated");
        RuleFor(x => x.GiaTri, f => f.Random.Long(100_000_000, 10_000_000_000));
    }
}

public class HopDongInsertDtoFaker : Faker<HopDongInsertDto>
{
    public HopDongInsertDtoFaker(Guid duAnId, Guid goiThauId)
    {
        RuleFor(x => x.DuAnId, duAnId);
        RuleFor(x => x.GoiThauId, goiThauId);
        RuleFor(x => x.Ten, f => f.Commerce.ProductName());
        RuleFor(x => x.SoHopDong, f => "HD_" + f.Random.AlphaNumeric(8).ToUpper());
        RuleFor(x => x.GiaTri, f => f.Random.Long(100_000_000, 10_000_000_000));
        RuleFor(x => x.NgayKy, DateTimeOffset.UtcNow);
        RuleFor(x => x.IsBienBan, true);
    }
}

public class HopDongUpdateDtoFaker : Faker<HopDongUpdateDto>
{
    public HopDongUpdateDtoFaker(Guid id, Guid goiThauId)
    {
        RuleFor(x => x.Id, id);
        RuleFor(x => x.GoiThauId, goiThauId);
        RuleFor(x => x.Ten, f => f.Commerce.ProductName() + " Updated");
        RuleFor(x => x.SoHopDong, f => "HD_UPD_" + f.Random.AlphaNumeric(8).ToUpper());
        RuleFor(x => x.GiaTri, f => f.Random.Long(100_000_000, 10_000_000_000));
        RuleFor(x => x.IsBienBan, true);
    }
}
