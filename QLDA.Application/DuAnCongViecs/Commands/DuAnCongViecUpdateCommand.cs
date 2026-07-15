using Microsoft.EntityFrameworkCore;
using QLDA.Application.DuAnCongViecs.DTOs;

namespace QLDA.Application.DuAnCongViecs.Commands;

public record DuAnCongViecUpdateCommand(DuAnCongViecUpdateDto Dto) : IRequest<DuAnCongViec>;

internal class DuAnCongViecUpdateCommandHandler : IRequestHandler<DuAnCongViecUpdateCommand, DuAnCongViec> {
    private readonly IRepository<DuAn, Guid> DuAn;
    private readonly IUnitOfWork _unitOfWork;

    public DuAnCongViecUpdateCommandHandler(IServiceProvider serviceProvider) {
        DuAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        _unitOfWork = DuAn.UnitOfWork;
    }

    public async Task<DuAnCongViec> Handle(DuAnCongViecUpdateCommand request, CancellationToken cancellationToken = default) {
        var duAnEntity = await DuAn.GetQueryableSet()
            .Include(e => e.DuAnCongViecs)
            .FirstOrDefaultAsync(e => e.Id == request.Dto.DuAnId, cancellationToken)
            ?? throw new ManagedException("Dự án không tồn tại");

        var entity = duAnEntity.DuAnCongViecs!
            .FirstOrDefault(e => e.RightId == request.Dto.CongViecId && !e.IsDeleted)
            ?? throw new ManagedException("Liên kết không tồn tại");

        entity.Update(request.Dto);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return entity!;
    }
}