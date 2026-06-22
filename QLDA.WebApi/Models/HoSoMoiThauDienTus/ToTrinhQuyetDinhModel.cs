using SequentialGuid;
using QLDA.WebApi.Models.TepDinhKems;

namespace QLDA.WebApi.Models.HoSoMoiThauDienTus;

public class ToTrinhQuyetDinhModel :  IMayHaveTepDinhKemModel {

    public long? Id { get; set; }
    public string? So { get; set; }
    public DateTimeOffset? Ngay { get; set; }
    public string? TrichYeu { get; set; }
    public string? NguoiKy { get; set; }
    public int? ChucVu { get; set; }
    public List<TepDinhKemModel>? DanhSachTepDinhKem { get; set; }
}