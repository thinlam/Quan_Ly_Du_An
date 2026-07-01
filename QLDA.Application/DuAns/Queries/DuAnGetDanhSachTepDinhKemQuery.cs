using Microsoft.EntityFrameworkCore;
using QLDA.Application.DuAns.Services;
using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.DuAns.Queries;

public class DuAnGetDanhSachTepDinhKemQuery : IRequest<List<TepDinhKemDto>> {
    public Guid DuAnId { get; set; }
}

internal class DuAnGetDanhSachTepDinhKemQueryHandler(
    IServiceProvider serviceProvider)
    : IRequestHandler<DuAnGetDanhSachTepDinhKemQuery, List<TepDinhKemDto>> {

    private readonly IRepository<TepDinhKem, Guid> _tepDinhKemRepo =
        serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
    private readonly DuAnTepDinhKemGroupIdResolver _groupIdResolver =
        serviceProvider.GetRequiredService<DuAnTepDinhKemGroupIdResolver>();

    public async Task<List<TepDinhKemDto>> Handle(
        DuAnGetDanhSachTepDinhKemQuery request,
        CancellationToken cancellationToken = default) {

        var groupIds = await _groupIdResolver.ResolveGroupIdsAsync(
            request.DuAnId, cancellationToken);

        var files = await _tepDinhKemRepo.GetQueryableSet()
            .Where(f => groupIds.Contains(f.GroupId) && !f.IsDeleted)
            .ToListAsync(cancellationToken);

        return files.ToDtos();
    }
}
