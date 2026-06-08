using QLDA.Application.DuToans.DTOs;
using QLDA.Application.KeHoachTrienKhaiHangMucs.DTOs;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.ToTrinhThamDinhNhaThaus.DTOs;

namespace QLDA.Application.KeHoachTrienKhaiHangMucMappings;

public static class KeHoachTrienKhaiHangMucMappings
{
    public static HangMucKeHoach ToEntity(this HangMucTrienKhaiDto item, Guid keHoachId)
    {
        return new HangMucKeHoach
        {
            Id = item.Id.GetId(),
            KeHoachId = keHoachId,
            GiaiDoanId = item.GiaiDoanId,
            TenHangMuc = item.TenHangMuc,
            KinhPhi = item.KinhPhi,
            NgayBatDau = item.NgayBatDau,
            NgayKetThuc = item.NgayKetThuc,
            ThoiHan = item.ThoiHan,
            CanBoChuTriId = item.CanBoChuTriId,
            CanBoPhoiHopIds = item.CanBoPhoiHopIds,
            DonViChuTriId = item.DonViChuTriId,
            DonViPhoiHopIds = item.DonViPhoiHops,
        };
    }
    public static void SyncHangMuc(this KeHoachTrienKhaiHangMuc entity, List<HangMucTrienKhaiDto>? hangMucs)
    {
        hangMucs ??= [];
        entity.DanhSachHangMuc ??= [];

        var existingIdsInDb = entity.DanhSachHangMuc.Select(x => x.Id).ToHashSet();

        var removeItems = entity.DanhSachHangMuc
            .Where(x => !hangMucs.Any(y => y.Id == x.Id))
            .ToList();

        foreach (var item in removeItems)
        {
            entity.DanhSachHangMuc.Remove(item);
        }

        // 3. ADD / UPDATE
        foreach (var item in hangMucs)
        {
            bool isUpdate = item.Id != Guid.Empty && existingIdsInDb.Contains(item.Id??Guid.Empty);

            if (!isUpdate)
            {
                entity.DanhSachHangMuc.Add(new HangMucKeHoach
                {
                    //Id =  Guid.NewGuid() ,
                    KeHoachId = entity.Id,
                    GiaiDoanId = item.GiaiDoanId,
                    TenHangMuc = item.TenHangMuc,
                    KinhPhi = item.KinhPhi,
                    NgayBatDau = item.NgayBatDau,
                    NgayKetThuc = item.NgayKetThuc,
                    ThoiHan = item.ThoiHan,
                    CanBoChuTriId = item.CanBoChuTriId,
                    CanBoPhoiHopIds = item.CanBoPhoiHopIds,
                    DonViChuTriId = item.DonViChuTriId,
                    DonViPhoiHopIds = item.DonViPhoiHops,
                //    CreatedAt = DateTimeOffset.UtcNow, // Đảm bảo không bị ngày 0001-01-01
                //    IsDeleted = false
                });
            }
            else
            {
                var existing = entity.DanhSachHangMuc.First(x => x.Id == item.Id);

                existing.GiaiDoanId = item.GiaiDoanId;
                existing.TenHangMuc = item.TenHangMuc;
                existing.KinhPhi = item.KinhPhi;
                existing.NgayBatDau = item.NgayBatDau;
                existing.NgayKetThuc = item.NgayKetThuc;
                existing.ThoiHan = item.ThoiHan;
                existing.CanBoChuTriId = item.CanBoChuTriId;
                existing.CanBoPhoiHopIds = item.CanBoPhoiHopIds;
                existing.DonViChuTriId = item.DonViChuTriId;
                existing.DonViPhoiHopIds = item.DonViPhoiHops;

                // Không đụng vào CreatedAt, chỉ cập nhật UpdatedAt
              //  existing.UpdatedAt = DateTimeOffset.UtcNow;
            }
        }
    }
    public static void SyncHangMuc1(this KeHoachTrienKhaiHangMuc entity, List<HangMucTrienKhaiDto>? hangMucs)
    {
        if (hangMucs is null)
        {
            entity.DanhSachHangMuc = [];
            return;
        }

        entity.DanhSachHangMuc ??= [];
        entity.DanhSachHangMuc.Clear();

        foreach (var items in hangMucs)
        {
            entity.DanhSachHangMuc.Add(new HangMucKeHoach
            {
                Id = items.Id ?? Guid.NewGuid(),
                KeHoachId = entity.Id   ,
                GiaiDoanId = items.GiaiDoanId,
                TenHangMuc = items.TenHangMuc,
                KinhPhi = items.KinhPhi,
                NgayBatDau = items.NgayBatDau,
                NgayKetThuc = items.NgayKetThuc,
                ThoiHan = items.ThoiHan,
                CanBoChuTriId = items.CanBoChuTriId,
                CanBoPhoiHopIds = items.CanBoPhoiHopIds,
                DonViChuTriId = items.DonViChuTriId,
                DonViPhoiHopIds = items.DonViPhoiHops,
            });
        }
    }
    public static KeHoachTrienKhaiHangMuc ToEntity(this KeHoachTrienKhaiHangMucDto dto)
    {
        var entity = new KeHoachTrienKhaiHangMuc()
        {
            Id= dto.Id??Guid.NewGuid(),
            DuAnId = dto.DuAnId,
            BuocId = dto.BuocId,
            So = dto.So,
            NgayToTrinh = dto.NgayTrinh,
            TrichYeu = dto.TrichYeu,
            TrangThaiId = dto.TrangThaiId,
        };

        entity.SyncHangMuc(dto.HangMucTrienKhai);

        return entity;
    }


    public static KeHoachTrienKhaiHangMucDto ToDto(this KeHoachTrienKhaiHangMuc entity, List<TepDinhKem>? files = null) =>
        new()
        {
            Id = entity.Id,
            So = entity.So,
            NgayTrinh = entity.NgayToTrinh,
            TrichYeu = entity.TrichYeu,
            TrangThaiId = entity.TrangThaiId,
            
            DanhSachTepDinhKem = files?.Select(x => x.ToDto()).ToList(),
            HangMucTrienKhai = entity.DanhSachHangMuc?.Select(x=>new HangMucTrienKhaiDto() {
                TenHangMuc = x.TenHangMuc,
                GiaiDoanId = x.GiaiDoanId,
                CanBoChuTriId = x.CanBoChuTriId,
                CanBoPhoiHopIds = x.CanBoPhoiHopIds,
                DonViChuTriId = x.DonViChuTriId,
                DonViPhoiHops = x.DonViPhoiHopIds,
                NgayBatDau = x.NgayBatDau,
                NgayKetThuc = x.NgayKetThuc,
                ThoiHan = x.ThoiHan,
                KinhPhi = x.KinhPhi,
                
            }).ToList(),

        };
}
