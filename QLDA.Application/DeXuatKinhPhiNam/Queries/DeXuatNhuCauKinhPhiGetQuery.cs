using Microsoft.EntityFrameworkCore;

namespace QLDA.Application.DeXuatNhuCauKinhPhiNams.Queries;

public record DeXuatNhuCauKinhPhiNamGetQuery : IRequest<DeXuatNhuCauKinhPhiNam> {
    public Guid Id { get; set; }
    public bool ThrowIfNull { get; set; } = true;
    public bool IsNoTracking { get; set; }
}

internal class DeXuatNhuCauKinhPhiNamGetQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<DeXuatNhuCauKinhPhiNamGetQuery, DeXuatNhuCauKinhPhiNam> {
    private readonly IRepository<DeXuatNhuCauKinhPhiNam, Guid> DeXuatNhuCauKinhPhiNam =
        serviceProvider.GetRequiredService<IRepository<DeXuatNhuCauKinhPhiNam, Guid>>();

    private readonly IRepository<TepDinhKem, Guid> TepDinhKem =
        serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();


    public async Task<DeXuatNhuCauKinhPhiNam> Handle(DeXuatNhuCauKinhPhiNamGetQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = DeXuatNhuCauKinhPhiNam.GetOrderedSet()
            .Where(e => e.Id == request.Id);

        if (request.IsNoTracking)
            queryable = queryable.AsNoTracking();


        var entity = await queryable
            .FirstOrDefaultAsync(cancellationToken);

        ManagedException.ThrowIf(request.ThrowIfNull && entity == null, "Không tìm thấy dữ liệu");

        return entity!;
    }
}