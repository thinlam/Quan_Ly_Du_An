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

        var duAnDict = await _duAnRepo.GetQueryableSet()
            .Where(e => tenDuAns.Contains(e.TenDuAn!))
            .Select(e => new { e.TenDuAn, e.Id })
            .ToListAsync(cancellationToken);
        var duAnByTen = duAnDict
            .DistinctBy(e => e.TenDuAn)
            .ToDictionary(e => e.TenDuAn!, e => e.Id);

        var nguonVonByDuAnAndTen = await _duAnRepo.GetQueryableSet()
            .Where(e => tenDuAns.Contains(e.TenDuAn!))
            .Include(e => e.DuAnNguonVons!)
            .ThenInclude(dnv => dnv.NguonVon)
            .ToListAsync(cancellationToken);
        var nguonVonLookup = nguonVonByDuAnAndTen
            .SelectMany(e => (e.DuAnNguonVons ?? [])
                .Where(dnv => dnv.NguonVon?.Ten != null)
                .Select(dnv => new {
                    TenDuAn = e.TenDuAn!,
                    TenNguonVon = dnv.NguonVon!.Ten!,
                    NguonVonId = dnv.RightId,
                }))
            .DistinctBy(x => (x.TenDuAn, x.TenNguonVon))
            .ToDictionary(x => (x.TenDuAn, x.TenNguonVon), x => x.NguonVonId);

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
                var (tenNguonVon, tenDuAnFromDisplay) =
                    PhanKhaiKinhPhiImportDisplay.Parse(item.TenNguonVon);

                if (tenDuAnFromDisplay == null) {
                    result.Errors.Add($"{rowLabel}: Không tìm thấy nguồn vốn");
                    result.ErrorCount++;
                    continue;
                }

                if (!string.Equals(tenDuAnFromDisplay, item.TenDuAn, StringComparison.Ordinal)) {
                    result.Errors.Add($"{rowLabel}: Nguồn vốn không thuộc dự án đã chọn");
                    result.ErrorCount++;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(tenNguonVon)
                    || !nguonVonLookup.TryGetValue((item.TenDuAn, tenNguonVon), out var nvId)) {
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
