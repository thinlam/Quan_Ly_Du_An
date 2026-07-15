using Microsoft.EntityFrameworkCore;

namespace QLDA.Application.TrienKhaiKeHoachLCNTs.Queries;

public record TrienKhaiKeHoachLCNTGetQuery : IRequest<TrienKhaiKeHoachLCNT> {
    public Guid Id { get; set; }
    public bool ThrowIfNull { get; set; } = true;
    public bool IsNoTracking { get; set; }
}

internal class TrienKhaiKeHoachLCNTGetQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<TrienKhaiKeHoachLCNTGetQuery, TrienKhaiKeHoachLCNT> {
    private readonly IRepository<TrienKhaiKeHoachLCNT, Guid> TrienKhaiKeHoachLCNT =
        serviceProvider.GetRequiredService<IRepository<TrienKhaiKeHoachLCNT, Guid>>();

    private readonly IRepository<Attachment, Guid> TepDinhKem =
        serviceProvider.GetRequiredService<IRepository<Attachment, Guid>>();


    public async Task<TrienKhaiKeHoachLCNT> Handle(TrienKhaiKeHoachLCNTGetQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = TrienKhaiKeHoachLCNT.GetOrderedSet()
            .Include(e => e.DonViTuVans)
            .Include(e => e.DmHinhThucLCNT)
            .Include(e => e.GoiThau)
            .Where(e => e.Id == request.Id);

        if (request.IsNoTracking)
            queryable = queryable.AsNoTracking();


        var entity = await queryable
            .FirstOrDefaultAsync(cancellationToken);

        ManagedException.ThrowIf(request.ThrowIfNull && entity == null, "Không tìm thấy dữ liệu");

        return entity!;
    }
}