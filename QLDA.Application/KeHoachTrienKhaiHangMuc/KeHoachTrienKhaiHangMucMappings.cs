using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.ToTrinhThamDinhNhaThaus.DTOs;
using QLDA.Application.KeHoachTrienKhaiHangMucs.DTOs;

namespace QLDA.Application.KeHoachTrienKhaiHangMucMappings;

public static class KeHoachTrienKhaiHangMucMappings
{
    public static void SyncHangMuc(this KeHoachTrienKhaiHangMuc entity, List<HangMucTrienKhaiDto>? hangMucs)
    {
        if (hangMucs is null)
        {
            entity.DanhSachHangMuc = [];
            return;
        }

        entity.DanhSachHangMuc ??= [];
        entity.DanhSachHangMuc.Clear();

        foreach (var items in hangMucs)
        {
            entity.DanhSachHangMuc.Add(new HangMucKeHoach
            {
                KeHoachId = entity.Id   ,
                GiaiDoanId = items.GiaiDoanId,
                TenHangMuc = items.TenHangMuc,
                KinhPhi = items.KinhPhi,
                NgayBatDau = items.NgayBatDau,
                NgayKetThuc = items.NgayKetThuc,
                ThoiHan = items.ThoiHan,
                CanBoChuTriId = items.CanBoChuTriId,
                CanBoPhoiHopIds = items.CanBoPhoiHopIds,
                DonViChuTriId = items.DonViChuTriId,
                DonViPhoiHopIds = items.DonViPhoiHops,
            });
        }
    }
    public static KeHoachTrienKhaiHangMuc ToEntity(this KeHoachTrienKhaiHangMucDto dto)
    {
        var entity = new KeHoachTrienKhaiHangMuc()
        {
            Id= dto.Id??Guid.NewGuid(),
            DuAnId = dto.DuAnId,
            BuocId = dto.BuocId,
            So = dto.So,
            NgayToTrinh = dto.NgayTrinh,
            TrichYeu = dto.TrichYeu,
            TrangThaiId = dto.TrangThaiId,
        };

        entity.SyncHangMuc(dto.HangMucTrienKhai);

        return entity;
    }


    public static KeHoachTrienKhaiHangMucDto ToDto(this KeHoachTrienKhaiHangMuc entity, List<TepDinhKem>? files = null) =>
        new()
        {
            Id = entity.Id,
            So = entity.So,
            NgayTrinh = entity.NgayToTrinh,
            TrichYeu = entity.TrichYeu,
            TrangThaiId = entity.TrangThaiId,
            
            DanhSachTepDinhKem = files?.Select(x => x.ToDto()).ToList(),
            HangMucTrienKhai = entity.DanhSachHangMuc?.Select(x=>new HangMucTrienKhaiDto() {
                TenHangMuc = x.TenHangMuc,
                GiaiDoanId = x.GiaiDoanId,
                CanBoChuTriId = x.CanBoChuTriId,
                CanBoPhoiHopIds = x.CanBoPhoiHopIds,
                DonViChuTriId = x.DonViChuTriId,
                DonViPhoiHops = x.DonViPhoiHopIds,
                NgayBatDau = x.NgayBatDau,
                NgayKetThuc = x.NgayKetThuc,
                ThoiHan = x.ThoiHan,
                KinhPhi = x.KinhPhi,
                
            }).ToList(),

        };
}
