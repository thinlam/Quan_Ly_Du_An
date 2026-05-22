using Azure.Core;
using QLDA.Application.DeXuatNhuCauKinhPhiNams.DTOs;
using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.DeXuatNhuCauKinhPhiNamMappings;

public static class DeXuatNhuCauKinhPhiNamMappings
{
    public static DeXuatNhuCauKinhPhiNam ToEntity(this DeXuatNhuCauKinhPhiNamInsertDto dto) {
        Guid id = dto.Id ?? Guid.NewGuid();
        return new DeXuatNhuCauKinhPhiNam {
            Id = id,
            So = dto.So,
            GhiChu = dto.GhiChu,
            NgayKeHoach = dto.NgayKeHoach,
            TrichYeu = dto.TrichYeu,
            TongKinhPhiDeXuat = dto.TongKinhPhiDeXuat,
            DeXuats = [..dto.DanhSachDeXuat?.Select(dx => new DeXuatTrinhKinhPhiNam {
                LeftId = id,
                RightId =dx
            }) ?? []]
        };
    }

    public static DeXuatNhuCauKinhPhiNamDto ToDto(this DeXuatNhuCauKinhPhiNam entity, List<TepDinhKem>? files = null) {
        return new DeXuatNhuCauKinhPhiNamDto {
            Id = entity.Id,
            So = entity.So,
            GhiChu = entity.GhiChu,
            NgayKeHoach = entity.NgayKeHoach,
            TrichYeu = entity.TrichYeu,
            TongKinhPhiDeXuat = entity.TongKinhPhiDeXuat,
            TrangThaiId = entity.TrangThaiId,
            DanhSachTepDinhKem = files?.Select(x => x.ToDto()).ToList(),
            DanhSachDeXuat = [..entity.DeXuats? .Select(xuLy => new DeXuatNhuCauKinhPhi
                                                                {
                                                                    Id= xuLy.RightId
                                                                }) ?? []]
        };
    }
}