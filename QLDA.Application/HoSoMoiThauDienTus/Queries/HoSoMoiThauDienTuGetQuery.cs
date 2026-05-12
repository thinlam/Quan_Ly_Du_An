using Microsoft.EntityFrameworkCore;

namespace QLDA.Application.HoSoMoiThauDienTus.Queries;

public record HoSoMoiThauDienTuGetQuery : IRequest<HoSoMoiThauDienTu> {
    public Guid Id { get; set; }
}

internal class HoSoMoiThauDienTuGetQueryHandler : IRequestHandler<HoSoMoiThauDienTuGetQuery, HoSoMoiThauDienTu> {
    private readonly IRepository<HoSoMoiThauDienTu, Guid> HoSoMoiThauDienTu;

    public HoSoMoiThauDienTuGetQueryHandler(IServiceProvider serviceProvider) {
        HoSoMoiThauDienTu = serviceProvider.GetRequiredService<IRepository<HoSoMoiThauDienTu, Guid>>();
    }

    public async Task<HoSoMoiThauDienTu> Handle(HoSoMoiThauDienTuGetQuery request, CancellationToken cancellationToken = default) {
        var entity = await HoSoMoiThauDienTu.GetQueryableSet()
            .AsNoTracking()
            .Include(e => e.DuAn)
            .Include(e => e.Buoc)
            .Include(e => e.HinhThucLuaChonNhaThau)
            .Include(e => e.GoiThau)
            .Include(e => e.TrangThaiPheDuyet)
            .FirstOrDefaultAsync(e => e.Id == request.Id && !e.IsDeleted, cancellationToken);
        ManagedException.ThrowIfNull(entity);
        return entity;
    }
}