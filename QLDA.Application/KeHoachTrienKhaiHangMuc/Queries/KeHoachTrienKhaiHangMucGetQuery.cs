using BuildingBlocks.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace QLDA.Application.KeHoachTrienKhaiHangMucs.Queries;

public record KeHoachTrienKhaiHangMucGetQuery : IRequest<KeHoachTrienKhaiHangMuc> {
    public Guid Id { get; set; }
    public bool ThrowIfNull { get; set; } = true;
    public bool IsNoTracking { get; set; }
}

internal class KeHoachTrienKhaiHangMucGetQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<KeHoachTrienKhaiHangMucGetQuery, KeHoachTrienKhaiHangMuc> {
    private readonly IRepository<UserMaster, long> userMaster =
        serviceProvider.GetRequiredService<IRepository<UserMaster, long>>();
    private readonly IRepository<KeHoachTrienKhaiHangMuc, Guid> KeHoachTrienKhaiHangMuc =
        serviceProvider.GetRequiredService<IRepository<KeHoachTrienKhaiHangMuc, Guid>>();

    public async Task<KeHoachTrienKhaiHangMuc> Handle(KeHoachTrienKhaiHangMucGetQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = KeHoachTrienKhaiHangMuc.GetOrderedSet()
            .Include(e => e.CanBoTrienKhais)
           .ThenInclude(g => g.CanBo)
            .Where(e => e.Id == request.Id);

        if (request.IsNoTracking)
            queryable = queryable.AsNoTracking();


        var entity = await queryable
            .FirstOrDefaultAsync(cancellationToken);

        ManagedException.ThrowIf(request.ThrowIfNull && entity == null, "Không tìm thấy dữ liệu");

        return entity!;
    }
}