using Microsoft.EntityFrameworkCore;
using QLDA.Application.GoiThaus.DTOs;
using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.GoiThaus.Commands;

public record GoiThauImportRangeCommand(List<GoiThauImportDto> Imports) : IRequest;

public class GoiThauImportRangeCommandHandler(IServiceProvider ServiceProvider)
    : IRequestHandler<GoiThauImportRangeCommand> {
    private readonly IRepository<GoiThau, Guid> _goiThau =
        ServiceProvider.GetRequiredService<IRepository<GoiThau, Guid>>();
    private readonly IRepository<KeHoachLuaChonNhaThau, Guid> _keHoach =
        ServiceProvider.GetRequiredService<IRepository<KeHoachLuaChonNhaThau, Guid>>();
    private readonly IRepository<DanhMucLoaiHopDong, int> _loaiHopDong =
        ServiceProvider.GetRequiredService<IRepository<DanhMucLoaiHopDong, int>>();
    private readonly IRepository<DanhMucHinhThucLuaChonNhaThau, int> _hinhThuc =
        ServiceProvider.GetRequiredService<IRepository<DanhMucHinhThucLuaChonNhaThau, int>>();
    private readonly IRepository<DanhMucPhuongThucLuaChonNhaThau, int> _phuongThuc =
        ServiceProvider.GetRequiredService<IRepository<DanhMucPhuongThucLuaChonNhaThau, int>>();
    private readonly IRepository<DanhMucNguonVon, int> _nguonVon =
        ServiceProvider.GetRequiredService<IRepository<DanhMucNguonVon, int>>();

    public async Task Handle(GoiThauImportRangeCommand request, CancellationToken cancellationToken) {
        var keHoachNames = request.Imports
            .Where(e => !string.IsNullOrWhiteSpace(e.TenKeHoachLuaChonNhaThau))
            .Select(e => e.TenKeHoachLuaChonNhaThau!)
            .Distinct()
            .ToList();

        var keHoachs = await _keHoach.GetQueryableSet()
            .Where(e => keHoachNames.Contains(e.Ten!))
            .Select(e => new { e.Ten, e.Id, e.DuAnId })
            .ToListAsync(cancellationToken);
        var keHoachDict = keHoachs
            .DistinctBy(e => e.Ten)
            .ToDictionary(g => g.Ten!, g => new { g.Id, g.DuAnId });

        var loaiHopDongNames = request.Imports
            .Where(e => !string.IsNullOrWhiteSpace(e.TenLoaiHopDong))
            .Select(e => e.TenLoaiHopDong!)
            .Distinct()
            .ToList();

        var loaiHopDongs = await _loaiHopDong.GetQueryableSet()
            .Where(e => loaiHopDongNames.Contains(e.Ten!))
            .Select(e => new { e.Ten, e.Id })
            .ToListAsync(cancellationToken);
        var loaiHopDongDict = loaiHopDongs.DistinctBy(e => e.Ten).ToDictionary(g => g.Ten!, g => g.Id);

        var hinhThucNames = request.Imports
            .Where(e => !string.IsNullOrWhiteSpace(e.TenHinhThucLuaChonNhaThau))
            .Select(e => e.TenHinhThucLuaChonNhaThau!)
            .Distinct()
            .ToList();

        var hinhThucs = await _hinhThuc.GetQueryableSet()
            .Where(e => hinhThucNames.Contains(e.Ten!))
            .Select(e => new { e.Ten, e.Id })
            .ToListAsync(cancellationToken);
        var hinhThucDict = hinhThucs.DistinctBy(e => e.Ten).ToDictionary(g => g.Ten!, g => g.Id);

        var phuongThucNames = request.Imports
            .Where(e => !string.IsNullOrWhiteSpace(e.TenPhuongThucLuaChonNhaThau))
            .Select(e => e.TenPhuongThucLuaChonNhaThau!)
            .Distinct()
            .ToList();

        var phuongThucs = await _phuongThuc.GetQueryableSet()
            .Where(e => phuongThucNames.Contains(e.Ten!))
            .Select(e => new { e.Ten, e.Id })
            .ToListAsync(cancellationToken);
        var phuongThucDict = phuongThucs.DistinctBy(e => e.Ten).ToDictionary(g => g.Ten!, g => g.Id);

        var nguonVonNames = request.Imports
            .Where(e => !string.IsNullOrWhiteSpace(e.TenNguonVon))
            .Select(e => e.TenNguonVon!)
            .Distinct()
            .ToList();

        var nguonVons = await _nguonVon.GetQueryableSet()
            .Where(e => nguonVonNames.Contains(e.Ten!))
            .Select(e => new { e.Ten, e.Id })
            .ToListAsync(cancellationToken);
        var nguonVonDict = nguonVons.DistinctBy(e => e.Ten).ToDictionary(g => g.Ten!, g => g.Id);

        foreach (var item in request.Imports) {
            if (!keHoachDict.TryGetValue(item.TenKeHoachLuaChonNhaThau ?? string.Empty, out var keHoachInfo))
                continue;

            int? loaiHopDongId = null;
            if (!string.IsNullOrWhiteSpace(item.TenLoaiHopDong))
                loaiHopDongDict.TryGetValue(item.TenLoaiHopDong, out var tmpLoaiHopDongId);

            int? hinhThucId = null;
            if (!string.IsNullOrWhiteSpace(item.TenHinhThucLuaChonNhaThau))
                hinhThucDict.TryGetValue(item.TenHinhThucLuaChonNhaThau, out var tmpHinhThucId);

            int? phuongThucId = null;
            if (!string.IsNullOrWhiteSpace(item.TenPhuongThucLuaChonNhaThau))
                phuongThucDict.TryGetValue(item.TenPhuongThucLuaChonNhaThau, out var tmpPhuongThucId);

            int? nguonVonId = null;
            if (!string.IsNullOrWhiteSpace(item.TenNguonVon))
                nguonVonDict.TryGetValue(item.TenNguonVon, out var tmpNguonVonId);

            await _goiThau.AddAsync(new GoiThau {
                Id = Guid.NewGuid(),
                DuAnId = keHoachInfo.DuAnId,
                KeHoachLuaChonNhaThauId = keHoachInfo.Id,
                Ten = item.Ten,
                GiaTri = item.GiaTri,
                LoaiHopDongId = loaiHopDongId,
                HinhThucLuaChonNhaThauId = hinhThucId,
                PhuongThucLuaChonNhaThauId = phuongThucId,
                NguonVonId = nguonVonId,
                ThoiGianLuaNhaThau = item.ThoiGianToChucLuaChonNhaThau,
                ThoiGianBatDauToChucLuaChonNhaThau = item.ThoiGianBatDauToChucLuaChonNhaThau,
                ThoiGianThucHienGoiThau = item.ThoiGianThucHienGoiThau,
                TomTatCongViecChinhGoiThau = item.TomTatCongViecChinhGoiThau,
                TuyChonMuaThem = item.TuyChonMuaThem,
                DaDuyet = false
            }, cancellationToken);
        }

        await _goiThau.UnitOfWork.SaveChangesAsync(cancellationToken);
    }
}