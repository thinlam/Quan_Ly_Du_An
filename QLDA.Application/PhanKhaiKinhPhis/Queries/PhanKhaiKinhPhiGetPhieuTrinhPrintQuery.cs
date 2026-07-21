using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.PhanKhaiKinhPhis.DTOs;

namespace QLDA.Application.PhanKhaiKinhPhis.Queries;

public record PhanKhaiKinhPhiGetPhieuTrinhPrintQuery : IRequest<PhanKhaiKinhPhiPhieuTrinhPrintDto> {
    public Guid Id { get; set; }
}

internal class PhanKhaiKinhPhiGetPhieuTrinhPrintQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<PhanKhaiKinhPhiGetPhieuTrinhPrintQuery, PhanKhaiKinhPhiPhieuTrinhPrintDto> {
    private readonly IRepository<PhanKhaiKinhPhi, Guid> _repo =
        serviceProvider.GetRequiredService<IRepository<PhanKhaiKinhPhi, Guid>>();
    private readonly IAuthorizationManager _authManager =
        serviceProvider.GetRequiredService<IAuthorizationManager>();

    public async Task<PhanKhaiKinhPhiPhieuTrinhPrintDto> Handle(
        PhanKhaiKinhPhiGetPhieuTrinhPrintQuery request,
        CancellationToken cancellationToken = default) {
        var entity =await _repo.GetQueryableSet()
            .AsNoTracking()
            .Include(e => e.DuAn)
            .Include(e => e.NguonVon)
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity, "Không tìm thấy dữ liệu phân khai kinh phí để xuất tờ trình.");

        return new PhanKhaiKinhPhiPhieuTrinhPrintDto {
            SoToTrinh = entity.SoToTrinh,
            TrichYeu = entity.TrichYeu,
            NgayToTrinh = entity.NgayToTrinh,
            TenDuAn = entity.DuAn?.TenDuAn,
            TenNguonVon = entity.NguonVon?.Ten,
            ThuyetMinh = entity.ThuyetMinh,
            KinhPhiDeXuat = entity.KinhPhiDeXuat,
            KinhPhiPhanKhai = entity.KinhPhiPhanKhai,
            TongMucDauTu = entity.DuAn?.TongMucDauTu,
        };
    }
}
