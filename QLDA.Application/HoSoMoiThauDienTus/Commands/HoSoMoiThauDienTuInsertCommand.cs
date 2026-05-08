using System.Data;
using QLDA.Application.HoSoMoiThauDienTus.DTOs;

namespace QLDA.Application.HoSoMoiThauDienTus.Commands;

public record HoSoMoiThauDienTuInsertCommand(HoSoMoiThauDienTuInsertDto Dto) : IRequest<HoSoMoiThauDienTu>;

internal class HoSoMoiThauDienTuInsertCommandHandler : IRequestHandler<HoSoMoiThauDienTuInsertCommand, HoSoMoiThauDienTu> {
    private readonly IRepository<HoSoMoiThauDienTu, Guid> HoSoMoiThauDienTu;
    private readonly IUnitOfWork _unitOfWork;

    public HoSoMoiThauDienTuInsertCommandHandler(IServiceProvider serviceProvider) {
        HoSoMoiThauDienTu = serviceProvider.GetRequiredService<IRepository<HoSoMoiThauDienTu, Guid>>();
        _unitOfWork = HoSoMoiThauDienTu.UnitOfWork;
    }

    public async Task<HoSoMoiThauDienTu> Handle(HoSoMoiThauDienTuInsertCommand request, CancellationToken cancellationToken = default) {
        var entity = request.Dto.ToEntity();

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await HoSoMoiThauDienTu.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return entity;
    }
}