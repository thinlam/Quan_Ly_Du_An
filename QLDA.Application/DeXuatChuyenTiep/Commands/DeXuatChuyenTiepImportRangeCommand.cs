using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.DeXuatChuyenTieps.DTOs;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.DeXuatChuyenTieps.Commands;

public record DeXuatChuyenTiepImportRangeCommand(List<DeXuatChuyenTiepImportDto> Imports) : IRequest {
    public Guid DuAnId { get; init; }
    public int BuocId { get; init; }
}

internal class DeXuatChuyenTiepImportRangeCommandHandler(IServiceProvider serviceProvider)
    : IRequestHandler<DeXuatChuyenTiepImportRangeCommand> {
    private readonly IRepository<DeXuatChuyenTiep, Guid> _repo =
        serviceProvider.GetRequiredService<IRepository<DeXuatChuyenTiep, Guid>>();

    private readonly IRepository<DuAn, Guid> _duAnRepo =
        serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();

    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo =
        serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();

    private readonly IBuocAuthorizationProvider _auth =
        serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
    private readonly IAuthorizationContext _authContext =
        serviceProvider.GetRequiredService<IAuthorizationContext>();

    public async Task Handle(
        DeXuatChuyenTiepImportRangeCommand request,
        CancellationToken cancellationToken = default) {
        var importTrongDuAn = request.DuAnId != Guid.Empty;

        if (importTrongDuAn && request.BuocId <= 0)
            return;

        // Check step authorization for import within DuAn
        if (importTrongDuAn)
            await _auth.EnsureCanExecuteStepAsync(request.BuocId, _authContext, cancellationToken);

        Dictionary<string, (Guid Id, int? BuocHienTaiId)>? duAnByTen = null;
        if (!importTrongDuAn) {
            var tenDuAns = request.Imports
                .Where(row => !IsEmptyRow(row))
                .Select(row => row.TenDuAn)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct()
                .ToList();

            if (tenDuAns.Count == 0)
                return;

            var duAns = await _duAnRepo.GetOrderedSet()
                .Where(e => tenDuAns.Contains(e.TenDuAn!))
                .Where(e => e.TrangThaiDuAn!.Ma == "DTH")
                .Select(e => new { e.TenDuAn, e.Id, e.BuocHienTaiId })
                .ToListAsync(cancellationToken);

            duAnByTen = duAns
                .DistinctBy(e => e.TenDuAn)
                .ToDictionary(e => e.TenDuAn!, e => (e.Id, e.BuocHienTaiId));
        }

        if (!request.Imports.Any(row => !IsEmptyRow(row)))
            return;

        ValidateNamDeXuat(request.Imports);

        var trangThaiDuThao = await _statusRepo
            .GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(
                s => s.Ma == TrangThaiPheDuyetCodes.DeXuatMacDinh.DuThao
                     && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt,
                cancellationToken);

        foreach (var item in request.Imports.Where(row => !IsEmptyRow(row))) {
            Guid duAnId;
            int buocId;

            if (importTrongDuAn) {
                duAnId = request.DuAnId;
                buocId = request.BuocId;
            } else {
                if (string.IsNullOrWhiteSpace(item.TenDuAn)
                    || duAnByTen == null
                    || !duAnByTen.TryGetValue(item.TenDuAn, out var duAn)
                    || duAn.BuocHienTaiId is not int buocHienTai
                    || buocHienTai <= 0)
                    continue;

                duAnId = duAn.Id;
                buocId = buocHienTai;
            }

            await _repo.AddAsync(new DeXuatChuyenTiep {
                DuAnId = duAnId,
                BuocId = buocId,
                SoLieuGiaiNgan = item.SoLieuGiaiNgan,
                UocGiaiNgan = item.UocGiaiNgan,
                NhuCauKinhPhi = item.NhuCauKinhPhi,
                KhoiLuongThucTe = item.KhoiLuongThucTe,
                KhoiLuongDuKien = item.KhoiLuongDuKien,
                NamDeXuat = item.NamDeXuat,
                TrangThaiId = trangThaiDuThao?.Id,
            }, cancellationToken);
        }

        await _repo.UnitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static void ValidateNamDeXuat(IReadOnlyList<DeXuatChuyenTiepImportDto> rows) {
        for (var i = 0; i < rows.Count; i++) {
            var item = rows[i];
            if (IsEmptyRow(item))
                continue;

            var rowLabel = $"dòng {i + 1}";

            if (!item.NamDeXuat.HasValue)
                ManagedException.Throw(
                    $"Trường Năm ({rowLabel}) không được để trống hoặc không parse được số nguyên.");

            if (item.NamDeXuat <= 0)
                ManagedException.Throw(
                    $"Trường Năm ({rowLabel}) không hợp lệ. Năm phải lớn hơn 0.");
        }
    }

    private static bool IsEmptyRow(DeXuatChuyenTiepImportDto row) =>
        string.IsNullOrWhiteSpace(row.TenDuAn)
        && !row.SoLieuGiaiNgan.HasValue
        && !row.UocGiaiNgan.HasValue
        && !row.NhuCauKinhPhi.HasValue
        && string.IsNullOrWhiteSpace(row.KhoiLuongThucTe)
        && string.IsNullOrWhiteSpace(row.KhoiLuongDuKien);
}
