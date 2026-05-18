namespace QLDA.WebApi.Models.DmLoaiDieuChinhs;

public static class DanhMucLoaiDieuChinhMappingConfigurations {
    public static DanhMucLoaiDieuChinhModel ToModel(this DanhMucLoaiDieuChinh entity)
        => new() {
            Id = entity.Id,
            Ma = entity.Ma,
            Ten = entity.Ten,
            MoTa = entity.MoTa,
            Stt = entity.Stt,
            Used = entity.Used
        };

    public static DanhMucLoaiDieuChinh ToEntity(this DanhMucLoaiDieuChinhModel model)
        => new() {
            Id = model.Id ?? 0,
            Ma = model.Ma,
            Ten = model.Ten,
            MoTa = model.MoTa,
            Stt = model.Stt,
            Used = model.Used
        };
}