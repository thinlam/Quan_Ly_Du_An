using QLDA.Application.Common.Enums;

namespace QLDA.Application.Common;

public static class DanhMucMappings {
    // For int-based categories
    public static object ToEntity(this DanhMucInsertDto dto, EDanhMuc danhMucType) {
        return danhMucType switch {
            EDanhMuc.DanhMucLinhVuc => new DanhMucLinhVuc {
                Ma = dto.Ma,
                Ten = dto.Ten,
                MoTa = dto.MoTa,
                Stt = dto.Stt,
                Used = dto.Used
            },
            EDanhMuc.DanhMucLoaiDuAn => new DanhMucLoaiDuAn {
                Ma = dto.Ma,
                Ten = dto.Ten,
                MoTa = dto.MoTa,
                Stt = dto.Stt,
                Used = dto.Used
            },
            EDanhMuc.DanhMucNguonVon => new DanhMucNguonVon {
                Ma = dto.Ma,
                Ten = dto.Ten,
                MoTa = dto.MoTa,
                Stt = dto.Stt,
                Used = dto.Used
            },
            EDanhMuc.DanhMucNhomDuAn => new DanhMucNhomDuAn {
                Ma = dto.Ma,
                Ten = dto.Ten,
                MoTa = dto.MoTa,
                Stt = dto.Stt,
                Used = dto.Used
            },
            EDanhMuc.DanhMucQuyTrinh => new DanhMucQuyTrinh {
                Ma = dto.Ma,
                Ten = dto.Ten,
                MoTa = dto.MoTa,
                Stt = dto.Stt,
                Used = dto.Used
            },
            EDanhMuc.DanhMucTrangThaiDuAn => new DanhMucTrangThaiDuAn {
                Ma = dto.Ma,
                Ten = dto.Ten,
                MoTa = dto.MoTa,
                Stt = dto.Stt,
                Used = dto.Used
            },
            EDanhMuc.DanhMucTrangThaiTienDo => new DanhMucTrangThaiTienDo {
                Ma = dto.Ma,
                Ten = dto.Ten,
                MoTa = dto.MoTa,
                Stt = dto.Stt,
                Used = dto.Used
            },
            EDanhMuc.DanhMucLoaiVanBan => new DanhMucLoaiVanBan {
                Ma = dto.Ma,
                Ten = dto.Ten,
                MoTa = dto.MoTa,
                Stt = dto.Stt,
                Used = dto.Used
            },
            EDanhMuc.DanhMucChucVu => new DanhMucChucVu {
                Ma = dto.Ma,
                Ten = dto.Ten,
                MoTa = dto.MoTa,
                Stt = dto.Stt,
                Used = dto.Used
            },
            EDanhMuc.DanhMucLoaiHopDong => new DanhMucLoaiHopDong {
                Ma = dto.Ma,
                Ten = dto.Ten,
                MoTa = dto.MoTa,
                Stt = dto.Stt,
                Used = dto.Used
            },
            EDanhMuc.DanhMucLoaiGoiThau => new DanhMucLoaiGoiThau {
                Ma = dto.Ma,
                Ten = dto.Ten,
                MoTa = dto.MoTa,
                Stt = dto.Stt,
                Used = dto.Used
            },
            EDanhMuc.DanhMucPhuongThucLuaChonNhaThau => new DanhMucPhuongThucLuaChonNhaThau {
                Ma = dto.Ma,
                Ten = dto.Ten,
                MoTa = dto.MoTa,
                Stt = dto.Stt,
                Used = dto.Used
            },
            EDanhMuc.DanhMucHinhThucLuaChonNhaThau => new DanhMucHinhThucLuaChonNhaThau {
                Ma = dto.Ma,
                Ten = dto.Ten,
                MoTa = dto.MoTa,
                Stt = dto.Stt,
                Used = dto.Used
            },
            EDanhMuc.DanhMucTinhTrangKhoKhan => new DanhMucTinhTrangKhoKhan {
                Ma = dto.Ma,
                Ten = dto.Ten,
                MoTa = dto.MoTa,
                Stt = dto.Stt,
                Used = dto.Used
            },
            EDanhMuc.DanhMucGiaiDoan => new DanhMucGiaiDoan {
                Ma = dto.Ma,
                Ten = dto.Ten,
                MoTa = dto.MoTa,
                Stt = dto.Stt,
                Used = dto.Used
            },
            EDanhMuc.DanhMucMucDoKhoKhan => new DanhMucMucDoKhoKhan {
                Ma = dto.Ma,
                Ten = dto.Ten,
                MoTa = dto.MoTa,
                Stt = dto.Stt,
                Used = dto.Used
            },
            EDanhMuc.DanhMucChuDauTu => new DanhMucChuDauTu {
                Ma = dto.Ma,
                Ten = dto.Ten,
                MoTa = dto.MoTa,
                Stt = dto.Stt,
                Used = dto.Used
            },
            EDanhMuc.DanhMucHinhThucDauTu => new DanhMucHinhThucDauTu {
                Ma = dto.Ma,
                Ten = dto.Ten,
                MoTa = dto.MoTa,
                Stt = dto.Stt,
                Used = dto.Used
            },
            EDanhMuc.DanhMucHinhThucQuanLy => new DanhMucHinhThucQuanLy {
                Ma = dto.Ma,
                Ten = dto.Ten,
                MoTa = dto.MoTa,
                Stt = dto.Stt,
                Used = dto.Used
            },
            EDanhMuc.DanhMucLoaiDuAnTheoNam => new DanhMucLoaiDuAnTheoNam {
                Ma = dto.Ma,
                Ten = dto.Ten,
                MoTa = dto.MoTa,
                Stt = dto.Stt,
                Used = dto.Used
            },

            EDanhMuc.DanhMucPhuongThucKySo => new DanhMucPhuongThucKySo
            {
                Ma= dto.Ma,
                Ten = dto.Ten,
                MoTa = dto.MoTa,
                Stt = dto.Stt,
                Used = dto.Used
            },
            _ => throw new ArgumentException($"Unsupported int-based DanhMuc type: {danhMucType}")
        };
    }

    // For Guid-based categories
    public static object ToEntity(this DanhMucGuidInsertDto dto, EDanhMuc danhMucType) {
        return danhMucType switch {
            EDanhMuc.DanhMucNhaThau => new DanhMucNhaThau {
                Id = dto.Id.GetId(),
                Ma = dto.Ma,
                Ten = dto.Ten,
                MoTa = dto.MoTa,
                Stt = dto.Stt,
                Used = dto.Used
            },
            _ => throw new ArgumentException($"Unsupported Guid-based DanhMuc type: {danhMucType}")
        };
    }

    // For Guid-based categories update
    public static object ToEntity(this DanhMucGuidUpdateDto dto, EDanhMuc danhMucType) {
        return danhMucType switch {
            EDanhMuc.DanhMucNhaThau => new DanhMucNhaThau {
                Id = dto.Id,
                Ma = dto.Ma,
                Ten = dto.Ten,
                MoTa = dto.MoTa,
                Stt = dto.Stt,
                Used = dto.Used
            },
            _ => throw new ArgumentException($"Unsupported Guid-based DanhMuc type: {danhMucType}")
        };
    }

    public static object ToEntity(this DanhMucUpdateDto dto, EDanhMuc danhMucType) {

        return danhMucType switch {
            EDanhMuc.DanhMucLinhVuc => new DanhMucLinhVuc {
                Id = dto.Id,
                Ma = dto.Ma,
                Ten = dto.Ten,
                MoTa = dto.MoTa,
                Stt = dto.Stt,
                Used = dto.Used
            },
            EDanhMuc.DanhMucLoaiDuAn => new DanhMucLoaiDuAn {
                Id = dto.Id,
                Ma = dto.Ma,
                Ten = dto.Ten,
                MoTa = dto.MoTa,
                Stt = dto.Stt,
                Used = dto.Used
            },
            EDanhMuc.DanhMucNguonVon => new DanhMucNguonVon {
                Id = dto.Id,
                Ma = dto.Ma,
                Ten = dto.Ten,
                MoTa = dto.MoTa,
                Stt = dto.Stt,
                Used = dto.Used
            },
            EDanhMuc.DanhMucNhomDuAn => new DanhMucNhomDuAn {
                Id = dto.Id,
                Ma = dto.Ma,
                Ten = dto.Ten,
                MoTa = dto.MoTa,
                Stt = dto.Stt,
                Used = dto.Used
            },
            EDanhMuc.DanhMucQuyTrinh => new DanhMucQuyTrinh {
                Id = dto.Id,
                Ma = dto.Ma,
                Ten = dto.Ten,
                MoTa = dto.MoTa,
                Stt = dto.Stt,
                Used = dto.Used
            },
            EDanhMuc.DanhMucTrangThaiDuAn => new DanhMucTrangThaiDuAn {
                Id = dto.Id,
                Ma = dto.Ma,
                Ten = dto.Ten,
                MoTa = dto.MoTa,
                Stt = dto.Stt,
                Used = dto.Used
            },
            EDanhMuc.DanhMucTrangThaiTienDo => new DanhMucTrangThaiTienDo {
                Id = dto.Id,
                Ma = dto.Ma,
                Ten = dto.Ten,
                MoTa = dto.MoTa,
                Stt = dto.Stt,
                Used = dto.Used
            },
            EDanhMuc.DanhMucLoaiVanBan => new DanhMucLoaiVanBan {
                Id = dto.Id,
                Ma = dto.Ma,
                Ten = dto.Ten,
                MoTa = dto.MoTa,
                Stt = dto.Stt,
                Used = dto.Used
            },
            EDanhMuc.DanhMucChucVu => new DanhMucChucVu {
                Id = dto.Id,
                Ma = dto.Ma,
                Ten = dto.Ten,
                MoTa = dto.MoTa,
                Stt = dto.Stt,
                Used = dto.Used
            },
            EDanhMuc.DanhMucLoaiHopDong => new DanhMucLoaiHopDong {
                Id = dto.Id,
                Ma = dto.Ma,
                Ten = dto.Ten,
                MoTa = dto.MoTa,
                Stt = dto.Stt,
                Used = dto.Used
            },
            EDanhMuc.DanhMucLoaiGoiThau => new DanhMucLoaiGoiThau {
                Id = dto.Id,
                Ma = dto.Ma,
                Ten = dto.Ten,
                MoTa = dto.MoTa,
                Stt = dto.Stt,
                Used = dto.Used
            },
            EDanhMuc.DanhMucPhuongThucLuaChonNhaThau => new DanhMucPhuongThucLuaChonNhaThau {
                Id = dto.Id,
                Ma = dto.Ma,
                Ten = dto.Ten,
                MoTa = dto.MoTa,
                Stt = dto.Stt,
                Used = dto.Used
            },
            EDanhMuc.DanhMucHinhThucLuaChonNhaThau => new DanhMucHinhThucLuaChonNhaThau {
                Id = dto.Id,
                Ma = dto.Ma,
                Ten = dto.Ten,
                MoTa = dto.MoTa,
                Stt = dto.Stt,
                Used = dto.Used
            },
            EDanhMuc.DanhMucTinhTrangKhoKhan => new DanhMucTinhTrangKhoKhan {
                Id = dto.Id,
                Ma = dto.Ma,
                Ten = dto.Ten,
                MoTa = dto.MoTa,
                Stt = dto.Stt,
                Used = dto.Used
            },
            EDanhMuc.DanhMucGiaiDoan => new DanhMucGiaiDoan {
                Id = dto.Id,
                Ma = dto.Ma,
                Ten = dto.Ten,
                MoTa = dto.MoTa,
                Stt = dto.Stt,
                Used = dto.Used
            },
            EDanhMuc.DanhMucMucDoKhoKhan => new DanhMucMucDoKhoKhan {
                Id = dto.Id,
                Ma = dto.Ma,
                Ten = dto.Ten,
                MoTa = dto.MoTa,
                Stt = dto.Stt,
                Used = dto.Used
            },
            EDanhMuc.DanhMucChuDauTu => new DanhMucChuDauTu {
                Id = dto.Id,
                Ma = dto.Ma,
                Ten = dto.Ten,
                MoTa = dto.MoTa,
                Stt = dto.Stt,
                Used = dto.Used
            },
            EDanhMuc.DanhMucHinhThucDauTu => new DanhMucHinhThucDauTu {
                Id = dto.Id,
                Ma = dto.Ma,
                Ten = dto.Ten,
                MoTa = dto.MoTa,
                Stt = dto.Stt,
                Used = dto.Used
            },
            EDanhMuc.DanhMucHinhThucQuanLy => new DanhMucHinhThucQuanLy {
                Id = dto.Id,
                Ma = dto.Ma,
                Ten = dto.Ten,
                MoTa = dto.MoTa,
                Stt = dto.Stt,
                Used = dto.Used
            },
            EDanhMuc.DanhMucLoaiDuAnTheoNam => new DanhMucLoaiDuAnTheoNam {
                Id = dto.Id,
                Ma = dto.Ma,
                Ten = dto.Ten,
                MoTa = dto.MoTa,
                Stt = dto.Stt,
                Used = dto.Used
            },
            EDanhMuc.DanhMucPhuongThucKySo => new DanhMucPhuongThucKySo
            {
                Id = dto.Id,
                Ma= dto.Ma,
                Ten = dto.Ten,
                MoTa = dto.MoTa,
                Stt = dto.Stt,
                Used = dto.Used
            },
            _ => throw new ArgumentException($"Unsupported DanhMuc type: {danhMucType}")
        };
    }
}