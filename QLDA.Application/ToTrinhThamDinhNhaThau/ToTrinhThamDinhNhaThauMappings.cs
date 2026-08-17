using QLDA.Application.ToTrinhThamDinhNhaThaus.DTOs;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Constants;
using QLDA.Domain.Enums;

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

    /// <summary>
    /// Map chi-tiet (GET {id}/chi-tiet) — bổ sung GoiThauId/ThongTinNhaThau/ToTrinhKetQua/QuyetDinhPheDuyet (Issue #179).
    /// Không sửa <see cref="ToDto(ToTrinhThamDinhNhaThau,List{Attachment},List{Attachment},List{TepDinhKemDto},List{TepDinhKemDto},List{TepDinhKemDto})"/>
    /// (dùng cho list/PUT) để tránh đổi shape list của dev D.
    /// </summary>
    public static ToTrinhThamDinhNhaThauChiTietDto ToChiTietDto(
        this ToTrinhThamDinhNhaThau entity,
        List<TepDinhKemDto>? danhSachTepDinhKem = null,
        List<TepDinhKemDto>? danhSachTepThamDinh = null,
        List<TepDinhKemDto>? filesDoiChieu = null,
        List<TepDinhKemDto>? filesThuongThao = null,
        List<TepDinhKemDto>? filesThamDinh = null,
        List<TepDinhKemDto>? fileEHSDT = null,
        List<TepDinhKemDto>? fileDanhGia = null,
        ToTrinhQuyetDinh? toTrinhKetQua = null,
        List<TepDinhKemDto>? filesToTrinhKetQua = null,
        VanBanQuyetDinh? quyetDinh = null,
        List<TepDinhKemDto>? filesQuyetDinh = null) =>
        new() {
            Id = entity.Id,
            DuAnId = entity.DuAnId,
            BuocId = entity.BuocId,
            GoiThauId = entity.GoiThauId,
            NhaThauId = entity.NhaThauId,
            TrangThaiDangTaiId = entity.TrangThaiDangTaiId,
            DanhSachTepDinhKem = danhSachTepDinhKem,
            DanhSachTepThamDinh = danhSachTepThamDinh,
            ThongTinNhaThau = new ThongTinNhaThauDto {
                NhaThauId = entity.NhaThauId,
                NgayKetThucDanhGia = entity.NgayKetThucDanhGia,
                FileEHSDT = fileEHSDT,
                FileDanhGia = fileDanhGia,
            },
            DoiChieu = entity.BuocXuLys?.FirstOrDefault(x => x.Loai == ToTrinhThamDinhBuocXuLyLoai.DoiChieu)?.ToDto(filesDoiChieu),
            ThuongThao = entity.BuocXuLys?.FirstOrDefault(x => x.Loai == ToTrinhThamDinhBuocXuLyLoai.ThuongThao)?.ToDto(filesThuongThao),
            ThamDinh = entity.BuocXuLys?.FirstOrDefault(x => x.Loai == ToTrinhThamDinhBuocXuLyLoai.ThamDinh)?.ToDto(filesThamDinh),
            ToTrinhKetQua = toTrinhKetQua == null ? null : new ToTrinhKetQuaDto {
                So = toTrinhKetQua.So,
                Ngay = toTrinhKetQua.Ngay,
                NguoiKy = toTrinhKetQua.NguoiKy,
                ChucVuId = toTrinhKetQua.ChucVu,
                TrichYeu = toTrinhKetQua.TrichYeu,
                File = filesToTrinhKetQua,
            },
            QuyetDinhPheDuyet = quyetDinh == null ? null : new QuyetDinhPheDuyetDto {
                So = quyetDinh.So,
                Ngay = quyetDinh.Ngay,
                NguoiKy = quyetDinh.NguoiKy,
                NgayKy = quyetDinh.NgayKy,
                ChucVuId = quyetDinh.NguoiKyChucVuId,
                TrichYeu = quyetDinh.TrichYeu,
                File = filesQuyetDinh,
            },
        };
}
