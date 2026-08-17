using QLDA.Application.ToTrinhThamDinhNhaThaus.DTOs;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Constants;

namespace QLDA.Application.ToTrinhThamDinhNhaThaus;

public static class ToTrinhThamDinhNhaThauMappings
{
    /// <summary>
    /// Dựng danh sách <see cref="ToTrinhThamDinhBuocXuLy"/> tạm (chưa gắn ToTrinhId/Id) từ 3 object
    /// riêng (Đối chiếu/Thương thảo/Thẩm định) — dùng chung cho cả Create và Update (Issue #179).
    /// </summary>
    public static List<ToTrinhThamDinhBuocXuLy> ToBuocXuLyList(
        ToTrinhThamDinhBuocXuLyDto? doiChieu,
        ToTrinhThamDinhBuocXuLyDto? thuongThao,
        ToTrinhThamDinhBuocXuLyDto? thamDinh) {
        var list = new List<ToTrinhThamDinhBuocXuLy>();
        if (doiChieu != null)
            list.Add(new() { So = doiChieu.So, Ngay = doiChieu.Ngay, NoiDung = doiChieu.NoiDung, Loai = ToTrinhThamDinhBuocXuLyLoai.DoiChieu });
        if (thuongThao != null)
            list.Add(new() { So = thuongThao.So, Ngay = thuongThao.Ngay, NoiDung = thuongThao.NoiDung, Loai = ToTrinhThamDinhBuocXuLyLoai.ThuongThao });
        if (thamDinh != null)
            list.Add(new() { So = thamDinh.So, Ngay = thamDinh.Ngay, NoiDung = thamDinh.NoiDung, Loai = ToTrinhThamDinhBuocXuLyLoai.ThamDinh });
        return list;
    }

    /// <summary>
    /// Sync 3 bước xử lý (Đối chiếu/Thương thảo/Thẩm định) vào entity đang tracked —
    /// insert/update đúng từng bước theo Loai, không tạo duplicate, không đụng bước khác (Issue #179).
    /// Dùng chung cho cả Create (entity.BuocXuLys rỗng) và Update (entity.BuocXuLys đã Include).
    /// </summary>
    public static void SyncBuocXuLys(this ToTrinhThamDinhNhaThau entity, List<ToTrinhThamDinhBuocXuLy> incoming) {
        entity.BuocXuLys ??= [];
        foreach (var loai in new[] { ToTrinhThamDinhBuocXuLyLoai.DoiChieu, ToTrinhThamDinhBuocXuLyLoai.ThuongThao, ToTrinhThamDinhBuocXuLyLoai.ThamDinh }) {
            var incomingItem = incoming.FirstOrDefault(x => x.Loai == loai);
            var existingItem = entity.BuocXuLys.FirstOrDefault(x => x.Loai == loai);

            if (incomingItem == null) {
                if (existingItem != null)
                    entity.BuocXuLys.Remove(existingItem);
                continue;
            }

            if (existingItem != null) {
                existingItem.So = incomingItem.So;
                existingItem.Ngay = incomingItem.Ngay;
                existingItem.NoiDung = incomingItem.NoiDung;
            } else {
                entity.BuocXuLys.Add(new ToTrinhThamDinhBuocXuLy {
                    ToTrinhId = entity.Id,
                    So = incomingItem.So,
                    Ngay = incomingItem.Ngay,
                    NoiDung = incomingItem.NoiDung,
                    Loai = loai,
                });
            }
        }
    }

    public static ToTrinhThamDinhBuocXuLyDto ToDto(this ToTrinhThamDinhBuocXuLy entity, List<TepDinhKemDto>? files = null) =>
        new() {
            So = entity.So,
            Ngay = entity.Ngay,
            NoiDung = entity.NoiDung,
            File = files,
        };

    public static ToTrinhThamDinhNhaThauDto ToDto(this ToTrinhThamDinhNhaThau entity, List<Attachment>? files = null, List<Attachment>? filesThamDinh = null,
        List<TepDinhKemDto>? filesDoiChieu = null, List<TepDinhKemDto>? filesThuongThao = null, List<TepDinhKemDto>? filesThamDinhBuoc = null) =>
        new() {
            Id = entity.Id,
            TrangThaiId = entity.TrangThaiId,
            TrangThaiDangTaiId = entity.TrangThaiDangTaiId,
            NhaThauId = entity.NhaThauId,
            DanhSachTepDinhKem = files?.Select(x => x.ToDto()).ToList(),
            DanhSachTepThamDinh = filesThamDinh?.Select(x => x.ToDto()).ToList(),
            DoiChieu = entity.BuocXuLys?.FirstOrDefault(x => x.Loai == ToTrinhThamDinhBuocXuLyLoai.DoiChieu)?.ToDto(filesDoiChieu),
            ThuongThao = entity.BuocXuLys?.FirstOrDefault(x => x.Loai == ToTrinhThamDinhBuocXuLyLoai.ThuongThao)?.ToDto(filesThuongThao),
            ThamDinh = entity.BuocXuLys?.FirstOrDefault(x => x.Loai == ToTrinhThamDinhBuocXuLyLoai.ThamDinh)?.ToDto(filesThamDinhBuoc),
        };
}
