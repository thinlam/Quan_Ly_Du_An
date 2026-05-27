using QLDA.Application.ToTrinhKetQuaGoiThaus.DTOs;
using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.ToTrinhKetQuaGoiThauMappings;

public static class ToTrinhKetQuaGoiThauMappings {
    public static void SyncGoiThauIds(this ToTrinhKetQuaGoiThau entity, List<Guid>? goiThauIds) {
        if (goiThauIds is null) {
            entity.GoiThaus = [];
            return;
        }

        entity.GoiThaus ??= [];
        entity.GoiThaus.Clear();
        foreach (var id in goiThauIds) {
            entity.GoiThaus.Add(new GoiThauTrinhPheDuyetKetQua {
                ToTrinhId = entity.Id,
                GoiThauId = id,
            });
        }
    }

    public static ToTrinhKetQuaGoiThauDto ToDto(this ToTrinhKetQuaGoiThau entity, List<TepDinhKem>? files = null) =>
        new() {
            Id = entity.Id,
            So = entity.So,
            NgayTrinh = entity.NgayTrinh,
            TrichYeu = entity.TrichYeu,
            TrangThaiId = entity.TrangThaiId,
            TrangThaiDangTaiId = entity.TrangThaiDangTaiId,
            DanhSachTepDinhKem = files?.Select(x => x.ToDto()).ToList(),
            DanhSachGoiThau = entity.GoiThaus?.Select(x => x.GoiThauId).ToList(),
        };
}
