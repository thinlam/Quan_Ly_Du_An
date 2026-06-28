using QLDA.WebApi.Models.TepDinhKems;

namespace QLDA.WebApi.Models.QuyetDinhLapBanQLDAs;

public static class QuyetDinhLapBanQldaMappingConfiguration {
    public static QuyetDinhLapBanQldaModel ToModel(this QuyetDinhLapBanQLDA entity,
        List<TepDinhKem>? danhSachTepDinhKem = null) =>
        new() {
            Id = entity.Id,
            DuAnId = entity.DuAnId,
            BuocId = entity.BuocId == 0 ? null : entity.BuocId,
            SoQuyetDinh = entity.So, //Số quyết định
            NgayQuyetDinh = entity.Ngay, //Ngày quyết định
            TrichYeu = entity.TrichYeu,
            NgayKy = entity.NgayKy,
            NguoiKy = entity.NguoiKy,
            SoDuThao = entity.SoDuThao,
            TrichYeuDuThao = entity.TrichYeuDuThao,
            DanhSachTepDinhKem = danhSachTepDinhKem?
                // .Where(o => o.GroupType == nameof(EGroupType.QuyetDinhLapBanQLDA))
                .Select(o => o.ToModel()).ToList(),
            DanhSachThanhVien = [.. entity.ThanhViens.Select(e => e.ToModel())]
        };


    public static QuyetDinhLapBanQLDA ToEntity(this QuyetDinhLapBanQldaModel model) {
        ManagedException.ThrowIf(model.DanhSachThanhVien == null || model.DanhSachThanhVien.Count == 0,
            "Phải có ít nhất 1 thành viên");
        return new() {
            Id = model.GetId(),
            DuAnId = model.DuAnId,
            BuocId = model.BuocId == 0 ? null : model.BuocId,
            So = model.SoQuyetDinh, //Số quyết định
            Ngay = model.NgayQuyetDinh, //Ngày quyết định
            TrichYeu = model.TrichYeu,
            NgayKy = model.NgayKy,
            NguoiKy = model.NguoiKy,
            SoDuThao = model.SoDuThao,
            TrichYeuDuThao = model.TrichYeuDuThao,
            ThanhViens = [.. model.DanhSachThanhVien.Select(e => e.ToEntity())],
        };
    }

    public static void Update(this QuyetDinhLapBanQLDA entity, QuyetDinhLapBanQldaModel model) {
        entity.DuAnId = model.DuAnId;
        entity.BuocId = model.BuocId == 0 ? null : model.BuocId;
        entity.So = model.SoQuyetDinh; //Số quyết định
        entity.Ngay = model.NgayQuyetDinh; //Ngày quyết định
        entity.TrichYeu = model.TrichYeu;
        entity.SoDuThao = model.SoDuThao;
        entity.TrichYeuDuThao = model.TrichYeuDuThao;
        entity.NgayKy = model.NgayKy;
        entity.NguoiKy = model.NguoiKy;
        entity.ThanhViens = model.DanhSachThanhVien?.Select(e => e.ToEntity()).ToList() ?? [];
    }
}