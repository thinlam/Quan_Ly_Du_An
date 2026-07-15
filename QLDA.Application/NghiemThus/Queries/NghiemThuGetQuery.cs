using Microsoft.EntityFrameworkCore;

namespace QLDA.Application.NghiemThus.Queries;

public class NghiemThuGetQuery : IRequest<NghiemThu> {
    public Guid Id { get; set; }
    public bool IncludePhuLucHopDong { get; set; }
    public bool IncludeThanhToan { get; set; }
    public bool ThrowIfNull { get; set; } = true;
    public bool IsNoTracking { get; set; }
}

internal class NghiemThuGetQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<NghiemThuGetQuery, NghiemThu> {
    private readonly IRepository<NghiemThu, Guid> NghiemThu =
        serviceProvider.GetRequiredService<IRepository<NghiemThu, Guid>>();

    private readonly IRepository<Attachment, Guid> TepDinhKem =
        serviceProvider.GetRequiredService<IRepository<Attachment, Guid>>();


    public async Task<NghiemThu> Handle(NghiemThuGetQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = NghiemThu.GetOrderedSet()
            .WhereFunc(request.IncludePhuLucHopDong, q => q.Include(i => i.NghiemThuPhuLucHopDongs))
            .WhereFunc(request.IncludeThanhToan, q => q.Include(i => i.ThanhToan))
            .WhereFunc(request.IsNoTracking, q => q.AsNoTracking())
            .Where(e => e.Id == request.Id);



        var entity = await queryable
            .FirstOrDefaultAsync(cancellationToken);

        ManagedException.ThrowIf(request.ThrowIfNull && entity == null, "Không tìm thấy dữ liệu");

        return entity!;
    }
}