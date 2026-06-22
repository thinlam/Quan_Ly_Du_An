using BuildingBlocks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Interfaces;
using QLDA.Application.TongHopDeXuatChuTruongs.DTOs;
using QLDA.Domain.Constants;

namespace QLDA.Application.TongHopDeXuatChuTruongs.Queries;

public record TongHopDeXuatChuTruongGetExportQuery : IMayHaveGlobalFilter, IRequest<TongHopDeXuatChuTruongExportResult> {
    public int? BuocId { get; set; }
    public Guid? DuAnId { get; set; }
    public string? GlobalFilter { get; set; }
    public string? Loai { get; set; }
    public long? DonViPhuTrachId { get; set; }
    public int? Nam { get; set; }
}

internal class TongHopDeXuatChuTruongGetExportQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<TongHopDeXuatChuTruongGetExportQuery, TongHopDeXuatChuTruongExportResult> {
    private readonly IRepository<DeXuatChuTruongMoi, Guid> _deXuatChuTruongMoi =
        serviceProvider.GetRequiredService<IRepository<DeXuatChuTruongMoi, Guid>>();
    private readonly IRepository<DeXuatChuyenTiep, Guid> _deXuatChuyenTiep =
        serviceProvider.GetRequiredService<IRepository<DeXuatChuyenTiep, Guid>>();
    private readonly IRepository<DmDonVi, long> _dmDonVi =
        serviceProvider.GetRequiredService<IRepository<DmDonVi, long>>();
    private readonly IRepository<UserMaster, long> _userMaster =
        serviceProvider.GetRequiredService<IRepository<UserMaster, long>>();

    public async Task<TongHopDeXuatChuTruongExportResult> Handle(
        TongHopDeXuatChuTruongGetExportQuery request,
        CancellationToken cancellationToken = default) {
        var userQuery = _userMaster.GetQueryableSet().AsNoTracking();
        var dmDonViQuery = _dmDonVi.GetQueryableSet().AsNoTracking();

        var queryableMoi = _deXuatChuTruongMoi.GetQueryableSet().AsNoTracking()
            .Where(e => !e.IsDeleted)
            .Where(e => !e.DuAn!.IsDeleted)
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereIf(request.DonViPhuTrachId != null, e => e.DonViPhuTrachChinhId == request.DonViPhuTrachId)
            .WhereIf(request.BuocId > 0, e => e.BuocId == request.BuocId)
            .WhereIf(request.Nam != null, e => e.NamDeXuat == request.Nam)
            .Select(e => new TongHopDeXuatChuTruongDto {
                Id = e.Id,
                DuAnId = e.DuAnId,
                BuocId = e.BuocId,
                TenDuAn = e.DuAn != null ? e.DuAn.TenDuAn : "Không rõ",
                TrangThaiId = e.TrangThaiId,
                MaTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG"
                    ? e.TrangThai.Ma
                    : TrangThaiPheDuyetCodes.Default.DuThao,
                TenTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG"
                    ? e.TrangThai.Ten
                    : TrangThaiPheDuyetCodes.Default.TenDuThao,
                TenPhongBanPhuTrach = e.CreatedBy != null
                    ? dmDonViQuery.Where(dv => dv.Id == userQuery
                            .Where(us => us.UserPortalId == Convert.ToInt64(e.CreatedBy))
                            .Select(us => us.PhongBanId)
                            .FirstOrDefault())
                        .Select(dv => dv.TenDonVi)
                        .FirstOrDefault() ?? "Không rõ"
                    : "Không rõ",
                Loai = "DeXuatMoi",
            });

        var queryableCT = _deXuatChuyenTiep.GetQueryableSet().AsNoTracking()
            .Where(e => !e.IsDeleted)
            .Where(e => !e.DuAn!.IsDeleted)
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereIf(request.BuocId > 0, e => e.BuocId == request.BuocId)
            .WhereIf(request.Nam != null, e => e.NamDeXuat == request.Nam)
            .Select(e => new TongHopDeXuatChuTruongDto {
                Id = e.Id,
                DuAnId = e.DuAnId,
                BuocId = e.BuocId,
                TenDuAn = e.DuAn != null ? e.DuAn.TenDuAn : "Không rõ",
                TenPhongBanPhuTrach = e.CreatedBy != null
                    ? dmDonViQuery.Where(dv => dv.Id == userQuery
                            .Where(us => us.UserPortalId == Convert.ToInt64(e.CreatedBy))
                            .Select(us => us.PhongBanId)
                            .FirstOrDefault())
                        .Select(dv => dv.TenDonVi)
                        .FirstOrDefault() ?? "Không rõ"
                    : "Không rõ",
                TrangThaiId = e.TrangThaiId,
                MaTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG"
                    ? e.TrangThai.Ma
                    : TrangThaiPheDuyetCodes.Default.DuThao,
                TenTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG"
                    ? e.TrangThai.Ten
                    : TrangThaiPheDuyetCodes.Default.TenDuThao,
                Loai = "ChuyenTiep",
            });

        var finalQueryable = queryableMoi.Concat(queryableCT);

        var tongDeXuatMoi = await queryableMoi.CountAsync(cancellationToken);
        var tongChuyenTiep = await queryableCT.CountAsync(cancellationToken);

        IQueryable<TongHopDeXuatChuTruongDto> dataQueryable = request.Loai switch {
            "DeXuatMoi" => queryableMoi,
            "ChuyenTiep" => queryableCT,
            _ => finalQueryable,
        };

        var rows = await dataQueryable
            .OrderBy(e => e.TenDuAn)
            .ThenBy(e => e.Id)
            .ToListAsync(cancellationToken);

        return new TongHopDeXuatChuTruongExportResult {
            TongDeXuatMoi = tongDeXuatMoi,
            TongDeXuatChuyenTiep = tongChuyenTiep,
            Rows = rows.Select((row, index) => new TongHopDeXuatChuTruongExportDto {
                Stt = index + 1,
                LoaiDeXuat = MapLoaiDeXuat(row.Loai),
                TenDuAn = row.TenDuAn,
                PhongBanPhuTrach = row.TenPhongBanPhuTrach,
            }).ToList(),
        };
    }

    private static string MapLoaiDeXuat(string? loai) => loai switch {
        "DeXuatMoi" => "Chủ trương mới",
        "ChuyenTiep" => "Chuyển tiếp",
        _ => loai ?? string.Empty,
    };
}
