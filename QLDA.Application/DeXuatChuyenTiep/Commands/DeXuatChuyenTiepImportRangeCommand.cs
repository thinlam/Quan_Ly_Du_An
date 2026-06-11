using Microsoft.EntityFrameworkCore;
using QLDA.Application.DeXuatChuyenTieps.DTOs;
using QLDA.Domain.Constants;
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

    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo =
        serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();

    public async Task Handle(
        DeXuatChuyenTiepImportRangeCommand request,
        CancellationToken cancellationToken = default) {
        if (request.DuAnId == Guid.Empty || request.BuocId <= 0)
            return;

        var trangThaiDuThao = await _statusRepo
            .GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(
                s => s.Ma == TrangThaiPheDuyetCodes.DeXuatMacDinh.DuThao
                     && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt,
                cancellationToken);

        foreach (var item in request.Imports.Where(row => !IsEmptyRow(row))) {
            await _repo.AddAsync(new DeXuatChuyenTiep {
                DuAnId = request.DuAnId,
                BuocId = request.BuocId,
                SoLieuGiaiNgan = item.SoLieuGiaiNgan,
                UocGiaiNgan = item.UocGiaiNgan,
                NhuCauKinhPhi = item.NhuCauKinhPhi,
                KhoiLuongThucTe = item.KhoiLuongThucTe,
                KhoiLuongDuKien = item.KhoiLuongDuKien,
                NamDeXuat = DateTime.Now.Year,
                TrangThaiId = trangThaiDuThao?.Id,
            }, cancellationToken);
        }

        await _repo.UnitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static bool IsEmptyRow(DeXuatChuyenTiepImportDto row) =>
        !row.SoLieuGiaiNgan.HasValue
        && !row.UocGiaiNgan.HasValue
        && !row.NhuCauKinhPhi.HasValue
        && string.IsNullOrWhiteSpace(row.KhoiLuongThucTe)
        && string.IsNullOrWhiteSpace(row.KhoiLuongDuKien);
}
