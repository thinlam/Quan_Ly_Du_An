using QLDA.Application.QuyetDinhDuyetQuyetToans.DTOs;

namespace QLDA.Application.QuyetDinhDuyetQuyetToans;

public static class QuyetDinhDuyetQuyetToanMappings {
    public static QuyetDinhDuyetQuyetToan ToEntity(this QuyetDinhDuyetQuyetToanInsertDto dto) {
        return new QuyetDinhDuyetQuyetToan {
            Id = Guid.NewGuid(),
            DuAnId = dto.DuAnId,
            BuocId = dto.BuocId,
            So = dto.SoQuyetDinh,
            Ngay = dto.NgayQuyetDinh,
            CoQuanQuyetDinh = dto.CoQuanQuyetDinh,
            TrichYeu = dto.TrichYeu,
            NgayKy = dto.NgayKy,
            NguoiKy = dto.NguoiKy,
            GiaTri = dto.GiaTri
        };
    }

    public static void ToEntity(this QuyetDinhDuyetQuyetToanUpdateDto dto, QuyetDinhDuyetQuyetToan entity) {

        entity.So = dto.SoQuyetDinh;
        entity.Ngay = dto.NgayQuyetDinh;
        entity.CoQuanQuyetDinh = dto.CoQuanQuyetDinh;
        entity.TrichYeu = dto.TrichYeu;
        entity.NgayKy = dto.NgayKy;
        entity.NguoiKy = dto.NguoiKy;
        entity.GiaTri = dto.GiaTri;
    }

}
