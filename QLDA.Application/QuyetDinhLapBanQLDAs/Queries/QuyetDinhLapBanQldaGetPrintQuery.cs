using BuildingBlocks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.QuyetDinhLapBanQLDAs.DTOs;
using QLDA.Domain.Entities;

namespace QLDA.Application.QuyetDinhLapBanQLDAs.Queries;

public class QuyetDinhLapBanQldaGetPrintQuery : IRequest<QuyetDinhLapBanQldaPrintDto>
{
    public Guid Id { get; set; }
    public bool ThrowIfNull { get; set; } = true;
}

internal class QuyetDinhLapBanQldaGetPrintQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<QuyetDinhLapBanQldaGetPrintQuery, QuyetDinhLapBanQldaPrintDto>
{
    private readonly IRepository<QuyetDinhLapBanQLDA, Guid> _quyetDinh =
        serviceProvider.GetRequiredService<IRepository<QuyetDinhLapBanQLDA, Guid>>();

    private readonly IRepository<UserMaster, long> _userMaster =
        serviceProvider.GetRequiredService<IRepository<UserMaster, long>>();

    public async Task<QuyetDinhLapBanQldaPrintDto> Handle(
        QuyetDinhLapBanQldaGetPrintQuery request,
        CancellationToken cancellationToken = default)
    {
        var userMasterQuery = _userMaster.GetQueryableSet().AsNoTracking();

        var dto = await _quyetDinh.GetOrderedSet()
            .AsNoTracking()
            .Where(e => e.Id == request.Id)
            .Select(x => new QuyetDinhLapBanQldaPrintDto
            {
                Id = x.Id,
                DuAnId = x.DuAnId,
                So = x.So,
                TrichYeu = x.TrichYeu,
                SoDuThao = x.SoDuThao,
                TrichYeuDuThao = x.TrichYeuDuThao,
                LanhDaoPhuTrachId = x.DuAn!.LanhDaoPhuTrachId,
                // Join UserPortalId (không dùng u.Id). FirstOrDefault → null nếu không match.
                TenLanhDaoPhuTrach = userMasterQuery
                    .Where(u => u.UserPortalId == x.DuAn!.LanhDaoPhuTrachId)
                    .Select(u => u.HoTen)
                    .FirstOrDefault(),
                ThanhViens = x.ThanhViens
                    .Select(tv => new ThanhVienBanQldaDto
                    {
                        Id = tv.Id,
                        Ten = tv.Ten,
                        ChucVu = tv.ChucVu,
                        VaiTro = tv.VaiTro
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        ManagedException.ThrowIf(request.ThrowIfNull && dto == null, "Không tìm thấy dữ liệu");

        return dto!;
    }
}
