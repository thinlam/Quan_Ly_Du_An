using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.ToTrinhPheDuyets.DTOs;

namespace QLDA.Application.ToTrinhPheDuyets;

public static class ToTrinhPheDuyetMappings {
    

    public static ToTrinhPheDuyetDto ToDto(this ToTrinhPheDuyet entity, List<Attachment>? files = null) {
        return new ToTrinhPheDuyetDto
        {
            Id = entity.Id,
            DuAnId = entity.DuAnId,
            BuocId = entity.BuocId,
            NgayToTrinh = entity.NgayToTrinh,
            TrichYeu = entity.TrichYeu,
            So = entity.So,
            Ten = entity.Ten ?? string.Empty,
            Loai = entity.Loai ?? string.Empty,
            TrangThaiId = entity.TrangThaiId,
            DanhSachTepDinhKem = files?.Select(x => x.ToDto()).ToList(),
        };
    }
}