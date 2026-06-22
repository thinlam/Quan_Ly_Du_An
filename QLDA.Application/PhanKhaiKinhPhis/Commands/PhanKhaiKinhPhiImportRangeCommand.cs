using Microsoft.EntityFrameworkCore;
using QLDA.Application.PhanKhaiKinhPhis.DTOs;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.PhanKhaiKinhPhis.Commands;

public record PhanKhaiKinhPhiImportRangeCommand(List<PhanKhaiKinhPhiImportDto> Imports)
    : IRequest<PhanKhaiKinhPhiImportResultDto>;

internal class PhanKhaiKinhPhiImportRangeCommandHandler(IServiceProvider serviceProvider)
    : IRequestHandler<PhanKhaiKinhPhiImportRangeCommand, PhanKhaiKinhPhiImportResultDto> {
    private readonly IRepository<PhanKhaiKinhPhi, Guid> _repo =
        serviceProvider.GetRequiredService<IRepository<PhanKhaiKinhPhi, Guid>>();
    private readonly IRepository<DuAn, Guid> _duAnRepo =
        serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
    private readonly IRepository<DanhMucNguonVon, int> _nguonVonRepo =
        serviceProvider.GetRequiredService<IRepository<DanhMucNguonVon, int>>();
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo =
        serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();

    public async Task<PhanKhaiKinhPhiImportResultDto> Handle(
        PhanKhaiKinhPhiImportRangeCommand request,
        CancellationToken cancellationToken = default) {
        var result = new PhanKhaiKinhPhiImportResultDto();
        var rows = request.Imports.Where(row => !IsEmptyRow(row)).ToList();

        if (rows.Count == 0)
            return result;

        var tenDuAns = rows
            .Where(r => !string.IsNullOrWhiteSpace(r.TenDuAn))
            .Select(r => r.TenDuAn!)
            .Distinct()
            .ToList();

        var tenNguonVons = rows
            .Where(r => !string.IsNullOrWhiteSpace(r.TenNguonVon))
            .Select(r => r.TenNguonVon!)
            .Distinct()
            .ToList();

        var duAnDict = await _duAnRepo.GetQueryableSet()
            .Where(e => tenDuAns.Contains(e.TenDuAn!))
            .Select(e => new { e.TenDuAn, e.Id })
            .ToListAsync(cancellationToken);
        var duAnByTen = duAnDict
            .DistinctBy(e => e.TenDuAn)
            .ToDictionary(e => e.TenDuAn!, e => e.Id);

        var nguonVonDict = await _nguonVonRepo.GetQueryableSet()
            .Where(e => tenNguonVons.Contains(e.Ten!))
            .Select(e => new { e.Ten, e.Id })
            .ToListAsync(cancellationToken);
        var nguonVonByTen = nguonVonDict
            .DistinctBy(e => e.Ten)
            .ToDictionary(e => e.Ten!, e => e.Id);

        var trangThaiDuThao = await _statusRepo
            .GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(
                s => s.Ma == TrangThaiPheDuyetCodes.PhanKhaiKinhPhi.DuThao
                     && s.Loai == PheDuyetEntityNames.PhanKhaiKinhPhi,
                cancellationToken);

        var validEntities = new List<PhanKhaiKinhPhi>();

        foreach (var item in rows) {
            var rowLabel = item.ExcelRowNumber > 0
                ? $"Dòng {item.ExcelRowNumber}"
                : "Dòng";

            if (string.IsNullOrWhiteSpace(item.TenDuAn)) {
                result.Errors.Add($"{rowLabel}: Dự án bắt buộc");
                result.ErrorCount++;
                continue;
            }

            if (!duAnByTen.TryGetValue(item.TenDuAn, out var duAnId)) {
                result.Errors.Add($"{rowLabel}: Không tìm thấy dự án");
                result.ErrorCount++;
                continue;
            }

            int? nguonVonId = null;
            if (!string.IsNullOrWhiteSpace(item.TenNguonVon)) {
                if (!nguonVonByTen.TryGetValue(item.TenNguonVon, out var nvId)) {
                    result.Errors.Add($"{rowLabel}: Không tìm thấy nguồn vốn");
                    result.ErrorCount++;
                    continue;
                }

                nguonVonId = nvId;
            }

            if (item.KinhPhiDeXuat is < 0) {
                result.Errors.Add($"{rowLabel}: Kinh phí đề xuất không hợp lệ");
                result.ErrorCount++;
                continue;
            }

            if (item.KinhPhiPhanKhai is < 0) {
                result.Errors.Add($"{rowLabel}: Kinh phí phân khai không hợp lệ");
                result.ErrorCount++;
                continue;
            }

            validEntities.Add(new PhanKhaiKinhPhi {
                DuAnId = duAnId,
                NguonVonId = nguonVonId,
                KinhPhiDeXuat = item.KinhPhiDeXuat.HasValue
                    ? (decimal)item.KinhPhiDeXuat.Value
                    : null,
                KinhPhiPhanKhai = item.KinhPhiPhanKhai.HasValue
                    ? (decimal)item.KinhPhiPhanKhai.Value
                    : null,
                ThuyetMinh = item.ThuyetMinhPhanKhai,
                SoToTrinh = item.SoToTrinh,
                NgayToTrinh = item.NgayToTrinh,
                TrichYeu = item.TrichYeu,
                TrangThaiId = trangThaiDuThao?.Id,
            });
        }

        foreach (var entity in validEntities)
            await _repo.AddAsync(entity, cancellationToken);

        if (validEntities.Count > 0)
            await _repo.UnitOfWork.SaveChangesAsync(cancellationToken);

        result.SuccessCount = validEntities.Count;
        return result;
    }

    private static bool IsEmptyRow(PhanKhaiKinhPhiImportDto row) =>
        string.IsNullOrWhiteSpace(row.TenDuAn)
        && string.IsNullOrWhiteSpace(row.TenNguonVon)
        && !row.KinhPhiDeXuat.HasValue
        && !row.KinhPhiPhanKhai.HasValue
        && string.IsNullOrWhiteSpace(row.ThuyetMinhPhanKhai)
        && string.IsNullOrWhiteSpace(row.SoToTrinh)
        && !row.NgayToTrinh.HasValue
        && string.IsNullOrWhiteSpace(row.TrichYeu);
}
