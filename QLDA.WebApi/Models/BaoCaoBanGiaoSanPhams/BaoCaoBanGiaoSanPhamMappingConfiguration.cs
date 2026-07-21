using QLDA.WebApi.Models.TepDinhKems;
using BuildingBlocks.Domain.Entities;

namespace QLDA.WebApi.Models.BaoCaoBanGiaoSanPhams;

public static class BaoCaoBanGiaoSanPhamMappingConfiguration {
    public static BaoCaoBanGiaoSanPhamModel ToModel(this BaoCaoBanGiaoSanPham entity,
        List<Attachment>? danhSachTepDinhKem = null) =>
        new() {
            Id = entity.Id,
            BuocId = entity.BuocId,
            DuAnId = entity.DuAnId,
            Ngay = entity.Ngay,
            NoiDung = entity.NoiDung,
            NguoiBaoCaoId = long.Parse(entity.CreatedBy),
            DonViBanGiaoId = entity.DonViBanGiaoId,
            DonViNhanBanGiaoId = entity.DonViNhanBanGiaoId,
            DanhSachTepDinhKem = danhSachTepDinhKem?
                // .Where(o => o.GroupType == nameof(EGroupType.BaoCaoBanGiaoSanPham))
                .Select(o => o.ToModel()).ToList()
        };


    public static BaoCaoBanGiaoSanPham ToEntity(this BaoCaoBanGiaoSanPhamModel model)
        => new() {
            Id = model.GetId(),
            BuocId = model.BuocId == 0 ? null : model.BuocId,
            DuAnId = model.DuAnId,
            Ngay = model.Ngay,
            NoiDung = model.NoiDung,
            DonViBanGiaoId = model.DonViBanGiaoId,
            DonViNhanBanGiaoId = model.DonViNhanBanGiaoId,
        };

    public static void Update(this BaoCaoBanGiaoSanPham entity, BaoCaoBanGiaoSanPhamModel model) {
        entity.BuocId = model.BuocId == 0 ? null : model.BuocId;
        entity.DuAnId = model.DuAnId;
        entity.Ngay = model.Ngay;
        entity.NoiDung = model.NoiDung;
        entity.DonViBanGiaoId = model.DonViBanGiaoId;
        entity.DonViNhanBanGiaoId = model.DonViNhanBanGiaoId;
    }
}
