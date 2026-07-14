namespace QLDA.WebApi.Models.KeHoachTrienKhaiHangMucs;

public static class KeHoachTrienKhaiHangMucMappingConfiguration
{
  

    public static KeHoachTrienKhaiHangMuc ToEntity(this KeHoachTrienKhaiHangMucModel model)
    { var id = model.GetId();
        return new() {
            Id = id,
            BuocId = model.BuocId,
            DuAnId = model.DuAnId,
            TrichYeu = model.TrichYeu,
            So = model.So,
            NgayToTrinh = model.NgayToTrinh,
            DanhSachHangMuc = model.DanhSachHangMuc!.Select(o => new HangMucKeHoach() {
                KeHoachId = id,
                GiaiDoanId = o.GiaiDoanId,
                TenHangMuc = o.TenHangMuc,
                KinhPhi = o.KinhPhi,
                NgayBatDau = o.NgayBatDau,
                NgayKetThuc = o.NgayKetThuc,
                ThoiHan = o.ThoiHan,
                CanBoChuTriId = o.CanBoChuTriId,
                CanBoPhoiHopIds = o.CanBoPhoiHopId,
                DonViChuTriId = o.DonViChuTriId,
                DonViPhoiHopIds = o.DonViPhoiHopId
            }).ToList()

        };
    }

  
}