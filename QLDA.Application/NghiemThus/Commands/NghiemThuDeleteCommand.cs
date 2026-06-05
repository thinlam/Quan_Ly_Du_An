using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;

namespace QLDA.Application.NghiemThus.Commands;

public record NghiemThuDeleteCommand(Guid Id) : IRequest {
}

public record NghiemThuDeleteCommandHandler : IRequestHandler<NghiemThuDeleteCommand> {
    private readonly IRepository<NghiemThu, Guid> NghiemThu;
    private readonly IRepository<ThanhToan, Guid> ThanhToan;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;
    private readonly IUnitOfWork _unitOfWork;

    public NghiemThuDeleteCommandHandler(IServiceProvider serviceProvider) {
        NghiemThu = serviceProvider.GetRequiredService<IRepository<NghiemThu, Guid>>();
        ThanhToan = serviceProvider.GetRequiredService<IRepository<ThanhToan, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        _unitOfWork = NghiemThu.UnitOfWork;
    }

    public async Task Handle(NghiemThuDeleteCommand request, CancellationToken cancellationToken) {
        var entity = await NghiemThu.GetOrderedSet()
           .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);
        var hasNghiemThu = await NghiemThu.GetQueryableSet().AnyAsync(e => e.HopDongId == request.Id && e.HopDong != null && !e.HopDong.IsDeleted, cancellationToken);
        if(entity == null) 
            ManagedException.ThrowIfNull(entity);
        var hasThanhToan = await ThanhToan.GetQueryableSet().AnyAsync(e => e.NghiemThuId == request.Id 
        && e.NghiemThu != null && !e.NghiemThu.IsDeleted, cancellationToken);

        if (hasThanhToan) 
          ManagedException.Throw("Nghiệm thu này đã có hóa đơn thanh toán. Không thể xóa");

        entity.IsDeleted = true;

        await SyncHelper.SetDeleteWithRelatedFiles(TepDinhKem, [entity.Id.ToString()], cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}