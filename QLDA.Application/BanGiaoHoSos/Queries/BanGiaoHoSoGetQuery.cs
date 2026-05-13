using MediatR;
using Microsoft.EntityFrameworkCore;
using QLDA.Domain.Entities;

namespace QLDA.Application.BanGiaoHoSos.Queries;

public record BanGiaoHoSoGetQuery(Guid Id) : IRequest<BanGiaoHoSo>;

internal class BanGiaoHoSoGetQueryHandler : IRequestHandler<BanGiaoHoSoGetQuery, BanGiaoHoSo> {
    private readonly IRepository<BanGiaoHoSo, Guid> _repository;

    public BanGiaoHoSoGetQueryHandler(IServiceProvider serviceProvider) {
        _repository = serviceProvider.GetRequiredService<IRepository<BanGiaoHoSo, Guid>>();
    }

    public async Task<BanGiaoHoSo> Handle(BanGiaoHoSoGetQuery request, CancellationToken cancellationToken = default) {
        var entity = await _repository.GetQueryableSet()
            .AsNoTracking()
            .Include(e => e.User)
            .Include(e => e.PhongBanChuTri)
            .Include(e => e.DuAn)
            .Include(e => e.Buoc)
            .FirstOrDefaultAsync(e => e.Id == request.Id && !e.IsDeleted, cancellationToken);
        ManagedException.ThrowIfNull(entity);
        return entity;
    }
}
