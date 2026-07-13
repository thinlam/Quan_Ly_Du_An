using System.ComponentModel;

namespace QLDA.Domain.Enums;

public enum EnumLoaiVanBanQuyetDinh {
  [Description("Quyết định duyệt dự án")] QuyetDinhDuyetDuAn,
  [Description("Quyết định duyệt KHLCNT")] QuyetDinhDuyetKHLCNT,
  [Description("Quyết định duyệt quyết toán")] QuyetDinhDuyetQuyetToan,
  [Description("Quyết định lập Ban QLDA")] QuyetDinhLapBanQLDA,
  [Description("Quyết định lập Bên mời thầu")] QuyetDinhLapBenMoiThau,
  [Description("Quyết định lập Hội đồng thẩm định")] QuyetDinhLapHoiDongThamDinh,
  [Description("Văn bản pháp lý")] VanBanPhapLy,
  [Description("Văn bản chủ trương")] VanBanChuTruong,
  [Description("Kế hoạch lựa chọn nhà thầu")] KeHoachLuaChonNhaThau,
  [Description("Quyết định phê duyệt dự toán")] PheDuyetDuToan,
  [Description("Quyết định hồ sơ mời thầu điện tử")] HoSoMoiThauDienTu,
}
