using QLDA.WebApi.Models.QuyetDinhDuyetDuAnHangMucs;
using BuildingBlocks.Domain.Entities;
using QLDA.WebApi.Models.QuyetDinhDuyetDuAnNguonVons;
using QLDA.WebApi.Models.TepDinhKems;
using SequentialGuid;

namespace QLDA.WebApi.Models.QuyetDinhDuyetDuAns;

public static class QuyetDinhDuyetDuAnMappingConfiguration {
    public static QuyetDinhDuyetDuAnModel ToModel(this QuyetDinhDuyetDuAn entity,
        List<Attachment>? danhSachTepDinhKem = null) =>
        new() {
            Id = entity.Id,
            DuAnId = entity.DuAnId,
            BuocId = entity.BuocId,
            SoQuyetDinh = entity.So,
            NgayQuyetDinh = entity.Ngay,
            TrichYeu = entity.TrichYeu,
            CoQuanQuyetDinhDauTu = entity.CoQuanQuyetDinhDauTu,
            HangMucs = entity.QuyetDinhDuyetDuAnNguonVons?
                .SelectMany(nv => nv.QuyetDinhDuyetDuAnHangMucs!.Select(e => new QuyetDinhDuyetDuAnHangMucModel() {
                    Id = e.Id,
                    QuyetDinhDuyetDuAnNguonVonId = nv.Id,
                    NguonVonId = nv.NguonVonId,
                    TenHangMuc = e.TenHangMuc,
                    QuyMoHangMuc = e.QuyMoHangMuc,
                    TongMucDauTu = e.TongMucDauTu,
                })).ToList(),
            DanhSachTepDinhKem = danhSachTepDinhKem?
                // .Where(o => o.GroupType == nameof(EGroupType.QuyetDinhDuyetDuAn))
                .Select(o => o.ToModel()).ToList()
        };


    public static QuyetDinhDuyetDuAn ToEntity(this QuyetDinhDuyetDuAnModel model) {
        var entity = new QuyetDinhDuyetDuAn() {
            Id = model.GetId(),
            DuAnId = model.DuAnId,
            BuocId = model.BuocId,
            So = model.SoQuyetDinh,
            Ngay = model.NgayQuyetDinh,
            TrichYeu = model.TrichYeu,
            CoQuanQuyetDinhDauTu = model.CoQuanQuyetDinhDauTu,
        };

        entity.QuyetDinhDuyetDuAnNguonVons = model.HangMucs?.ToNguonVons().ToList();

        return entity;
    }

    private static IEnumerable<QuyetDinhDuyetDuAnNguonVon> ToNguonVons(
        this List<QuyetDinhDuyetDuAnHangMucModel> hangMucs) {
        if (hangMucs.Count == 0)
            yield break;

        // Nhóm theo NguonVonId
        foreach (var group in hangMucs.GroupBy(hm => hm.NguonVonId )) {
            //Nếu chưa có qd duyệt dự án nguồn vốn id => tạo mới nguồn vốn
            var existId = group
                .Where(e => e.NguonVonId == group.Key)
                .Select(e => e.QuyetDinhDuyetDuAnNguonVonId)
                .FirstOrDefault();
            // Tạo Guid mới cho mỗi nhóm
            var newId = SequentialGuidGenerator.Instance.NewGuid();

            // Tính tổng giá trị đầu tư của nhóm
            var tongGiaTri = group.Sum(hm => hm.TongMucDauTu);
            var lstEntities = group
                .Select(hm => hm.ToEntity(newId, existId))
                .ToList();

            // Trả về đối tượng kết quả cho nhóm này
            yield return new QuyetDinhDuyetDuAnNguonVon {
                Id = existId ?? newId,
                NguonVonId = group.Key,
                GiaTri = tongGiaTri,
                QuyetDinhDuyetDuAnHangMucs = lstEntities
            };
        }
    }


    public static void Update(this QuyetDinhDuyetDuAn entity, QuyetDinhDuyetDuAnModel model) {
        entity.DuAnId = model.DuAnId;
        entity.BuocId = model.BuocId;
        entity.So = model.SoQuyetDinh;
        entity.Ngay = model.NgayQuyetDinh;
        entity.TrichYeu = model.TrichYeu;
        entity.CoQuanQuyetDinhDauTu = model.CoQuanQuyetDinhDauTu;
        entity.QuyetDinhDuyetDuAnNguonVons = model.HangMucs?.ToNguonVons().ToList();
    }
}
