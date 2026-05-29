using Microsoft.EntityFrameworkCore;

namespace QLDA.Application.ToTrinhKetQuaGoiThaus.Queries;

public record ToTrinhKetQuaGoiThauGetQuery : IRequest<ToTrinhKetQuaGoiThau> {
    public Guid Id { get; set; }
    public bool ThrowIfNull { get; set; } = true;
    public bool IsNoTracking { get; set; }
}

internal class ToTrinhKetQuaGoiThauGetQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<ToTrinhKetQuaGoiThauGetQuery, ToTrinhKetQuaGoiThau> {
    private readonly IRepository<ToTrinhKetQuaGoiThau, Guid> ToTrinhKetQuaGoiThau =
        serviceProvider.GetRequiredService<IRepository<ToTrinhKetQuaGoiThau, Guid>>();

    private readonly IRepository<TepDinhKem, Guid> TepDinhKem =
        serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();


    public async Task<ToTrinhKetQuaGoiThau> Handle(ToTrinhKetQuaGoiThauGetQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = ToTrinhKetQuaGoiThau.GetOrderedSet()
            .Include(e => e.GoiThaus)
           .ThenInclude(g => g.GoiThau)
           .Where(e => e.Id == request.Id);

        if (request.IsNoTracking)
            queryable = queryable.AsNoTracking();


        var entity = await queryable
            .FirstOrDefaultAsync(cancellationToken);

        ManagedException.ThrowIf(request.ThrowIfNull && entity == null, "Không tìm thấy dữ liệu");

        return entity!;
    }
}