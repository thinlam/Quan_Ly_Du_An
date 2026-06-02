using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.DuToanDauTus.DTOs;

namespace QLDA.Application.DuToanDauTus;

public static class DuToanDauTuMappings {
    public static DuToanDauTu ToEntity(this DuToanDauTuDto dto) {
        return new DuToanDauTu {
            Id = dto.Id ?? Guid.NewGuid(),
            DuAnId = dto.DuAnId,
            BuocId = dto.BuocId,
            NgayTrinh = dto.NgayTrinh,
            TrichYeu = dto.TrichYeu,
            SoToTrinh = dto.SoToTrinh,
           
            TongDuToan = dto.TongDuToan,
            TongMucDauTu = dto.TongMucDauTu,    
            PhuongAnThietKeId = dto.PhuongAnThietKeId,  
            NguonVonId = dto.NguonVonId,
            Nam = dto.Nam,
            NoiDungChiPhis = dto.NoiDungChiPhi
        };
    }



    public static DuToanDauTuDto ToDto(this DuToanDauTu entity, List<TepDinhKem>? files = null) {
        return new DuToanDauTuDto
        {
            Id = entity.Id,
            DuAnId = entity.DuAnId,
            BuocId = entity.BuocId,
            NgayTrinh = entity.NgayTrinh,
            TrichYeu = entity.TrichYeu,
            SoToTrinh = entity.SoToTrinh,
            TrangThaiId = entity.TrangThaiId,

            TongDuToan = entity.TongDuToan,
            TongMucDauTu = entity.TongMucDauTu,
            PhuongAnThietKeId = entity.PhuongAnThietKeId,
            NguonVonId = entity.NguonVonId,
            Nam = entity.Nam,
            NoiDungChiPhi = entity.NoiDungChiPhis,

            DanhSachTepDinhKem = files?.Select(x => x.ToDto()).ToList(),
        };
    }
}