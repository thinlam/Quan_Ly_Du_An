using QLDA.Application.QuyetDinhDuyetKHLCNTs.DTOs;

namespace QLDA.Application.QuyetDinhDuyetKHLCNTs;

public static class QuyetDinhDuyetKHLCNTMappings
{
    public static QuyetDinhDuyetKHLCNT ToEntity(this QuyetDinhDuyetKHLCNTInsertDto dto)
    {
        return new QuyetDinhDuyetKHLCNT
        {
            Id = Guid.NewGuid(),
            KeHoachLuaChonNhaThauId = dto.KeHoachLuaChonNhaThauId,
            VanBanQuyetDinh = new VanBanQuyetDinh
            {
                DuAnId = dto.DuAnId,
                BuocId = dto.BuocId,
                So = dto.SoQuyetDinh,
                Ngay = dto.NgayQuyetDinh,
                CoQuanQuyetDinh = dto.CoQuanQuyetDinh,
                TrichYeu = dto.TrichYeu,
                NgayKy = dto.NgayKy,
                NguoiKy = dto.NguoiKy
            }
        };
    }

    public static QuyetDinhDuyetKHLCNT ToEntity(this QuyetDinhDuyetKHLCNTUpdateDto dto)
    {
        return new QuyetDinhDuyetKHLCNT
        {
            Id = dto.Id,
            KeHoachLuaChonNhaThauId = dto.KeHoachLuaChonNhaThauId,
            VanBanQuyetDinh = new VanBanQuyetDinh
            {
                So = dto.SoQuyetDinh,
                Ngay = dto.NgayQuyetDinh,
                CoQuanQuyetDinh = dto.CoQuanQuyetDinh,
                TrichYeu = dto.TrichYeu,
                NgayKy = dto.NgayKy,
                NguoiKy = dto.NguoiKy
            }
        };
    }

    public static QuyetDinhDuyetKHLCNTDto ToDto(this QuyetDinhDuyetKHLCNT entity)
    {
        return new QuyetDinhDuyetKHLCNTDto
        {
            Id = entity.Id,
            KeHoachLuaChonNhaThauId = entity.KeHoachLuaChonNhaThauId,
            VanBanQuyetDinh = new TongHopVanBanQuyetDinhs.DTOs.VanBanQuyetDinhDto
            {
                TableName =  entity.VanBanQuyetDinh.Loai,// EnumLoaiVanBanQuyetDinh.QuyetDinhDuyetKHLCNT,
                DuAnId = entity.VanBanQuyetDinh.DuAnId,
                BuocId = entity.VanBanQuyetDinh.BuocId,
                So = entity.VanBanQuyetDinh.So,
                Ngay = entity.VanBanQuyetDinh.Ngay,
                CoQuanQuyetDinh = entity.VanBanQuyetDinh.CoQuanQuyetDinh,
                TrichYeu = entity.VanBanQuyetDinh.TrichYeu,
                NgayKy = entity.VanBanQuyetDinh.NgayKy,
               NguoiKy = entity.VanBanQuyetDinh.NguoiKy
            },

        };
    }
}