using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.ThuyetMinhDuAns.DTOs;

namespace QLDA.Application.ThuyetMinhDuAns;

public static class ThuyetMinhDuAnMappings
{
    public static ThuyetMinhDuAn ToEntity(this ThuyetMinhDuAnInsertDto dto)
    {
        return new ThuyetMinhDuAn
        {
            Id = dto.Id ?? Guid.NewGuid(),
            DuAnId = dto.DuAnId,
            BuocId = dto.BuocId,
            So = dto.So,
            NgayTrinh = dto.NgayTrinh,
            TrichYeu = dto.TrichYeu,
            KetQuaThamDinh = dto.KetQuaThamDinh,
            TrangThaiId = dto.TrangThaiId,
            TrangThaiThamDinhId = dto.TrangThaiThamDinhId
        };
    }

    public static ThuyetMinhDuAnDto ToDto(this ThuyetMinhDuAn entity, List<Attachment>? files = null, List<Attachment>? filesThamDinh = null)
    {
        return new ThuyetMinhDuAnDto
        {
            Id = entity.Id,
            DuAnId = entity.DuAnId,
            BuocId = entity.BuocId,
            So = entity.So,
            NgayTrinh = entity.NgayTrinh,
            KetQuaThamDinh = entity.KetQuaThamDinh,
            TrichYeu = entity.TrichYeu,
            TrangThaiThamDinhId = entity.TrangThaiThamDinhId,
            TrangThaiId = entity.TrangThaiId,
            DanhSachTepDinhKem = files?.Select(x => x.ToDto()).ToList(),
            DanhSachTepThamDinh = filesThamDinh?.Select(x => x.ToDto()).ToList(),
        };
    }
}