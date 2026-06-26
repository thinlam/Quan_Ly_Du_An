using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.KeHoachTrienKhaiHangMucs.DTOs;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;
using BuildingBlocks.Domain.Entities;
using BuildingBlocks.Domain.Providers;

namespace QLDA.Application.KeHoachTrienKhaiHangMucs.Commands;

public record KeHoachTrienKhaiHangMucImportRangeCommand(
    List<KeHoachTrienKhaiHangMucImportDto> Imports,
    Guid DuAnId,
    int BuocId) : IRequest<KeHoachTrienKhaiHangMucImportResultDto>;

internal class KeHoachTrienKhaiHangMucImportRangeCommandHandler(IServiceProvider serviceProvider)
    : IRequestHandler<KeHoachTrienKhaiHangMucImportRangeCommand, KeHoachTrienKhaiHangMucImportResultDto> {
    private readonly IRepository<KeHoachTrienKhaiHangMuc, Guid> _repo =
        serviceProvider.GetRequiredService<IRepository<KeHoachTrienKhaiHangMuc, Guid>>();
    private readonly IRepository<DanhMucGiaiDoan, int> _giaiDoanRepo =
        serviceProvider.GetRequiredService<IRepository<DanhMucGiaiDoan, int>>();
    private readonly IRepository<UserMaster, long> _userRepo =
        serviceProvider.GetRequiredService<IRepository<UserMaster, long>>();
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo =
        serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
    private readonly IBuocAuthorizationProvider _auth =
        serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
    private readonly IAuthorizationContext _authContext =
        serviceProvider.GetRequiredService<IAuthorizationContext>();
    private readonly IUserProvider _userProvider =
        serviceProvider.GetRequiredService<IUserProvider>();

    public async Task<KeHoachTrienKhaiHangMucImportResultDto> Handle(
        KeHoachTrienKhaiHangMucImportRangeCommand request,
        CancellationToken cancellationToken = default) {
        var result = new KeHoachTrienKhaiHangMucImportResultDto();
        var rows = request.Imports.Where(row => !IsEmptyRow(row)).ToList();

        if (rows.Count == 0)
            return result;

        await _auth.EnsureCanExecuteStepAsync(request.BuocId, _authContext, cancellationToken);

        var giaiDoanByTen = await _giaiDoanRepo.GetQueryableSet()
            .AsNoTracking()
            .Where(e => e.Ten != null)
            .Select(e => new { e.Ten, e.Id })
            .ToListAsync(cancellationToken);
        var giaiDoanLookup = giaiDoanByTen
            .GroupBy(e => e.Ten!.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().Id, StringComparer.OrdinalIgnoreCase);

        var donViId = KeHoachTrienKhaiHangMucImportUserScope.TryGetCurrentDonViId(_userProvider);
        var usersInDonVi = await _userRepo.GetQueryableSet()
            .AsNoTracking()
            .Where(e => e.LaDonViChinh == true)
            .WhereIf(donViId > 0, e => e.DonViId == donViId)
            .Where(e => e.HoTen != null && e.HoTen != "")
            .Select(e => new UserImportLookup(e.Id, e.HoTen!, e.DonViId))
            .ToListAsync(cancellationToken);

        var trangThaiDuThao = await _statusRepo
            .GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(
                s => s.Ma == TrangThaiPheDuyetCodes.DeXuatMacDinh.DuThao
                     && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt,
                cancellationToken);

        var validRows = new List<ValidatedImportRow>();

        foreach (var item in rows) {
            var rowLabel = item.ExcelRowNumber > 0
                ? $"Dòng {item.ExcelRowNumber}"
                : "Dòng";

            if (string.IsNullOrWhiteSpace(item.TenHangMuc)) {
                AddError(result, item, rowLabel, "Tên hạng mục bắt buộc");
                continue;
            }

            if (string.IsNullOrWhiteSpace(item.TenGiaiDoan)) {
                AddError(result, item, rowLabel, "Giai đoạn bắt buộc");
                continue;
            }

            if (!giaiDoanLookup.TryGetValue(item.TenGiaiDoan.Trim(), out var giaiDoanId)) {
                AddError(result, item, rowLabel, "Không tìm thấy giai đoạn");
                continue;
            }

            if (string.IsNullOrWhiteSpace(item.TenCanBoChuTri)) {
                AddError(result, item, rowLabel, "Cán bộ chủ trì bắt buộc");
                continue;
            }

            if (!TryResolveUser(item.TenCanBoChuTri, usersInDonVi, out var chuTriUser, out var chuTriError)) {
                AddError(result, item, rowLabel, chuTriError ?? "Không tìm thấy cán bộ chủ trì");
                continue;
            }

            List<long>? canBoPhoiHopIds = null;
            List<long>? donViPhoiHopIds = null;
            if (!string.IsNullOrWhiteSpace(item.TenCanBoPhoiHop)) {
                if (!TryResolveUser(item.TenCanBoPhoiHop, usersInDonVi, out var phoiHopUser, out var phoiHopError)) {
                    AddError(result, item, rowLabel, phoiHopError ?? "Không tìm thấy cán bộ phối hợp");
                    continue;
                }

                canBoPhoiHopIds = [phoiHopUser.Id];
                if (phoiHopUser.DonViId is > 0)
                    donViPhoiHopIds = [phoiHopUser.DonViId.Value];
            }

            if (string.IsNullOrWhiteSpace(item.So)) {
                AddError(result, item, rowLabel, "Tờ trình bắt buộc");
                continue;
            }

            if (item.KinhPhi is < 0) {
                AddError(result, item, rowLabel, "Kinh phí không hợp lệ");
                continue;
            }

            validRows.Add(new ValidatedImportRow(
                item,
                giaiDoanId,
                chuTriUser.Id,
                chuTriUser.DonViId is > 0 ? chuTriUser.DonViId : null,
                canBoPhoiHopIds,
                donViPhoiHopIds));
        }

        if (validRows.Count == 0)
            return result;

        var groups = validRows.GroupBy(r => new ParentKey(
            r.Source.So!.Trim(),
            r.Source.NgayTrinh,
            r.Source.TrichYeu?.Trim() ?? string.Empty));

        foreach (var group in groups) {
            var parent = new KeHoachTrienKhaiHangMuc {
                DuAnId = request.DuAnId,
                BuocId = request.BuocId,
                So = group.Key.So,
                NgayToTrinh = group.Key.NgayTrinh,
                TrichYeu = string.IsNullOrEmpty(group.Key.TrichYeu) ? null : group.Key.TrichYeu,
                TrangThaiId = trangThaiDuThao?.Id,
                DanhSachHangMuc = [],
            };

            foreach (var row in group) {
                parent.DanhSachHangMuc!.Add(new HangMucKeHoach {
                    TenHangMuc = row.Source.TenHangMuc!.Trim(),
                    GiaiDoanId = row.GiaiDoanId,
                    DonViChuTriId = row.DonViChuTriId,
                    DonViPhoiHopIds = row.DonViPhoiHopIds,
                    CanBoChuTriId = row.CanBoChuTriId,
                    CanBoPhoiHopIds = row.CanBoPhoiHopIds,
                    NgayBatDau = row.Source.NgayBatDau,
                    NgayKetThuc = row.Source.NgayKetThuc,
                    ThoiHan = row.Source.ThoiHan,
                    KinhPhi = row.Source.KinhPhi,
                });
            }

            await _repo.AddAsync(parent, cancellationToken);
        }

        await _repo.UnitOfWork.SaveChangesAsync(cancellationToken);
        result.SuccessCount = validRows.Count;
        return result;
    }

    private static void AddError(
        KeHoachTrienKhaiHangMucImportResultDto result,
        KeHoachTrienKhaiHangMucImportDto item,
        string rowLabel,
        string message) {
        var fullMessage = $"{rowLabel}: {message}";
        result.Errors.Add(fullMessage);
        item.ErrorMessage = message;
        result.ErrorCount++;
    }

    private static bool TryResolveUser(
        string tenCanBo,
        List<UserImportLookup> usersInDonVi,
        out UserImportLookup user,
        out string? error) {
        user = default!;
        error = null;

        var trimmed = tenCanBo.Trim();
        var matches = usersInDonVi
            .Where(u => string.Equals(u.HoTen.Trim(), trimmed, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (matches.Count == 0) {
            error = "Không tìm thấy cán bộ";
            return false;
        }

        if (matches.Count > 1) {
            error = "Cán bộ không xác định (trùng tên)";
            return false;
        }

        user = matches[0];
        return true;
    }

    private static bool IsEmptyRow(KeHoachTrienKhaiHangMucImportDto row) =>
        string.IsNullOrWhiteSpace(row.TenHangMuc)
        && string.IsNullOrWhiteSpace(row.TenGiaiDoan)
        && string.IsNullOrWhiteSpace(row.TenCanBoChuTri)
        && string.IsNullOrWhiteSpace(row.TenCanBoPhoiHop)
        && !row.NgayBatDau.HasValue
        && !row.NgayKetThuc.HasValue
        && !row.KinhPhi.HasValue
        && !row.ThoiHan.HasValue
        && string.IsNullOrWhiteSpace(row.So)
        && !row.NgayTrinh.HasValue
        && string.IsNullOrWhiteSpace(row.TrichYeu);

    private sealed record UserImportLookup(long Id, string HoTen, long? DonViId);

    private sealed record ParentKey(string So, DateTimeOffset? NgayTrinh, string TrichYeu);

    private sealed record ValidatedImportRow(
        KeHoachTrienKhaiHangMucImportDto Source,
        int GiaiDoanId,
        long CanBoChuTriId,
        long? DonViChuTriId,
        List<long>? CanBoPhoiHopIds,
        List<long>? DonViPhoiHopIds);
}
