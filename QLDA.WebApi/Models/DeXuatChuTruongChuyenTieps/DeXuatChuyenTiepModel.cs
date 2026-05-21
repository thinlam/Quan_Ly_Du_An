using System.ComponentModel;
using QLDA.Domain.Interfaces;
using QLDA.WebApi.Models.TepDinhKems;
using SequentialGuid;

namespace QLDA.WebApi.Models.PheDuyetEntityNames.DeXuatChuTruongChuyenTieps;

public class DeXuatChuyenTiepModel : IHasKey<Guid?>, IMustHaveId<Guid>, IMayHaveTepDinhKemModel, ITienDo{
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

    public long? SoLieuGiaiNgan { get; set; }
    public long? UocGiaiNgan { get; set; }
    public long? NhuCauKinhPhi { get; set; }
    public string? KhoiLuongThucTe { get; set; }
    public string? KhoiLuongDuKien { get; set; }
    public int? TrangThaiId { get; set; }
    public List<TepDinhKemModel>? DanhSachTepDinhKem { get; set; }

}