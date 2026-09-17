using Microsoft.EntityFrameworkCore;
using QLDA.Application.DanhMucQuyTrinhs.DTOs;

namespace QLDA.Application.DanhMucQuyTrinhs.Queries;

public record DanhMucQuyTrinhGetQuery(int Id, bool ThrowIfNull = true, bool IsNoTracking = true) : IRequest<DanhMucQuyTrinhDto>;

public record DanhMucQuyTrinhGetQueryHandler(IServiceProvider ServiceProvider)
    : IRequestHandler<DanhMucQuyTrinhGetQuery, DanhMucQuyTrinhDto> {
    private readonly IRepository<DanhMucQuyTrinh, int> DanhMucQuyTrinh =
        ServiceProvider.GetRequiredService<IRepository<DanhMucQuyTrinh, int>>();

    public async Task<DanhMucQuyTrinhDto> Handle(DanhMucQuyTrinhGetQuery request, CancellationToken cancellationToken) {
        var query = DanhMucQuyTrinh.GetQueryableSet()
            .Where(e => e.Id == request.Id && !e.IsDeleted);

        if (request.IsNoTracking)
            query = query.AsNoTracking();

        var entity = await query.FirstOrDefaultAsync(cancellationToken);

        ManagedException.ThrowIf(request.ThrowIfNull && entity == null, "Danh mục quy trình không tồn tại.");

        return entity!.ToDto();
    }
}