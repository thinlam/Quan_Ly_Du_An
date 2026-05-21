using System.Data;
using BuildingBlocks.CrossCutting.ExtensionMethods;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.ToTrinhKeHoachs.DTOs;
using QLDA.Domain.Constants;

namespace QLDA.Application.ToTrinhKeHoachs.Commands;

public record ToTrinhKeHoachUpdateCommand(ToTrinhKeHoachUpdateDto Dto) : IRequest<ToTrinhKeHoach>;

internal class ToTrinhKeHoachUpdateCommandHandler : IRequestHandler<ToTrinhKeHoachUpdateCommand, ToTrinhKeHoach>
{
    private readonly IRepository<ToTrinhKeHoach, Guid> _repo;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IUnitOfWork _unitOfWork;

    public ToTrinhKeHoachUpdateCommandHandler(IServiceProvider serviceProvider)
    {
        _repo = serviceProvider.GetRequiredService<IRepository<ToTrinhKeHoach, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _unitOfWork = _repo.UnitOfWork;
    }

    public async Task<ToTrinhKeHoach> Handle(ToTrinhKeHoachUpdateCommand request, CancellationToken cancellationToken = default)
    {
        var trangThaiDuThao = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.ToTrinhKeHoach.DuThao && s.Loai == PheDuyetEntityNames.ToTrinhKeHoach, cancellationToken);

        var entity = await _repo.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Dto.Id, cancellationToken);
        ManagedException.ThrowIf(entity == null, "Không tìm thấy dữ liệu.");

        // Validate current status must be null (legacy), Dự thảo, or Migrated (LEG)
        if (entity.TrangThaiId != null && entity.TrangThaiId != trangThaiDuThao?.Id && entity.TrangThai?.Ma != "LEG")
        {
            throw new ManagedException("Chỉ có thể cập nhật khi trạng thái là dự thảo");
        }

        entity.So = request.Dto.So;
        entity.NgayToTrinh = request.Dto.NgayToTrinh.ToStartOfDayUtc();
        entity.TrichYeu = request.Dto.TrichYeu;
        entity.DuAnId = request.Dto.DuAnId;
        entity.BuocId = request.Dto.BuocId;

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await _repo.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return entity;
    }
}

