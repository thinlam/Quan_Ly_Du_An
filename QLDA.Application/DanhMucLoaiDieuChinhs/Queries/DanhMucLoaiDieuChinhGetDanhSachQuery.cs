using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Mapping;
using QLDA.Application.DanhMucLoaiDieuChinhs.DTOs;

namespace QLDA.Application.DanhMucLoaiDieuChinhs.Queries;

public record DanhMucLoaiDieuChinhGetDanhSachQuery(bool GetAll = false) : IRequest<PaginatedList<DanhMucLoaiDieuChinhDto>>;

internal class DanhMucLoaiDieuChinhGetDanhSachQueryHandler : IRequestHandler<DanhMucLoaiDieuChinhGetDanhSachQuery, PaginatedList<DanhMucLoaiDieuChinhDto>> {
    private readonly IRepository<DanhMucLoaiDieuChinh, int> DanhMucLoaiDieuChinh;

    public DanhMucLoaiDieuChinhGetDanhSachQueryHandler(IServiceProvider serviceProvider) {
        DanhMucLoaiDieuChinh = serviceProvider.GetRequiredService<IRepository<DanhMucLoaiDieuChinh, int>>();
    }

    public async Task<PaginatedList<DanhMucLoaiDieuChinhDto>> Handle(DanhMucLoaiDieuChinhGetDanhSachQuery request, CancellationToken cancellationToken) {
        var query = DanhMucLoaiDieuChinh.GetQueryableSet()
            .OrderBy(x => x.Stt)
            .Select(e => new DanhMucLoaiDieuChinhDto {
                Id = e.Id,
                Ma = e.Ma,
                Ten = e.Ten,
                MoTa = e.MoTa,
                Stt = e.Stt,
                Used = e.Used,
            });

        if (request.GetAll) {
            return await query.PaginatedListAsync(0, 0, cancellationToken);
        }

        return await query.PaginatedListAsync(0, int.MaxValue, cancellationToken);
    }
}