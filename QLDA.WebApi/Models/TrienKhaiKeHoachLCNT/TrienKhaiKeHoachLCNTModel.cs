using System.ComponentModel;
using QLDA.Domain.Interfaces;
using QLDA.WebApi.Models.DonViTuVanKeHoachs;
using QLDA.WebApi.Models.TepDinhKems;
using SequentialGuid;

namespace QLDA.WebApi.Models.TrienKhaiKeHoachLCNTs;

public class TrienKhaiKeHoachLCNTModel : IHasKey<Guid?>, IMustHaveId<Guid>, IMayHaveTepDinhKemModel, ITienDo
{
    [DefaultValue(null)] public Guid? Id { get; set; }

    /// <summary>
    /// Nếu có id => cập nhật, ngược lại là tạo mới
    /// </summary>
    /// <returns></returns>
    public Guid GetId()
    {
        Id ??= SequentialGuidGenerator.Instance.NewGuid();
        return (Guid)Id;
    }

    public Guid SetId()
    {

        return SequentialGuidGenerator.Instance.NewGuid();
    }

    public int? BuocId { get; set; }
    public Guid DuAnId { get; set; }
    public string So { get; set; } = string.Empty;
    public DateTimeOffset NgayTrinh { get; set; }
    public string? TrichYeu { get; set; } = string.Empty;
    public Guid GoiThauId { get; set; }
    public int? HinhThucLCNT { get; set; }
    public int? TrangThaiId { get; set; }
    public int? TrangThaiDangTaiId { get; set; }
    public long? GiaTri { get; set; }
    public string? ThoiGianThucHien { get; set; }
    public string? NoiDung { get; set; }
    public string? YeuCau { get; set; }
    public List<DonViTuVanKeHoachModel>? DonViTuVans { get; set; }

    public List<TepDinhKemModel>? DanhSachTepDinhKem { get; set; }
}