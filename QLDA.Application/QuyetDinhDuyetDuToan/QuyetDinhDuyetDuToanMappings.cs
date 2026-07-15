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
            So = dto.So ?? string.Empty,
            Ngay = dto.Ngay,
            TrichYeu = dto.TrichYeu ?? string.Empty,
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
                ChiPhi = cp.TenChiPhi ?? string.Empty,
                GiaTri = cp.GiaTri??0
           })];
        }
        return entity!;
    }

    public static QuyetDinhDuyetDuToanDto ToDto(this QuyetDinhDuyetDuToan entity) {
        return new QuyetDinhDuyetDuToanDto
        {
            Id = entity.Id,
            DuAnId = entity.DuAnId,
            BuocId = entity.BuocId,
            SoQuyetDinh = entity.So ?? string.Empty,
            NgayQuyetDinh = entity.Ngay,
            TrichYeu = entity.TrichYeu ?? string.Empty,
            GiaTri = entity.GiaTri,
            ThoiGian    = entity.ThoiGianThucHien ?? string.Empty,
            TenHinhThucQuanLy = entity.HinhThucQuanLyDuAn!.Ten ?? string.Empty,
            TenKeHoachLuaChonNhaThau = entity.KeHoachLuaChonNhaThau!.Ten ?? string.Empty,
        };
    }

    public static void UpdateToEntity(this QuyetDinhDuyetDuToanInsUpdDto dto, QuyetDinhDuyetDuToan entity)
    {
        entity.DuAnId = dto.DuAnId;
        entity.BuocId = dto.BuocId;
        entity.So = dto.So ?? string.Empty;
        entity.Ngay = dto.Ngay;
        entity.TrichYeu = dto.TrichYeu ?? string.Empty;
        entity.ThoiGianThucHien = dto.ThoiGianThucHien;
        entity.GiaTri = dto.GiaTri;
        entity.HinhThucQuanLyId = dto.HinhThucQuanLyId;
        entity.KeHoachLuaChonNhaThauId = dto.KeHoachLuaChonNhaThauId;

        if (dto.ChiPhis != null)
        {
            var incomingChiPhiIds = dto.ChiPhis.Select(cp => cp.GetId()).ToList();
            var chiPhisToRemove = entity.ChiPhis!.Where(cp => !incomingChiPhiIds.Contains(cp.Id)).ToList();
            foreach (var rm in chiPhisToRemove)
            {
                entity.ChiPhis!.Remove(rm);
            }

            foreach (var cpDto in dto.ChiPhis)
            {
                var cpId = cpDto.GetId();
                var existingChiPhi = entity.ChiPhis!.FirstOrDefault(cp => cp.Id == cpId);

                if (existingChiPhi != null)
                {
                    existingChiPhi.ChiPhi = cpDto.TenChiPhi ?? string.Empty;
                    existingChiPhi.GiaTri = cpDto.GiaTri ?? 0;
                }
                else
                {
                    entity.ChiPhis!.Add(new QuyetDinhDuyetDuToanChiPhi
                    {
                        Id = cpId,
                        QuyetDinhDuToanId = entity.Id,
                        ChiPhi = cpDto.TenChiPhi ?? string.Empty,
                        GiaTri = cpDto.GiaTri ?? 0
                    });
                }
            }
        }
        else
        {
            entity.ChiPhis!.Clear();
        }

        if (dto.KeHoachVons != null)
        {
            var incomingVonIds = dto.KeHoachVons.Select(v => v.GetId()).ToList();
            var vonsToRemove = entity.KeHoachVons!.Where(v => !incomingVonIds.Contains(v.Id)).ToList();
            foreach (var rm in vonsToRemove)
            {
                entity.KeHoachVons!.Remove(rm);
            }

            foreach (var vDto in dto.KeHoachVons)
            {
                var vId = vDto.GetId();
                var existingVon = entity.KeHoachVons!.FirstOrDefault(v => v.Id == vId);

                if (existingVon != null)
                {
                    existingVon.NguonVonId = vDto.NguonVonId;
                    existingVon.GiaTri = vDto.GiaTri ?? 0;
                    existingVon.Nam = vDto.Nam;
                }
                else
                {
                    entity.KeHoachVons!.Add(new QuyetDinhDuyetDuToanNguonVon
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
            entity.KeHoachVons!.Clear();
        }
    }
}