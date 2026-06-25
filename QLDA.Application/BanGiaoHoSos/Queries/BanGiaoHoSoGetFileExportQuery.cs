using MediatR;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.BanGiaoHoSos.DTOs;
using QLDA.Domain.Entities;
using QLDA.Domain.Enums;

namespace QLDA.Application.BanGiaoHoSos.Queries;

public record BanGiaoHoSoGetFileExportQuery(Guid Id) : IRequest<BanGiaoHoSoFileExportResultDto>;

internal class BanGiaoHoSoGetFileExportQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<BanGiaoHoSoGetFileExportQuery, BanGiaoHoSoFileExportResultDto>
{
    private readonly IRepository<BanGiaoHoSo, Guid> _banGiaoRepository =
        serviceProvider.GetRequiredService<IRepository<BanGiaoHoSo, Guid>>();
    private readonly IRepository<TepDinhKem, Guid> _tepDinhKemRepository =
        serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
    private readonly IRepository<DuAnBuoc, int> _duAnBuocRepo =
        serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
    private readonly IBuocAuthorizationProvider _buocAuth =
        serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
    private readonly IAuthorizationContext _authContext =
        serviceProvider.GetRequiredService<IAuthorizationContext>();

    public async Task<BanGiaoHoSoFileExportResultDto> Handle(
        BanGiaoHoSoGetFileExportQuery request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _buocAuth.FilterVisibleChildEntities(
                _banGiaoRepository.GetQueryableSet(),
                _duAnBuocRepo,
                _authContext,
                e => e.BuocId)
            .AsNoTracking()
            .Where(e => e.Id == request.Id && e.CreatedBy == _authContext.UserId.ToString())
            .Select(e => new
            {
                e.Id,
                TenDuAn = e.DuAn != null ? e.DuAn.TenDuAn : null,
            })
            .FirstOrDefaultAsync(cancellationToken);

        ManagedException.ThrowIfNull(entity, "Không tìm thấy bản ghi");

        var files = await _tepDinhKemRepository.GetQueryableSet()
            .AsNoTracking()
            .Where(f => f.GroupId == entity.Id.ToString()
                        && f.GroupType == nameof(EGroupType.BanGiaoHoSo))
            .OrderBy(f => f.CreatedAt)
            .Select(f => new BanGiaoHoSoFileExportItemDto
            {
                TenFile = f.OriginalName ?? f.FileName,
                ThoiGianDinhKem = f.CreatedAt,
            })
            .ToListAsync(cancellationToken);

        ManagedException.ThrowIf(files.Count == 0, "Không có dữ liệu để xuất");

        return new BanGiaoHoSoFileExportResultDto
        {
            TenDuAn = entity.TenDuAn,
            Files = files,
        };
    }
}
