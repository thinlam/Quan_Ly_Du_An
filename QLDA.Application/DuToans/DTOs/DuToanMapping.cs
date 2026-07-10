using QLDA.Application.Common;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Enums;

namespace QLDA.Application.DuToans.DTOs;

public static class DuToanMapping {
    public static (DuToan, List<TepDinhKem>) ToEntity(this DuToanInsertModel model, Guid duAnId) {
        var id = GuidExtensions.GetSequentialGuidId();
        var duToan = new DuToan {
            Id = id,
            DuAnId = duAnId,
            SoDuToan = model.SoDuToan,
            NamDuToan = model.NamDuToan,
            SoQuyetDinhDuToan = model.SoQuyetDinhDuToan,
            NgayKyDuToan = model.NgayKyDuToan,
            GhiChu = model.GhiChu,
        };
        var tepDinhKems = model.DanhSachTepDinhKem?.ToEntities(
            groupId: id,
            groupType: EGroupType.DuToan
        ) ?? [];
        return (duToan, [.. tepDinhKems]);
    }
    public static DuToan ToEntity(this DuToanUpdateModel model, Guid duAnId) {
        return new DuToan {
            Id = model.Id.GetId(),
            DuAnId = duAnId,
            SoDuToan = model.SoDuToan,
            NamDuToan = model.NamDuToan,
            SoQuyetDinhDuToan = model.SoQuyetDinhDuToan,
            NgayKyDuToan = model.NgayKyDuToan,
            GhiChu = model.GhiChu,
        };
    }

    public static (DuToan, List<TepDinhKem>) ToEntityWithFiles(this DuToanUpdateModel model, Guid duAnId) {
        var duToan = model.ToEntity(duAnId);
        var tepDinhKems = model.DanhSachTepDinhKem?.ToEntities(
            groupId: duToan.Id,
            groupType: EGroupType.DuToan
        ) ?? [];
        return (duToan, [.. tepDinhKems]);
    }
    public static DuToanDto ToDto(this DuToan entity) {
        return new DuToanDto {
            Id = entity.Id,
            DuAnId = entity.DuAnId,
            SoDuToan = entity.SoDuToan,
            NamDuToan = entity.NamDuToan,
            SoQuyetDinhDuToan = entity.SoQuyetDinhDuToan,
            NgayKyDuToan = entity.NgayKyDuToan,
            GhiChu = entity.GhiChu,
        };
    }
}
