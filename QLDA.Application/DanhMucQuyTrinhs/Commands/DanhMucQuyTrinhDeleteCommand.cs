using Microsoft.EntityFrameworkCore;

namespace QLDA.Application.DanhMucQuyTrinhs.Commands;

public record DanhMucQuyTrinhDeleteCommand(int Id) : IRequest<int>;

internal class DanhMucQuyTrinhDeleteCommandHandler : IRequestHandler<DanhMucQuyTrinhDeleteCommand, int> {
    private readonly IRepository<DanhMucQuyTrinh, int> DanhMucQuyTrinh;
    private readonly IUnitOfWork UnitOfWork;

    public DanhMucQuyTrinhDeleteCommandHandler(IServiceProvider serviceProvider) {
        DanhMucQuyTrinh = serviceProvider.GetRequiredService<IRepository<DanhMucQuyTrinh, int>>();
        UnitOfWork = DanhMucQuyTrinh.UnitOfWork;
    }

    public async Task<int> Handle(DanhMucQuyTrinhDeleteCommand request, CancellationToken cancellationToken) {
        var entity = await DanhMucQuyTrinh.GetOrderedSet().FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
        ManagedException.ThrowIf(entity == null, "Danh mục quy trình không tồn tại.");

        entity!.IsDeleted = true;

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}