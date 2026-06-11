using BuildingBlocks.Domain.Entities;
using BuildingBlocks.Domain.Providers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.BanGiaoHoSos.DTOs;
using QLDA.Domain.Entities;
using QLDA.Domain.Enums;

namespace QLDA.Application.BanGiaoHoSos.Queries;

public record BanGiaoHoSoPrintQuery(Guid Id) : IRequest<BanGiaoHoSoPrintDto>;

internal class BanGiaoHoSoPrintQueryHandler : IRequestHandler<BanGiaoHoSoPrintQuery, BanGiaoHoSoPrintDto> {
    private readonly IRepository<BanGiaoHoSo, Guid> _banGiaoRepository;
    private readonly IRepository<Domain.Entities.TepDinhKem, Guid> _tepDinhKemRepository;
    private readonly IRepository<DmDonVi, long> _donViRepository;

    public BanGiaoHoSoPrintQueryHandler(IServiceProvider serviceProvider) {
        _banGiaoRepository = serviceProvider.GetRequiredService<IRepository<BanGiaoHoSo, Guid>>();
        _tepDinhKemRepository = serviceProvider.GetRequiredService<IRepository<Domain.Entities.TepDinhKem, Guid>>();
        _donViRepository = serviceProvider.GetRequiredService<IRepository<DmDonVi, long>>();
    }

    public async Task<BanGiaoHoSoPrintDto> Handle(BanGiaoHoSoPrintQuery request, CancellationToken cancellationToken = default) {
        var donVis = _donViRepository.GetQueryableSet().AsNoTracking();

        var dto = await _banGiaoRepository.GetQueryableSet()
            .AsNoTracking()
            .Where(e => e.Id == request.Id)
            .Include(e => e.DuAn)
            .LeftOuterJoin(donVis, e => e.PhongBanChuTriId, d => (long?)d.Id, (e, pbChuTri) => new { e, pbChuTri })
            .LeftOuterJoin(donVis, x => x.e.PhongBanNhanId, d => (long?)d.Id, (x, pbNhan) => new { x.e, x.pbChuTri, pbNhan })
            .Select(x => new BanGiaoHoSoPrintDto {
                Ma = x.e.Ma,
                TenHoSo = x.e.TenHoSo,
                TenDuAn = x.e.DuAn!.TenDuAn,
                MaDuAn = x.e.DuAn!.MaDuAn,
                TenPhongBanChuTri = x.pbChuTri != null ? x.pbChuTri.TenDonVi : null,
                TenPhongBanNhan = x.pbNhan != null ? x.pbNhan.TenDonVi : null,
                GhiChu = x.e.GhiChu,
                NgayBanGiao = x.e.NgayBanGiao,
                TongSoTepDinhKem = _tepDinhKemRepository.GetQueryableSet()
                    .Count(f => f.GroupId == x.e.Id.ToString() && f.GroupType == nameof(EGroupType.BanGiaoHoSo))
            })
            .FirstOrDefaultAsync(cancellationToken);

        ManagedException.ThrowIfNull(dto, "Không tìm thấy bản ghi");
        return dto;
    }
}
