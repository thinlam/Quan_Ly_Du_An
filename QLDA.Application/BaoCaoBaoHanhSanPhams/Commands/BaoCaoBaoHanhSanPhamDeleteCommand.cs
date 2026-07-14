using System.Data;
using Microsoft.EntityFrameworkCore;

namespace QLDA.Application.BaoCaoBaoHanhSanPhams.Commands;

public record BaoCaoBaoHanhSanPhamDeleteCommand(Guid Id) : IRequest {
}

public record BaoCaoBaoHanhSanPhamDeleteCommandHandler : IRequestHandler<BaoCaoBaoHanhSanPhamDeleteCommand> {
    private readonly IRepository<BaoCaoBaoHanhSanPham, Guid> BaoCaoBaoHanhSanPham;
    private readonly IRepository<Attachment, Guid> TepDinhKem;
    private readonly IUnitOfWork _unitOfWork;

    public BaoCaoBaoHanhSanPhamDeleteCommandHandler(IServiceProvider serviceProvider) {
        BaoCaoBaoHanhSanPham = serviceProvider.GetRequiredService<IRepository<BaoCaoBaoHanhSanPham, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<Attachment, Guid>>();
        _unitOfWork = BaoCaoBaoHanhSanPham.UnitOfWork;
    }

    public async Task Handle(BaoCaoBaoHanhSanPhamDeleteCommand request, CancellationToken cancellationToken) {
        var entity = await BaoCaoBaoHanhSanPham.GetOrderedSet()
           .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity);
        entity.IsDeleted = true;

        var files = await TepDinhKem.GetOrderedSet()
            .Where(o => o.GroupId == entity.Id.ToString()).ToListAsync(cancellationToken);

        files.ForEach(f => f.IsDeleted = true);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}