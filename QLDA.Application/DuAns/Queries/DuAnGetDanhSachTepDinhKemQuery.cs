using Microsoft.EntityFrameworkCore;
using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.DuAns.Queries;

public class DuAnGetDanhSachTepDinhKemQuery : IRequest<List<TepDinhKemDto>> {
    public Guid DuAnId { get; set; }
}

internal class DuAnGetDanhSachTepDinhKemQueryHandler(
    IServiceProvider serviceProvider)
    : IRequestHandler<DuAnGetDanhSachTepDinhKemQuery, List<TepDinhKemDto>> {

    private readonly IRepository<Attachment, Guid> _tepDinhKemRepo =
        serviceProvider.GetRequiredService<IRepository<Attachment, Guid>>();

    public async Task<List<TepDinhKemDto>> Handle(
        DuAnGetDanhSachTepDinhKemQuery request,
        CancellationToken cancellationToken = default) {

        var groupIds = await DuAnTepDinhKemGroupIdQueryExtensions.ResolveGroupIdsAsync(
            request.DuAnId, serviceProvider, cancellationToken);

        var files = await _tepDinhKemRepo.GetQueryableSet()
            .Where(f => groupIds.Contains(f.GroupId) && !f.IsDeleted)
            .ToListAsync(cancellationToken);

        return files.ToDtos();
    }
}
