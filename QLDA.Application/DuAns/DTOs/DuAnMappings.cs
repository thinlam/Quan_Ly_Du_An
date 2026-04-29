using QLDA.Application.DuAns.DTOs;
using QLDA.Application.DuToans.DTOs;
using QLDA.Application.KeHoachVons.DTOs;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Enums;

namespace QLDA.Application.DuAns.DTOs;

public static class DuAnMappings {
    public static DuAn ToEntity(this DuAnInsertDto dto) {
        var id = GuidExtensions.GetSequentialGuidId();
        var entity = new DuAn {
            Id = id,
            TenDuAn = dto.TenDuAn,
            QuyTrinhId = dto.QuyTrinhId,
            DiaDiem = dto.DiaDiem,
            ChuDauTuId = dto.ChuDauTuId,
            ThoiGianKhoiCong = dto.ThoiGianKhoiCong,
            ThoiGianHoanThanh = dto.ThoiGianHoanThanh,
            MaDuAn = dto.MaDuAn,
            MaNganSach = dto.MaNganSach,
            DuAnTrongDiem = dto.DuAnTrongDiem,
            LinhVucId = dto.LinhVucId,
            NhomDuAnId = dto.NhomDuAnId,
            NangLucThietKe = dto.NangLucThietKe,
            QuyMoDuAn = dto.QuyMoDuAn,
            HinhThucQuanLyDuAnId = dto.HinhThucQuanLyDuAnId,
            HinhThucDauTuId = dto.HinhThucDauTuId,
            LoaiDuAnId = dto.LoaiDuAnId,
            TongMucDauTu = dto.TongMucDauTu,
            ParentId = dto.ParentId,
            LoaiDuAnTheoNamId = dto.LoaiDuAnTheoNamId,
            TrangThaiDuAnId = dto.TrangThaiDuAnId,
            GhiChu = dto.GhiChu,
            NgayBatDau = dto.NgayBatDau,
            LanhDaoPhuTrachId = dto.LanhDaoPhuTrachId,
            DonViPhuTrachChinhId = dto.DonViPhuTrachChinhId,
            KhaiToanKinhPhi = dto.KhaiToanKinhPhi,
            DuAnNguonVons = [..dto.DanhSachNguonVon?.Select(nguonVonId => new DuAnNguonVon {
                LeftId = id,
                RightId = nguonVonId
            }) ?? []],
            DuAnChiuTrachNhiemXuLys = [..dto.DonViPhoiHopIds?.Select(phoiHopId => new DuAnChiuTrachNhiemXuLy {
                LeftId = id,
                RightId = phoiHopId,
                Loai = EChiuTrachNhiemXuLy.DonViPhoiHop
            }) ?? []],
            KeHoachVons = [.. dto.KeHoachVons?.Select(e => e.ToEntity(id)) ?? []]
        };

        // Set DuAnId for each DuToan entity
        if (entity.DuToans != null && entity.DuToans.Count > 0) {
            foreach (var duToan in entity.DuToans) {
                duToan.DuAnId = id;
            }
        }
        return entity;
    }

    public static void Update(this DuAn entity, DuAnUpdateModel dto) {
        entity.Id = dto.Id;
        entity.TenDuAn = dto.TenDuAn;
        entity.QuyTrinhId = dto.QuyTrinhId;//PHẢI CLONE LẠI BƯỚC
        entity.NgayBatDau = dto.NgayBatDau;
        entity.DiaDiem = dto.DiaDiem;
        entity.ChuDauTuId = dto.ChuDauTuId;
        entity.ThoiGianKhoiCong = dto.ThoiGianKhoiCong;
        entity.ThoiGianHoanThanh = dto.ThoiGianHoanThanh;
        entity.MaDuAn = dto.MaDuAn;
        entity.MaNganSach = dto.MaNganSach;
        entity.DuAnTrongDiem = dto.DuAnTrongDiem;
        entity.LinhVucId = dto.LinhVucId;
        entity.NhomDuAnId = dto.NhomDuAnId;
        entity.NangLucThietKe = dto.NangLucThietKe;
        entity.QuyMoDuAn = dto.QuyMoDuAn;
        entity.HinhThucQuanLyDuAnId = dto.HinhThucQuanLyDuAnId;
        entity.HinhThucDauTuId = dto.HinhThucDauTuId;
        entity.LoaiDuAnId = dto.LoaiDuAnId;
        entity.TongMucDauTu = dto.TongMucDauTu;
        entity.ParentId = dto.ParentId;
        entity.LoaiDuAnTheoNamId = dto.LoaiDuAnTheoNamId;
        entity.TrangThaiDuAnId = dto.TrangThaiDuAnId;
        entity.GhiChu = dto.GhiChu;
        entity.LanhDaoPhuTrachId = dto.LanhDaoPhuTrachId;
        entity.DonViPhuTrachChinhId = dto.DonViPhuTrachChinhId;
        entity.KhaiToanKinhPhi = dto.KhaiToanKinhPhi;
        entity.DuAnNguonVons = [.. dto.DanhSachNguonVon?.Select(nguonVonId => new DuAnNguonVon {
            LeftId = dto.Id,
            RightId = nguonVonId
        }) ?? []];
        entity.DuAnChiuTrachNhiemXuLys = [..dto.DonViPhoiHopIds?.Select(phoiHopId => new DuAnChiuTrachNhiemXuLy {
            LeftId = dto.Id,
            RightId = phoiHopId,
            Loai = EChiuTrachNhiemXuLy.DonViPhoiHop
        }) ?? []];
        // Note: KeHoachVons NOT updated here - handled by SyncHelper.SyncCollection in DuAnUpdateCommand
    }

    public static DuAnDto ToDto(this DuAn entity) {
        var dto = new DuAnDto {
            Id = entity.Id,
            TenDuAn = entity.TenDuAn,
            QuyTrinhId = entity.QuyTrinhId,
            DiaDiem = entity.DiaDiem,
            ChuDauTuId = entity.ChuDauTuId,
            ThoiGianKhoiCong = entity.ThoiGianKhoiCong,
            ThoiGianHoanThanh = entity.ThoiGianHoanThanh,
            MaDuAn = entity.MaDuAn,
            MaNganSach = entity.MaNganSach,
            DuAnTrongDiem = entity.DuAnTrongDiem,
            LinhVucId = entity.LinhVucId,
            NhomDuAnId = entity.NhomDuAnId,
            NangLucThietKe = entity.NangLucThietKe,
            QuyMoDuAn = entity.QuyMoDuAn,
            HinhThucQuanLyDuAnId = entity.HinhThucQuanLyDuAnId,
            HinhThucDauTuId = entity.HinhThucDauTuId,
            LoaiDuAnId = entity.LoaiDuAnId,
            TongMucDauTu = entity.TongMucDauTu,
            ParentId = entity.ParentId,
            LoaiDuAnTheoNamId = entity.LoaiDuAnTheoNamId,
            TrangThaiDuAnId = entity.TrangThaiDuAnId,
            GhiChu = entity.GhiChu,
            NgayBatDau = entity.NgayBatDau,
            LanhDaoPhuTrachId = entity.LanhDaoPhuTrachId,
            DonViPhuTrachChinhId = entity.DonViPhuTrachChinhId,
            KhaiToanKinhPhi = entity.KhaiToanKinhPhi,
            DanhSachNguonVon = [.. entity.DuAnNguonVons?.Select(nguonVon => nguonVon.RightId) ?? []],
            DonViPhoiHopIds = [.. entity.DuAnChiuTrachNhiemXuLys?.Where(e => e.Loai == EChiuTrachNhiemXuLy.DonViPhoiHop).Select(chiuTrachNhiemXuLy => chiuTrachNhiemXuLy.RightId) ?? []],
            DuToans = [.. entity.DuToans?.Where(e => !e.IsDeleted)
                .OrderBy(e => e.Index)
                .Select(e => e.ToDto()) ?? []],
            KeHoachVons = [.. entity.KeHoachVons?.Where(e => !e.IsDeleted)
                .Select(e => e.ToDto()) ?? []]
        };

        return dto;
    }
    public static DuAnDto ToDto(this DuAn entity, List<TepDinhKem>? files = null) {
        var dto = entity.ToDto();
        if (dto.DuToans != null && dto.DuToans.Count != 0 && files != null && files.Count != 0) {
            foreach (var duToan in dto.DuToans) {
                duToan.DanhSachTepDinhKem = files.Where(f => f.GroupId == duToan.Id.ToString()).ToDtos();
            }
        }
        return dto;
    }
}