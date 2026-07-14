using Microsoft.EntityFrameworkCore;
using QLDA.Application.DanhMucBuocs.DTOs;
using QLDA.Application.DuAnBuocs.Extensions;

namespace QLDA.Application.DanhMucBuocs.Queries;

public record DanhMucBuocGetTreeListQuery : IRequest<List<DanhMucBuocDto>> {
    public int QuyTrinhId { get; set; }
    public bool IsNoTracking { get; set; } = true;
}

internal class DanhMucBuocGetTreeListQueryHandler(IServiceProvider ServiceProvider)
    : IRequestHandler<DanhMucBuocGetTreeListQuery, List<DanhMucBuocDto>> {
    private readonly IRepository<DanhMucBuoc, int> DanhMucBuocRepository =
        ServiceProvider.GetRequiredService<IRepository<DanhMucBuoc, int>>();
    private readonly IRepository<DanhMucManHinh, int> DanhMucManHinh =
        ServiceProvider.GetRequiredService<IRepository<DanhMucManHinh, int>>();

    public async Task<List<DanhMucBuocDto>> Handle(DanhMucBuocGetTreeListQuery query,
        CancellationToken cancellationToken = default) {
        var queryable = DanhMucBuocRepository.GetQueryableSet()
            .Where(o => o.QuyTrinhId == query.QuyTrinhId);

        if (query.IsNoTracking)
            queryable = queryable.AsNoTracking();

        var all = await queryable.ToListAsync(cancellationToken: cancellationToken);

        var orderedSteps = all.ToSteps().ToTreeList();
        return [.. orderedSteps.Join(all, step => step.BuocId, origin => origin.Id,
                (step, origin) => new { step, origin })
            .Select(e => new DanhMucBuocDto() {
                Id = e.step.Id,
                Ten = e.step.Ten,
                QuyTrinhId = e.step.Id,
                GiaiDoanId = e.origin.GiaiDoanId,
                ParentId = e.step.ParentId,
                Level = e.step.Level,
                PartialView = e.step.PartialView,
                Path = e.step.Path,
                Stt = e.step.Stt,
            })];
    }
}