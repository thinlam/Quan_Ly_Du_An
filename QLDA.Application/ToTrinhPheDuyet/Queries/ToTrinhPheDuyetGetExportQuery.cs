using Microsoft.EntityFrameworkCore;
using QLDA.Application.ToTrinhPheDuyets.DTOs;

namespace QLDA.Application.ToTrinhPheDuyets.Queries;

public record ToTrinhPheDuyetGetExportQuery : IRequest<ToTrinhPheDuyetExportDto>
{
    public Guid Id { get; set; }
    public bool ThrowIfNull { get; set; } = true;
    public bool IsNoTracking { get; set; }
}

internal class ToTrinhPheDuyetGetExportQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<ToTrinhPheDuyetGetExportQuery, ToTrinhPheDuyetExportDto>
{
    private readonly IRepository<ToTrinhPheDuyet, Guid> ToTrinhPheDuyet = serviceProvider.GetRequiredService<IRepository<ToTrinhPheDuyet, Guid>>();
    private readonly IRepository<DmDonVi, long> _dmDonVi = serviceProvider.GetRequiredService<IRepository<DmDonVi, long>>();
    private readonly IRepository<UserMaster, long> userMaster = serviceProvider.GetRequiredService<IRepository<UserMaster, long>>();

    private readonly IRepository<BuildingBlocks.Domain.Entities.TepDinhKem, Guid> TepDinhKem =
        serviceProvider.GetRequiredService<IRepository<BuildingBlocks.Domain.Entities.TepDinhKem, Guid>>();

    public async Task<ToTrinhPheDuyetExportDto> Handle(ToTrinhPheDuyetGetExportQuery request, CancellationToken cancellationToken = default)
    {
        var dmDonViQuery = _dmDonVi.GetQueryableSet().AsNoTracking();
        var queryable = ToTrinhPheDuyet.GetOrderedSet().Where(e => e.Id == request.Id).Include(e=> e.DuAn).AsNoTracking();

        if (request.IsNoTracking)
            queryable = queryable.AsNoTracking();
        var DuAn = queryable.FirstOrDefault()?.DuAn;
        var entity = await queryable
            .Select(x => new ToTrinhPheDuyetExportDto
            {
                So = x.So,
                TrichYeu = x.TrichYeu,
                NgayToTrinh = x.NgayToTrinh,
                DuAnId = x.DuAnId,
                TrangThaiId = x.TrangThaiId,
                MaTrangThai = x.TrangThai!.Ma,
                TenTrangThai = x.TrangThai!.Ten,
                BuocId = x.BuocId,
                Loai = x.Loai,
                Ten = x.Ten,
                TenDuAn = x.DuAn!.TenDuAn,
                CreatedBy = x.CreatedBy,
                TenLanhDaoPhuTrach = userMaster.GetQueryableSet()
                                        .Where(u => u.UserPortalId == x.DuAn!.LanhDaoPhuTrachId)
                                        .Select(u => u.HoTen).FirstOrDefault(),
                

            })
            .FirstOrDefaultAsync(cancellationToken);

        if (entity != null && long.TryParse(entity.CreatedBy, out var userPortalId))
        {
            entity.TenDonViTrinh =
                await (from u in userMaster.GetQueryableSet()
                       join dv in _dmDonVi.GetQueryableSet()
                            on u.PhongBanId equals dv.Id
                       where u.UserPortalId == userPortalId
                       select dv.TenDonVi)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return entity!;
    }
}