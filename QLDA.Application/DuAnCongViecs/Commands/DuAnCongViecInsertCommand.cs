using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.DuAnCongViecs.DTOs;

namespace QLDA.Application.DuAnCongViecs.Commands;

public record DuAnCongViecInsertCommand(DuAnCongViecInsertDto Dto) : IRequest<DuAnCongViec>;

internal class DuAnCongViecInsertCommandHandler : IRequestHandler<DuAnCongViecInsertCommand, DuAnCongViec> {
    private readonly IRepository<DuAn, Guid> DuAn;
    private readonly IUserProvider _userProvider;
    private readonly IUnitOfWork _unitOfWork;

    public DuAnCongViecInsertCommandHandler(IServiceProvider serviceProvider) {
        DuAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _unitOfWork = DuAn.UnitOfWork;
    }

    public async Task<DuAnCongViec> Handle(DuAnCongViecInsertCommand request, CancellationToken cancellationToken = default) {
        var duAnEntity = await DuAn.GetQueryableSet()
            .Include(e => e.DuAnCongViecs)
            .FirstOrDefaultAsync(e => e.Id == request.Dto.DuAnId, cancellationToken)
            ?? throw new ManagedException("Dự án không tồn tại");

        ManagedException.ThrowIf(duAnEntity.DuAnCongViecs!.Any(e => e.RightId == request.Dto.CongViecId && !e.IsDeleted),
            "Liên kết đã tồn tại");

        var entity = request.Dto.ToEntity(_userProvider.Id);
        duAnEntity.DuAnCongViecs!.Add(entity);

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return entity!;
    }
}