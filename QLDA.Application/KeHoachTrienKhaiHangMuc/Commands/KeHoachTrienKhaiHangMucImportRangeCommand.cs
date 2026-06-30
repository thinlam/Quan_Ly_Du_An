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
    private readonly IRepository<DuAn, Guid> _duAnRepo =
        serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
    private readonly IRepository<DanhMucGiaiDoan, int> _giaiDoanRepo =
        serviceProvider.GetRequiredService<IRepository<DanhMucGiaiDoan, int>>();
    private readonly IRepository<DmDonVi, long> _donViRepo =
        serviceProvider.GetRequiredService<IRepository<DmDonVi, long>>();
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

        var tenDuAns = rows
            .Where(r => !string.IsNullOrWhiteSpace(r.TenDuAn))
            .Select(r => r.TenDuAn!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var duAnByTen = (await _duAnRepo.GetQueryableSet()
            .AsNoTracking()
            .Where(e => e.TenDuAn != null && tenDuAns.Contains(e.TenDuAn))
            .Select(e => new { e.TenDuAn, e.Id })
            .ToListAsync(cancellationToken))
            .GroupBy(e => e.TenDuAn!.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().Id, StringComparer.OrdinalIgnoreCase);

        var giaiDoanByTen = await _giaiDoanRepo.GetQueryableSet()
            .AsNoTracking()
            .Where(e => e.Ten != null)
            .Select(e => new { e.Ten, e.Id })
            .ToListAsync(cancellationToken);
        var giaiDoanLookup = giaiDoanByTen
            .GroupBy(e => e.Ten!.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().Id, StringComparer.OrdinalIgnoreCase);

        var donViRows = await _donViRepo.GetQueryableSet()
            .AsNoTracking()
            .Where(e => e.TenDonVi != null && e.TenDonVi != "")
            .Select(e => new DonViImportLookup(e.Id, e.TenDonVi!))
            .ToListAsync(cancellationToken);

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

            if (string.IsNullOrWhiteSpace(item.TenDuAn)) {
                AddError(result, item, rowLabel, "Dự án bắt buộc");
                continue;
            }

            if (!duAnByTen.TryGetValue(item.TenDuAn.Trim(), out var duAnId)) {
                AddError(result, item, rowLabel, "Không tìm thấy dự án");
                continue;
            }

            if (request.DuAnId != Guid.Empty && request.DuAnId != duAnId) {
                AddError(result, item, rowLabel, "Dự án trên Excel không khớp dự án đang import");
                continue;
            }

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

            if (string.IsNullOrWhiteSpace(item.TenDonViChuTri)) {
                AddError(result, item, rowLabel, "Đơn vị chủ trì bắt buộc");
                continue;
            }

            if (!TryResolveDonVi(item.TenDonViChuTri, donViRows, out var donViChuTriId, out var dvError)) {
                AddError(result, item, rowLabel, dvError ?? "Không tìm thấy đơn vị chủ trì");
                continue;
            }

            if (!TryResolveDonViMulti(item.TenDonViPhoiHop, donViRows, out var donViPhoiHopIds, out var dvPhError)) {
                AddError(result, item, rowLabel, dvPhError ?? "Không tìm thấy đơn vị phối hợp");
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

            if (!TryResolveUsersMulti(item.TenCanBoPhoiHop, usersInDonVi, out var canBoPhoiHopIds, out var cbPhError)) {
                AddError(result, item, rowLabel, cbPhError ?? "Không tìm thấy cán bộ phối hợp");
                continue;
            }

            if (item.KinhPhi is < 0) {
                AddError(result, item, rowLabel, "Kinh phí không hợp lệ");
                continue;
            }

            validRows.Add(new ValidatedImportRow(
                item,
                duAnId,
                giaiDoanId,
                donViChuTriId,
                chuTriUser.Id,
                donViPhoiHopIds,
                canBoPhoiHopIds));
        }

        if (validRows.Count == 0)
            return result;

        var groups = validRows.GroupBy(r => r.DuAnId);

        foreach (var group in groups) {
            var parent = new KeHoachTrienKhaiHangMuc {
                DuAnId = group.Key,
                BuocId = request.BuocId,
                So = string.Empty,
                NgayToTrinh = null,
                TrichYeu = null,
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

    private static List<string> SplitMulti(string? raw) =>
        string.IsNullOrWhiteSpace(raw)
            ? []
            : raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(s => s.Length > 0)
                .ToList();

    private static bool TryResolveDonVi(
        string tenDonVi,
        List<DonViImportLookup> donVis,
        out long donViId,
        out string? error) {
        donViId = default;
        error = null;

        var trimmed = tenDonVi.Trim();
        var matches = donVis
            .Where(d => string.Equals(d.TenDonVi.Trim(), trimmed, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (matches.Count == 0) {
            error = "Không tìm thấy đơn vị";
            return false;
        }

        if (matches.Count > 1) {
            error = "Đơn vị không xác định (trùng tên)";
            return false;
        }

        donViId = matches[0].Id;
        return true;
    }

    private static bool TryResolveDonViMulti(
        string? raw,
        List<DonViImportLookup> donVis,
        out List<long>? ids,
        out string? error) {
        ids = null;
        error = null;
        var tokens = SplitMulti(raw);
        if (tokens.Count == 0)
            return true;

        var resolved = new List<long>();
        foreach (var token in tokens) {
            if (!TryResolveDonVi(token, donVis, out var id, out error))
                return false;
            resolved.Add(id);
        }

        ids = resolved;
        return true;
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

    private static bool TryResolveUsersMulti(
        string? raw,
        List<UserImportLookup> usersInDonVi,
        out List<long>? ids,
        out string? error) {
        ids = null;
        error = null;
        var tokens = SplitMulti(raw);
        if (tokens.Count == 0)
            return true;

        var resolved = new List<long>();
        foreach (var token in tokens) {
            if (!TryResolveUser(token, usersInDonVi, out var user, out error))
                return false;
            resolved.Add(user.Id);
        }

        ids = resolved;
        return true;
    }

    private static bool IsEmptyRow(KeHoachTrienKhaiHangMucImportDto row) =>
        string.IsNullOrWhiteSpace(row.TenDuAn)
        && string.IsNullOrWhiteSpace(row.TenHangMuc)
        && string.IsNullOrWhiteSpace(row.TenGiaiDoan)
        && string.IsNullOrWhiteSpace(row.TenDonViChuTri)
        && string.IsNullOrWhiteSpace(row.TenDonViPhoiHop)
        && string.IsNullOrWhiteSpace(row.TenCanBoChuTri)
        && string.IsNullOrWhiteSpace(row.TenCanBoPhoiHop)
        && !row.NgayBatDau.HasValue
        && !row.NgayKetThuc.HasValue
        && !row.KinhPhi.HasValue
        && !row.ThoiHan.HasValue;

    private sealed record DonViImportLookup(long Id, string TenDonVi);

    private sealed record UserImportLookup(long Id, string HoTen, long? DonViId);

    private sealed record ValidatedImportRow(
        KeHoachTrienKhaiHangMucImportDto Source,
        Guid DuAnId,
        int GiaiDoanId,
        long DonViChuTriId,
        long CanBoChuTriId,
        List<long>? DonViPhoiHopIds,
        List<long>? CanBoPhoiHopIds);
}
