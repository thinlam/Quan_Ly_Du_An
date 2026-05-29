using QLDA.Application.ToTrinhThamDinhNhaThaus.DTOs;
using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.ToTrinhThamDinhNhaThaus;

public static class ToTrinhThamDinhNhaThauMappings
{
    public static void SyncNhaThauIds(this ToTrinhThamDinhNhaThau entity, List<KetQuaThamDinhNhaThau>? NhaThaus) {
        if (NhaThaus is null) {
            entity.NhaThaus = [];
            return;
        }
        entity.NhaThaus ??= [];
        entity.NhaThaus.Clear();
        foreach (var item in NhaThaus) {
            entity.NhaThaus.Add(new KetQuaThamDinhNhaThau
            {
                Id = Guid.NewGuid(),
                ToTrinhId = entity.Id,
                NhaThauId = item.NhaThauId,
                GoiThauId = item.GoiThauId,
                KetQuaDanhGia = item.KetQuaDanhGia,
            }); 
           
        }
    }

    public static ToTrinhThamDinhNhaThauDto ToDto(this ToTrinhThamDinhNhaThau entity, List<TepDinhKem>? files = null, List<TepDinhKem>? filesThamDinh = null) =>
        new() {
            Id = entity.Id,
            So = entity.So,
            NgayTrinh = entity.NgayTrinh,
            TrichYeu = entity.TrichYeu,
            TrangThaiId = entity.TrangThaiId,
            TrangThaiDangTaiId = entity.TrangThaiDangTaiId,
            DaThamDinh= entity.DaThamDinh,
            DanhSachTepDinhKem = files?.Select(x => x.ToDto()).ToList(),
            DanhSachTepThamDinh = filesThamDinh?.Select(x => x.ToDto()).ToList(),
            //DanhSachNhaThau = entity.DanhSachNhaThau?.Select(x => new KetQuaThamDinhNhaThau() { 
                                
            //}).ToList(),
        };
}
