using QLDA.Application.QuyetDinhDuyetDuToanDtos.DTOs;
using QLDA.Application.QuyetDinhDuyetDuToans.DTOs;

namespace QLDA.Application.QuyetDinhDuyetDuToans;

public static class QuyetDinhDuyetDuToanMappings
{
    public static QuyetDinhDuyetDuToan ToEntity(this QuyetDinhDuyetDuToanInsUpdDto dto) {
        Guid id = dto.GetId();
        var entity = new QuyetDinhDuyetDuToan {
            Id = id,
            DuAnId = dto.DuAnId,
            BuocId = dto.BuocId,
            So = dto.So,
            Ngay = dto.Ngay,
            TrichYeu = dto.TrichYeu,
            ThoiGianThucHien = dto.ThoiGianThucHien,
            GiaTri = dto.GiaTri,
            HinhThucQuanLyId = dto.HinhThucQuanLyId,
            KeHoachLuaChonNhaThauId = dto.KeHoachLuaChonNhaThauId,
        };

        if (dto.KeHoachVons != null) {
            entity.KeHoachVons = [.. dto.KeHoachVons.Select(tv => new QuyetDinhDuyetDuToanNguonVon {
                Id = tv.GetId(),
                QuyetDinhDuToanId= entity.Id,
                NguonVonId = tv.NguonVonId,
                GiaTri = tv.GiaTri??0,
                Nam = tv.Nam,
           })];
        }

        if (dto.ChiPhis != null)
        {
            entity.ChiPhis = [.. dto.ChiPhis.Select(cp => new QuyetDinhDuyetDuToanChiPhi {
                Id = cp.GetId(),
                QuyetDinhDuToanId= entity.Id,
                ChiPhi = cp.TenChiPhi,
                GiaTri = cp.GiaTri??0
           })];
        }
        return entity;
    }

    public static QuyetDinhDuyetDuToanDto ToDto(this QuyetDinhDuyetDuToan entity) {
        return new QuyetDinhDuyetDuToanDto
        {
            Id = entity.Id,
            DuAnId = entity.DuAnId,
            BuocId = entity.BuocId,
            SoQuyetDinh = entity.So,
            NgayQuyetDinh = entity.Ngay,
            TrichYeu = entity.TrichYeu,
            GiaTri = entity.GiaTri,
            ThoiGian    = entity.ThoiGianThucHien,
            TenHinhThucQuanLy = entity.HinhThucQuanLyDuAn.Ten,
            TenKeHoachLuaChonNhaThau = entity.KeHoachLuaChonNhaThau.Ten,
        };
    }
    public static void UpdateToEntity(this QuyetDinhDuyetDuToanInsUpdDto dto, QuyetDinhDuyetDuToan entity)
    {
        // 1. Cập nhật các thuộc tính cha (Master)
        entity.DuAnId = dto.DuAnId;
        entity.BuocId = dto.BuocId;
        entity.So = dto.So;
        entity.Ngay = dto.Ngay;
        entity.TrichYeu = dto.TrichYeu;
        entity.ThoiGianThucHien = dto.ThoiGianThucHien;
        entity.GiaTri = dto.GiaTri;
        entity.HinhThucQuanLyId = dto.HinhThucQuanLyId;
        entity.KeHoachLuaChonNhaThauId = dto.KeHoachLuaChonNhaThauId;

        // 2. CẬP NHẬT ĐỘNG CHI PHÍ (Thêm, Sửa, Xóa)
        if (dto.ChiPhis != null)
        {
            var incomingChiPhiIds = dto.ChiPhis.Select(cp => cp.GetId()).ToList();

            // Bước A: Xóa những chi phí cũ không còn tồn tại trong DTO gửi lên
            var chiPhisToRemove = entity.ChiPhis.Where(cp => !incomingChiPhiIds.Contains(cp.Id)).ToList();
            foreach (var rm in chiPhisToRemove)
            {
                entity.ChiPhis.Remove(rm);
            }

            // Bước B: Thêm mới hoặc Cập nhật những cái còn lại
            foreach (var cpDto in dto.ChiPhis)
            {
                var cpId = cpDto.GetId();
                var existingChiPhi = entity.ChiPhis.FirstOrDefault(cp => cp.Id == cpId);

                if (existingChiPhi != null)
                {
                    // Nếu đã có -> Cập nhật giá trị (SỬA)
                    existingChiPhi.ChiPhi = cpDto.TenChiPhi;
                    existingChiPhi.GiaTri = cpDto.GiaTri ?? 0;
                }
                else
                {
                    // Nếu chưa có -> THÊM MỚI
                    entity.ChiPhis.Add(new QuyetDinhDuyetDuToanChiPhi
                    {
                        Id = cpId,
                        QuyetDinhDuToanId = entity.Id,
                        ChiPhi = cpDto.TenChiPhi,
                        GiaTri = cpDto.GiaTri ?? 0
                    });
                }
            }
        }
        else
        {
            entity.ChiPhis.Clear(); // Nếu dto truyền lên null/trống -> Xóa sạch chi phí cũ
        }

        // 3. CẬP NHẬT ĐỘNG NGUỒN VỐN (Thêm, Sửa, Xóa tương tự Chi Phí)
        if (dto.KeHoachVons != null)
        {
            var incomingVonIds = dto.KeHoachVons.Select(v => v.GetId()).ToList();

            // Xóa nguồn vốn cũ thừa
            var vonsToRemove = entity.KeHoachVons.Where(v => !incomingVonIds.Contains(v.Id)).ToList();
            foreach (var rm in vonsToRemove)
            {
                entity.KeHoachVons.Remove(rm);
            }

            // Thêm/Sửa nguồn vốn
            foreach (var vDto in dto.KeHoachVons)
            {
                var vId = vDto.GetId();
                var existingVon = entity.KeHoachVons.FirstOrDefault(v => v.Id == vId);

                if (existingVon != null)
                {
                    existingVon.NguonVonId = vDto.NguonVonId;
                    existingVon.GiaTri = vDto.GiaTri ?? 0;
                    existingVon.Nam = vDto.Nam;
                }
                else
                {
                    entity.KeHoachVons.Add(new QuyetDinhDuyetDuToanNguonVon
                    {
                        Id = vId,
                        QuyetDinhDuToanId = entity.Id,
                        NguonVonId = vDto.NguonVonId,
                        GiaTri = vDto.GiaTri ?? 0,
                        Nam = vDto.Nam
                    });
                }
            }
        }
        else
        {
            entity.KeHoachVons.Clear();
        }
    }

}