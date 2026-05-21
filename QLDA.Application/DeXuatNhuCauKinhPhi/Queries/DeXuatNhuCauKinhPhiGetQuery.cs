using Microsoft.EntityFrameworkCore;

namespace QLDA.Application.DeXuatNhuCauKinhPhis.Queries;

public record DeXuatNhuCauKinhPhiGetQuery : IRequest<DeXuatNhuCauKinhPhi> {
    public Guid Id { get; set; }
    public bool ThrowIfNull { get; set; } = true;
    public bool IsNoTracking { get; set; }
}

internal class DeXuatNhuCauKinhPhiGetQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<DeXuatNhuCauKinhPhiGetQuery, DeXuatNhuCauKinhPhi> {
    private readonly IRepository<DeXuatNhuCauKinhPhi, Guid> DeXuatNhuCauKinhPhi =
        serviceProvider.GetRequiredService<IRepository<DeXuatNhuCauKinhPhi, Guid>>();

    private readonly IRepository<TepDinhKem, Guid> TepDinhKem =
        serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();


    public async Task<DeXuatNhuCauKinhPhi> Handle(DeXuatNhuCauKinhPhiGetQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = DeXuatNhuCauKinhPhi.GetOrderedSet()
            .Where(e => e.Id == request.Id);

        if (request.IsNoTracking)
            queryable = queryable.AsNoTracking();


        var entity = await queryable
            .FirstOrDefaultAsync(cancellationToken);

        ManagedException.ThrowIf(request.ThrowIfNull && entity == null, "Không tìm thấy dữ liệu");

        return entity!;
    }
}