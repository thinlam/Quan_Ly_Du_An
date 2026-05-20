using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.HoSoMoiThauDienTus.DTOs;
using QLDA.Domain.Constants;

namespace QLDA.Application.HoSoMoiThauDienTus.Commands;

public record HoSoMoiThauDienTuUpdateCommand(HoSoMoiThauDienTuUpdateModel Model) : IRequest<HoSoMoiThauDienTu>;

internal class HoSoMoiThauDienTuUpdateCommandHandler : IRequestHandler<HoSoMoiThauDienTuUpdateCommand, HoSoMoiThauDienTu> {
    private readonly IRepository<HoSoMoiThauDienTu, Guid> HoSoMoiThauDienTu;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IUnitOfWork _unitOfWork;

    public HoSoMoiThauDienTuUpdateCommandHandler(IServiceProvider serviceProvider) {
        HoSoMoiThauDienTu = serviceProvider.GetRequiredService<IRepository<HoSoMoiThauDienTu, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _unitOfWork = HoSoMoiThauDienTu.UnitOfWork;
    }

    public async Task<HoSoMoiThauDienTu> Handle(HoSoMoiThauDienTuUpdateCommand request, CancellationToken cancellationToken = default) {
        var trangThaiDuThao = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.HoSoMoiThauDienTu.DuThao && s.Loai == PheDuyetEntityNames.HoSoMoiThauDienTu, cancellationToken);

        var entity = await HoSoMoiThauDienTu.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Model.Id, cancellationToken);
        ManagedException.ThrowIfNull(entity, "Không tìm thấy hồ sơ mời thầu điện tử");

        // Validate current status must be null (legacy), Dự thảo, or Migrated (LEG)
        if (entity.TrangThaiId != null && entity.TrangThaiId != trangThaiDuThao?.Id && entity.TrangThaiPheDuyet?.Ma != "LEG") {
            throw new ManagedException("Chỉ có thể cập nhật khi trạng thái là Dự thảo");
        }

        entity.Update(request.Model);

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await HoSoMoiThauDienTu.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return entity;
    }
}