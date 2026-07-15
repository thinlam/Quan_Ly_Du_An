using QLDA.WebApi.Models.TepDinhKems;
using BuildingBlocks.Domain.Entities;

namespace QLDA.WebApi.Models.BaoCaoBaoHanhSanPhams;

public static class BaoCaoBaoHanhSanPhamMappingConfiguration {
    public static BaoCaoBaoHanhSanPhamModel ToModel(this BaoCaoBaoHanhSanPham entity,
        List<Attachment>? danhSachTepDinhKem = null) =>
        new() {
            Id = entity.Id,
            BuocId = entity.BuocId,
            DuAnId = entity.DuAnId,
            Ngay = entity.Ngay,
            NoiDung = entity.NoiDung,
            NguoiBaoCaoId = long.Parse(entity.CreatedBy),
            LanhDaoPhuTrachId = entity.LanhDaoPhuTrachId,
            DanhSachTepDinhKem = danhSachTepDinhKem?
                // .Where(o => o.GroupType == nameof(EGroupType.BaoCaoBaoHanhSanPham))
                .Select(o => o.ToModel()).ToList()
        };


    public static BaoCaoBaoHanhSanPham ToEntity(this BaoCaoBaoHanhSanPhamModel model)
        => new() {
            Id = model.GetId(),
            BuocId = model.BuocId == 0 ? null : model.BuocId,
            DuAnId = model.DuAnId,
            Ngay = model.Ngay,
            NoiDung = model.NoiDung,
            LanhDaoPhuTrachId = model.LanhDaoPhuTrachId,
        };

    public static void Update(this BaoCaoBaoHanhSanPham entity, BaoCaoBaoHanhSanPhamModel model) {
        entity.BuocId = model.BuocId == 0 ? null : model.BuocId;
        entity.DuAnId = model.DuAnId;
        entity.Ngay = model.Ngay;
        entity.NoiDung = model.NoiDung;
        entity.LanhDaoPhuTrachId = model.LanhDaoPhuTrachId;
    }
}
