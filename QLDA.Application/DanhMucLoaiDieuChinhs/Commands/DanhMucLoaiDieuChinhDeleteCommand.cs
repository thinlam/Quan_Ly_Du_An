using Microsoft.EntityFrameworkCore;

namespace QLDA.Application.DanhMucLoaiDieuChinhs.Commands;

public record DanhMucLoaiDieuChinhDeleteCommand(int Id) : IRequest<bool>;

internal class DanhMucLoaiDieuChinhDeleteCommandHandler : IRequestHandler<DanhMucLoaiDieuChinhDeleteCommand, bool> {
    private readonly IRepository<DanhMucLoaiDieuChinh, int> DanhMucLoaiDieuChinh;
    private readonly IUnitOfWork UnitOfWork;

    public DanhMucLoaiDieuChinhDeleteCommandHandler(IServiceProvider serviceProvider) {
        DanhMucLoaiDieuChinh = serviceProvider.GetRequiredService<IRepository<DanhMucLoaiDieuChinh, int>>();
        UnitOfWork = DanhMucLoaiDieuChinh.UnitOfWork;
    }

    public async Task<bool> Handle(DanhMucLoaiDieuChinhDeleteCommand request, CancellationToken cancellationToken) {
        var entity = await DanhMucLoaiDieuChinh.GetOrderedSet().FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
        ManagedException.ThrowIf(entity == null, $"Không tìm thấy loại điều chỉnh có ID {request.Id}");

        entity.IsDeleted = true;
        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}