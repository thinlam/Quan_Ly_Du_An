using Microsoft.EntityFrameworkCore;

namespace QLDA.Application.KySos.Queries;

public record KySoGetQuery : IRequest<KySo> {
    public Guid Id { get; set; }
}

internal class KySoGetQueryHandler : IRequestHandler<KySoGetQuery, KySo> {
    private readonly IRepository<KySo, Guid> _kySo;

    public KySoGetQueryHandler(IServiceProvider serviceProvider) {
        _kySo = serviceProvider.GetRequiredService<IRepository<KySo, Guid>>();
    }

    public async Task<KySo> Handle(KySoGetQuery request, CancellationToken cancellationToken = default) {
        var entity = await _kySo.GetQueryableSet().AsNoTracking()
            .Include(e => e.PhuongThucKySo)
            .Include(e => e.ChucVu)
            .FirstOrDefaultAsync(e => e.Id == request.Id && !e.IsDeleted, cancellationToken);
        ManagedException.ThrowIfNull(entity);
        return entity;
    }
}