using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.GoiThaus.DTOs;

namespace QLDA.Application.GoiThaus.Commands;

public record GoiThauImportRangeCommand(List<GoiThauImportDto> Imports) : IRequest<GoiThauImportResultDto> {
    public Guid DuAnId { get; init; }
    public int BuocId { get; init; }
}

internal class GoiThauImportRangeCommandHandler(IServiceProvider serviceProvider)
    : IRequestHandler<GoiThauImportRangeCommand, GoiThauImportResultDto> {
    private readonly IRepository<GoiThau, Guid> _goiThau =
        serviceProvider.GetRequiredService<IRepository<GoiThau, Guid>>();
    private readonly IRepository<KeHoachLuaChonNhaThau, Guid> _keHoach =
        serviceProvider.GetRequiredService<IRepository<KeHoachLuaChonNhaThau, Guid>>();
    private readonly IRepository<DanhMucLoaiHopDong, int> _loaiHopDong =
        serviceProvider.GetRequiredService<IRepository<DanhMucLoaiHopDong, int>>();
    private readonly IRepository<DanhMucHinhThucLuaChonNhaThau, int> _hinhThuc =
        serviceProvider.GetRequiredService<IRepository<DanhMucHinhThucLuaChonNhaThau, int>>();
    private readonly IRepository<DanhMucPhuongThucLuaChonNhaThau, int> _phuongThuc =
        serviceProvider.GetRequiredService<IRepository<DanhMucPhuongThucLuaChonNhaThau, int>>();
    private readonly IRepository<DanhMucNguonVon, int> _nguonVon =
        serviceProvider.GetRequiredService<IRepository<DanhMucNguonVon, int>>();
    private readonly IBuocAuthorizationProvider _auth =
        serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
    private readonly IAuthorizationContext _authContext =
        serviceProvider.GetRequiredService<IAuthorizationContext>();

    public async Task<GoiThauImportResultDto> Handle(
        GoiThauImportRangeCommand request,
        CancellationToken cancellationToken) {
        var result = new GoiThauImportResultDto();
        var rows = request.Imports.Where(row => !IsEmptyRow(row)).ToList();

        if (rows.Count == 0)
            return result;

        var importTrongDuAn = request.DuAnId != Guid.Empty;

        if (importTrongDuAn && request.BuocId <= 0) {
            result.Errors.Add("Thiếu buocId");
            result.ErrorCount = 1;
            return result;
        }

        if (importTrongDuAn)
            await _auth.EnsureCanExecuteStepAsync(request.BuocId, _authContext, cancellationToken);

        var keHoachNames = rows
            .Where(e => !string.IsNullOrWhiteSpace(e.TenKeHoachLuaChonNhaThau))
            .Select(e => e.TenKeHoachLuaChonNhaThau!)
            .Distinct()
            .ToList();

        var keHoachQuery = _keHoach.GetQueryableSet()
            .Where(e => keHoachNames.Contains(e.Ten!));

        if (importTrongDuAn)
            keHoachQuery = keHoachQuery.Where(e => e.DuAnId == request.DuAnId);

        var keHoachs = await keHoachQuery
            .Select(e => new { e.Ten, e.Id, e.DuAnId })
            .ToListAsync(cancellationToken);
        var keHoachDict = keHoachs
            .DistinctBy(e => e.Ten)
            .ToDictionary(g => g.Ten!, g => new { g.Id, g.DuAnId });

        var loaiHopDongNames = rows
            .Where(e => !string.IsNullOrWhiteSpace(e.TenLoaiHopDong))
            .Select(e => e.TenLoaiHopDong!)
            .Distinct()
            .ToList();

        var loaiHopDongs = await _loaiHopDong.GetQueryableSet()
            .Where(e => loaiHopDongNames.Contains(e.Ten!))
            .Select(e => new { e.Ten, e.Id })
            .ToListAsync(cancellationToken);
        var loaiHopDongDict = loaiHopDongs.DistinctBy(e => e.Ten).ToDictionary(g => g.Ten!, g => g.Id);

        var hinhThucNames = rows
            .Where(e => !string.IsNullOrWhiteSpace(e.TenHinhThucLuaChonNhaThau))
            .Select(e => e.TenHinhThucLuaChonNhaThau!)
            .Distinct()
            .ToList();

        var hinhThucs = await _hinhThuc.GetQueryableSet()
            .Where(e => hinhThucNames.Contains(e.Ten!))
            .Select(e => new { e.Ten, e.Id })
            .ToListAsync(cancellationToken);
        var hinhThucDict = hinhThucs.DistinctBy(e => e.Ten).ToDictionary(g => g.Ten!, g => g.Id);

        var phuongThucNames = rows
            .Where(e => !string.IsNullOrWhiteSpace(e.TenPhuongThucLuaChonNhaThau))
            .Select(e => e.TenPhuongThucLuaChonNhaThau!)
            .Distinct()
            .ToList();

        var phuongThucs = await _phuongThuc.GetQueryableSet()
            .Where(e => phuongThucNames.Contains(e.Ten!))
            .Select(e => new { e.Ten, e.Id })
            .ToListAsync(cancellationToken);
        var phuongThucDict = phuongThucs.DistinctBy(e => e.Ten).ToDictionary(g => g.Ten!, g => g.Id);

        var nguonVonNames = rows
            .Where(e => !string.IsNullOrWhiteSpace(e.TenNguonVon))
            .Select(e => e.TenNguonVon!)
            .Distinct()
            .ToList();

        var nguonVons = await _nguonVon.GetQueryableSet()
            .Where(e => nguonVonNames.Contains(e.Ten!))
            .Select(e => new { e.Ten, e.Id })
            .ToListAsync(cancellationToken);
        var nguonVonDict = nguonVons.DistinctBy(e => e.Ten).ToDictionary(g => g.Ten!, g => g.Id);

        for (var i = 0; i < rows.Count; i++) {
            var item = rows[i];
            var rowLabel = $"dòng {i + 1}";

            if (string.IsNullOrWhiteSpace(item.TenKeHoachLuaChonNhaThau)) {
                result.Errors.Add($"{rowLabel}: Kế hoạch lựa chọn nhà thầu không được để trống");
                result.ErrorCount++;
                continue;
            }

            if (!keHoachDict.TryGetValue(item.TenKeHoachLuaChonNhaThau, out var keHoachInfo)) {
                result.Errors.Add(
                    $"{rowLabel}: Không tìm thấy kế hoạch lựa chọn nhà thầu '{item.TenKeHoachLuaChonNhaThau}'");
                result.ErrorCount++;
                continue;
            }

            if (importTrongDuAn && keHoachInfo.DuAnId != request.DuAnId) {
                result.Errors.Add(
                    $"{rowLabel}: Kế hoạch '{item.TenKeHoachLuaChonNhaThau}' không thuộc dự án hiện tại");
                result.ErrorCount++;
                continue;
            }

            int? loaiHopDongId = !string.IsNullOrWhiteSpace(item.TenLoaiHopDong)
                && loaiHopDongDict.TryGetValue(item.TenLoaiHopDong, out var loaiHopDongIdVal)
                ? loaiHopDongIdVal
                : null;

            int? hinhThucId = !string.IsNullOrWhiteSpace(item.TenHinhThucLuaChonNhaThau)
                && hinhThucDict.TryGetValue(item.TenHinhThucLuaChonNhaThau, out var hinhThucIdVal)
                ? hinhThucIdVal
                : null;

            int? phuongThucId = !string.IsNullOrWhiteSpace(item.TenPhuongThucLuaChonNhaThau)
                && phuongThucDict.TryGetValue(item.TenPhuongThucLuaChonNhaThau, out var phuongThucIdVal)
                ? phuongThucIdVal
                : null;

            int? nguonVonId = !string.IsNullOrWhiteSpace(item.TenNguonVon)
                && nguonVonDict.TryGetValue(item.TenNguonVon, out var nguonVonIdVal)
                ? nguonVonIdVal
                : null;

            await _goiThau.AddAsync(new GoiThau {
                Id = Guid.NewGuid(),
                DuAnId = importTrongDuAn ? request.DuAnId : keHoachInfo.DuAnId,
                BuocId = request.BuocId > 0 ? request.BuocId : null,
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
                DaDuyet = true
            }, cancellationToken);

            result.SuccessCount++;
        }

        if (result.SuccessCount > 0)
            await _goiThau.UnitOfWork.SaveChangesAsync(cancellationToken);

        return result;
    }

    private static bool IsEmptyRow(GoiThauImportDto row) =>
        string.IsNullOrWhiteSpace(row.TenKeHoachLuaChonNhaThau)
        && string.IsNullOrWhiteSpace(row.Ten);
}
