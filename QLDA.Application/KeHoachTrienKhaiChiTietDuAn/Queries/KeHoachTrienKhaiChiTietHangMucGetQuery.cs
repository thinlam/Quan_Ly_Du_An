using BuildingBlocks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.KeHoachTrienKhaiChiTietDuAns.DTOs;
using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.KeHoachTrienKhaiChiTietDuAns.Queries;

public record KeHoachTrienKhaiChiTietDuAnGetQuery : IRequest<KeHoachTrienKhaiChiTietDuAn> {
    public Guid Id { get; set; }
    public bool ThrowIfNull { get; set; } = true;
    public bool IsNoTracking { get; set; }
}

internal class KeHoachTrienKhaiChiTietDuAnGetQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<KeHoachTrienKhaiChiTietDuAnGetQuery, KeHoachTrienKhaiChiTietDuAn> {
    private readonly IRepository<UserMaster, long> userMaster =
        serviceProvider.GetRequiredService<IRepository<UserMaster, long>>();
    private readonly IRepository<KeHoachTrienKhaiChiTietDuAn, Guid> KeHoachTrienKhaiChiTietDuAn =
        serviceProvider.GetRequiredService<IRepository<KeHoachTrienKhaiChiTietDuAn, Guid>>();
    private readonly IRepository<Domain.Entities.TepDinhKem, Guid> TepDinhKem =
        serviceProvider.GetRequiredService<IRepository<Domain.Entities.TepDinhKem, Guid>>();

    public async Task<KeHoachTrienKhaiChiTietDuAn> Handle(KeHoachTrienKhaiChiTietDuAnGetQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = KeHoachTrienKhaiChiTietDuAn.GetOrderedSet()
            .Where(e => e.Id == request.Id);

        if (request.IsNoTracking)
            queryable = queryable.AsNoTracking();


        var entity = await queryable.FirstOrDefaultAsync(cancellationToken);

        ManagedException.ThrowIf(request.ThrowIfNull && entity == null, "Không tìm thấy dữ liệu");

        return entity!;
    }
}