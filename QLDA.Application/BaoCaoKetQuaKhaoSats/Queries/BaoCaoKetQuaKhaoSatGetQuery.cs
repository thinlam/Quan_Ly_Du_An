using Microsoft.EntityFrameworkCore;

namespace QLDA.Application.BaoCaoKetQuaKhaoSats.Queries;

public record BaoCaoKetQuaKhaoSatGetQuery : IRequest<BaoCaoKetQuaKhaoSat>
{
    public Guid Id { get; set; }
}

internal class BaoCaoKetQuaKhaoSatGetQueryHandler
    : IRequestHandler<BaoCaoKetQuaKhaoSatGetQuery, BaoCaoKetQuaKhaoSat>
{
    private readonly IRepository<BaoCaoKetQuaKhaoSat, Guid> _repository;

    public BaoCaoKetQuaKhaoSatGetQueryHandler(IServiceProvider serviceProvider)
    {
        _repository = serviceProvider.GetRequiredService<IRepository<BaoCaoKetQuaKhaoSat, Guid>>();
    }

    public async Task<BaoCaoKetQuaKhaoSat> Handle(
        BaoCaoKetQuaKhaoSatGetQuery request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetQueryableSet()
            .AsNoTracking()
            .Include(e => e.TrangThai)
            .FirstOrDefaultAsync(e => e.Id == request.Id && !e.IsDeleted, cancellationToken);
        ManagedException.ThrowIfNull(entity);
        return entity!;
    }
}
