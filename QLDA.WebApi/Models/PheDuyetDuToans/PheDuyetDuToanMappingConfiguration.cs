using QLDA.WebApi.Models.TepDinhKems;

namespace QLDA.WebApi.Models.PheDuyetDuToans;

public static class PheDuyetDuToanMappingConfiguration {
    public static PheDuyetDuToanModel ToModel(this PheDuyetDuToan entity,
        List<TepDinhKem>? danhSachTepDinhKem = null) =>
        new() {
            Id = entity.Id,
            BuocId = entity.BuocId,
            DuAnId = entity.DuAnId,
            ChucVuId = entity.ChucVuId,
            NgayKy = entity.NgayKy,
            NguoiKy = entity.NguoiKy,
            SoVanBan = entity.So,
            GiaTriDuThau = entity.GiaTriDuThau,
            TrichYeu = entity.TrichYeu,
            TrangThaiId = entity.TrangThaiId,
            TenTrangThai = entity.TrangThai != null ? entity.TrangThai.Ten : null,
            DanhSachTepDinhKem = danhSachTepDinhKem?
                // .Where(o => o.GroupType == nameof(EGroupType.PheDuyetDuToan))
                .Select(o => o.ToModel()).ToList()
        };


    public static PheDuyetDuToan ToEntity(this PheDuyetDuToanModel model)
        => new() {
            Id = model.GetId(),
            BuocId = model.BuocId,
            DuAnId = model.DuAnId,
            ChucVuId = model.ChucVuId,
            NgayKy = model.NgayKy,
            NguoiKy = model.NguoiKy,
            So = model.SoVanBan,
            GiaTriDuThau = model.GiaTriDuThau,
            TrichYeu = model.TrichYeu,
        };

    public static void Update(this PheDuyetDuToan entity, PheDuyetDuToanModel model) {
        entity.BuocId = model.BuocId;
        entity.DuAnId = model.DuAnId;
        entity.ChucVuId = model.ChucVuId;
        entity.NgayKy = model.NgayKy;
        entity.NguoiKy = model.NguoiKy;
        entity.So = model.SoVanBan;
        entity.GiaTriDuThau = model.GiaTriDuThau;
        entity.TrichYeu = model.TrichYeu;
    }
}