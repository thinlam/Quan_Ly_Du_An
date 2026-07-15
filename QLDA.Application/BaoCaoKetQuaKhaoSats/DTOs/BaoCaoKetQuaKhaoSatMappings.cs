using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Constants;

namespace QLDA.Application.BaoCaoKetQuaKhaoSats.DTOs;

public static class BaoCaoKetQuaKhaoSatMappings
{
    public static BaoCaoKetQuaKhaoSat ToEntity(this BaoCaoKetQuaKhaoSatInsertDto dto) => new()
    {
        Id = Guid.NewGuid(),
        DuAnId = dto.DuAnId,
        BuocId = dto.BuocId,
        NoiDungBaoCao = dto.NoiDungBaoCao,
        NoiDungNghiemThu = dto.NoiDungNghiemThu,
        NgayKhaoSat = dto.NgayKhaoSat.ToStartOfDayUtc(),
    };

    public static void Update(this BaoCaoKetQuaKhaoSat entity, BaoCaoKetQuaKhaoSatUpdateModel model)
    {
        entity.NoiDungBaoCao = model.NoiDungBaoCao;
        entity.NoiDungNghiemThu = model.NoiDungNghiemThu;
        entity.NgayKhaoSat = model.NgayKhaoSat.ToStartOfDayUtc();
    }

    public static BaoCaoKetQuaKhaoSatDto ToDto(this BaoCaoKetQuaKhaoSat entity, List<Attachment>? files) => new() {
        Id = entity.Id,
        DuAnId = entity.DuAnId,
        BuocId = entity.BuocId,
        NoiDungBaoCao = entity.NoiDungBaoCao,
        NoiDungNghiemThu = entity.NoiDungNghiemThu,
        NgayKhaoSat = entity.NgayKhaoSat.ToDateOnlyVn(),
        TrangThaiId = entity.TrangThaiId,
        TenTrangThai = entity.TrangThaiId == null
            ? TrangThaiPheDuyetCodes.Default.TenDuThao
            : entity.TrangThai?.Ten,
        DanhSachTepDinhKem = files?.Select(f => new TepDinhKemDto
        {
            Id = f.Id,
            ParentId = f.ParentId,
            GroupId = f.GroupId,
            GroupType = f.GroupType,
            FileName = f.FileName,
            OriginalName = f.OriginalName,
            Path = f.Path,
            Size = f.Size,
            Type = f.Type,

        }).ToList(),
        //NgayTrinh = entity.NgayTrinh.ToDateOnlyVn(),
    };
}
