using Microsoft.EntityFrameworkCore;
using QLDA.Application.DanhMucLoaiDieuChinhs.DTOs;

namespace QLDA.Application.DanhMucLoaiDieuChinhs.Queries;

public record DanhMucLoaiDieuChinhGetByIdQuery(int Id) : IRequest<DanhMucLoaiDieuChinhDto?>;

internal class DanhMucLoaiDieuChinhGetByIdQueryHandler : IRequestHandler<DanhMucLoaiDieuChinhGetByIdQuery, DanhMucLoaiDieuChinhDto?> {
    private readonly IRepository<DanhMucLoaiDieuChinh, int> DanhMucLoaiDieuChinh;

    public DanhMucLoaiDieuChinhGetByIdQueryHandler(IServiceProvider serviceProvider) {
        DanhMucLoaiDieuChinh = serviceProvider.GetRequiredService<IRepository<DanhMucLoaiDieuChinh, int>>();
    }

    public async Task<DanhMucLoaiDieuChinhDto?> Handle(DanhMucLoaiDieuChinhGetByIdQuery request, CancellationToken cancellationToken) {
        var entity = await DanhMucLoaiDieuChinh.GetOrderedSet().FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
        if (entity == null) return null;

        return new DanhMucLoaiDieuChinhDto {
            Id = entity.Id,
            Ma = entity.Ma,
            Ten = entity.Ten,
            MoTa = entity.MoTa,
            Used = entity.Used,
        };
    }
}