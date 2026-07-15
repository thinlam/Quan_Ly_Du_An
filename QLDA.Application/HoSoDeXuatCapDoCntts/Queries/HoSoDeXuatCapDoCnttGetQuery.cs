using Microsoft.EntityFrameworkCore;

namespace QLDA.Application.HoSoDeXuatCapDoCntts.Queries;

public record HoSoDeXuatCapDoCnttGetQuery : IRequest<HoSoDeXuatCapDoCntt> {
    public Guid Id { get; set; }
}

internal class HoSoDeXuatCapDoCnttGetQueryHandler : IRequestHandler<HoSoDeXuatCapDoCnttGetQuery, HoSoDeXuatCapDoCntt> {
    private readonly IRepository<HoSoDeXuatCapDoCntt, Guid> HoSoDeXuatCapDoCntt;

    public HoSoDeXuatCapDoCnttGetQueryHandler(IServiceProvider serviceProvider) {
        HoSoDeXuatCapDoCntt = serviceProvider.GetRequiredService<IRepository<HoSoDeXuatCapDoCntt, Guid>>();
    }

    public async Task<HoSoDeXuatCapDoCntt> Handle(HoSoDeXuatCapDoCnttGetQuery request, CancellationToken cancellationToken = default) {
        var entity = await HoSoDeXuatCapDoCntt.GetQueryableSet()
            .AsNoTracking()
            .Include(e => e.CapDo)
            .FirstOrDefaultAsync(e => e.Id == request.Id && !e.IsDeleted, cancellationToken);
        ManagedException.ThrowIfNull(entity);
        return entity!;
    }
}