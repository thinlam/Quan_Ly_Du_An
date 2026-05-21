using System.ComponentModel;
using QLDA.Domain.Interfaces;
using QLDA.WebApi.Models.TepDinhKems;
using SequentialGuid;

namespace QLDA.WebApi.Models.DeXuatChuTruongMois;

public class DeXuatChuTruongMoiModel : IHasKey<Guid?>, IMustHaveId<Guid>, IMayHaveTepDinhKemModel, ITienDo{
    [DefaultValue(null)] public Guid? Id { get; set; }

    /// <summary>
    /// Nếu có id => cập nhật, ngược lại là tạo mới
    /// </summary>
    /// <returns></returns>
    public Guid GetId() {
        Id ??= SequentialGuidGenerator.Instance.NewGuid();
        return (Guid)Id;
    }

    public Guid SetId() {
        
        return SequentialGuidGenerator.Instance.NewGuid();
    }
    /// <summary>
    /// Tên dự án
    /// </summary>
    public int? BuocId { get; set; }
    public Guid DuAnId { get; set; }

    public string? TomTatNoiDung { get; set; }

    public long? TongMucDauTu { get; set; }

    public DateTimeOffset? NgayBatDauDuKien { get; set; }
    public int? HinhThucDauTuId { get; set; }
    public long? LanhDaoPhuTrachId { get; set; }
    public long? DonViPhuTrachChinhId { get; set; }
    public long? NguoiXuLyChinhId { get; set; }
    public int? TrangThaiId { get; set; }
    public List<long>? DonViPhoiHopIds { get; set; }
    public List<TepDinhKemModel>? DanhSachTepDinhKem { get; set; }

}