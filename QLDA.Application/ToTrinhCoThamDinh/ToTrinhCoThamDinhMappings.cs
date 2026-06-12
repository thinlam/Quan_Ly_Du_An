using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.ToTrinhCoThamDinhs.DTOs;

namespace QLDA.Application.ToTrinhCoThamDinhs;

public static class ToTrinhCoThamDinhMappings
{
    public static ToTrinhCoThamDinhDto ToDto(this ToTrinhCoThamDinh entity, List<TepDinhKem>? files = null, List<TepDinhKem>? fileThamDinhs = null) {
        return new ToTrinhCoThamDinhDto
        {
            Id = entity.Id,
            DuAnId = entity.DuAnId,
            BuocId = entity.BuocId,
            NgayToTrinh = entity.NgayToTrinh,
            TrichYeu = entity.TrichYeu,
            So = entity.So,
            Loai = entity.Loai,
            TrangThaiId = entity.TrangThaiId,
            KetQuaThamDinh = entity.KetQuaThamDinh,
            KetQuaThamTra = entity.KetQuaThamTra,
            DanhSachTepThamDinh = fileThamDinhs?.Select(x => x.ToDto()).ToList(),
            DanhSachTepDinhKem = files?.Select(x => x.ToDto()).ToList(),
        };
    }
    public static ToTrinhCoThamDinh ToEntity(this ToTrinhCoThamDinhDto entity, List<TepDinhKem>? files = null, List<TepDinhKem>? fileThamDinhs = null)
    {
        return new ToTrinhCoThamDinh
        {
            Id = entity.GetId(),
            DuAnId = entity.DuAnId,
            BuocId = entity.BuocId,
            NgayToTrinh = entity.NgayToTrinh,
            TrichYeu = entity.TrichYeu,
            So = entity.So,
            Loai = entity.Loai,
            TrangThaiId = entity.TrangThaiId,
            TrangThaiThamTraId = entity.TrangThaiThamTraId,
            KetQuaThamDinh = entity.KetQuaThamDinh,
            KetQuaThamTra = entity.KetQuaThamTra,
        };
    }
    public static ToTrinhCoThamDinh ToEntity(this ToTrinhCoThamDinhInsUpdDto entity)
    {
        return new ToTrinhCoThamDinh
        {
            Id = entity.Id?? Guid.NewGuid() ,
            DuAnId = entity.DuAnId,
            BuocId = entity.BuocId,
            NgayToTrinh = entity.NgayToTrinh,
            TrichYeu = entity.TrichYeu,
            So = entity.So,
            Loai = entity.Loai,
            TrangThaiThamTraId = entity.TrangThaiThamTraId,
            KetQuaThamDinh = entity.KetQuaThamDinh,
            KetQuaThamTra = entity.KetQuaThamTra,
        };
    }
}