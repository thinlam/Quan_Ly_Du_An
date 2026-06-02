using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.ToTrinhThamDinhNhaThaus.DTOs;
using QLDA.Application.KeHoachTrienKhaiHangMucs.DTOs;

namespace QLDA.Application.KeHoachTrienKhaiHangMucMappings;

public static class KeHoachTrienKhaiHangMucMappings
{
    public static void SyncCanBoPhoiHop(this KeHoachTrienKhaiHangMuc entity, List<long>? canBoIds)
    {
        if (canBoIds is null)
        {
            entity.CanBoTrienKhais = [];
            return;
        }

        entity.CanBoTrienKhais ??= [];
        entity.CanBoTrienKhais.Clear();

        foreach (var id in canBoIds)
        {
            entity.CanBoTrienKhais.Add(new CanBoTrienKhaiHangMuc
            {
                KeHoachId = entity.Id,
                CanBoId = id
            });
        }
    }
    public static KeHoachTrienKhaiHangMuc ToEntity(this KeHoachTrienKhaiHangMucDto dto)
    {
        var entity = new KeHoachTrienKhaiHangMuc()
        {
            DuAnId = dto.DuAnId,
            BuocId = dto.BuocId,
            So = dto.So,
            NgayToTrinh = dto.NgayTrinh,
            TrichYeu = dto.TrichYeu,
            TrangThaiId = dto.TrangThaiId,

            TenHangMuc = dto.TenHangMuc,
            CanBoChuTriId = dto.CanBoChuTriId,
            NgayBatDau = dto.NgayBatDau,
            NgayKetThuc = dto.NgayKetThuc,
            ThoiHan = dto.ThoiHan,
            KinhPhi = dto.KinhPhi,
            GiaiDoanId = dto.GiaiDoanId,
        };

        entity.SyncCanBoPhoiHop(dto.DanhSachCanBoPhoiHop);

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
            TenHangMuc = entity.TenHangMuc,
            CanBoChuTriId = entity.CanBoChuTriId,
            NgayBatDau = entity.NgayBatDau,
            NgayKetThuc = entity.NgayKetThuc,
            ThoiHan = entity.ThoiHan,
            KinhPhi = entity.KinhPhi,
            DanhSachTepDinhKem = files?.Select(x => x.ToDto()).ToList(),
            DanhSachCanBoPhoiHop = entity.CanBoTrienKhais?.Select(x=>x.CanBoId).ToList(),

        };
}
