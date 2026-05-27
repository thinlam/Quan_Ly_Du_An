using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.ToTrinhKetQuaGoiThauMappings;
using QLDA.Application.ToTrinhKetQuaGoiThaus.DTOs;
using QLDA.Domain.Constants;

namespace QLDA.Application.ToTrinhKetQuaGoiThaus.Commands;

public record ToTrinhKetQuaGoiThauUpdateCommand(ToTrinhKetQuaGoiThauInsertDto Dto) : IRequest<ToTrinhKetQuaGoiThau>;

internal class ToTrinhKetQuaGoiThauUpdateCommandHandler : IRequestHandler<ToTrinhKetQuaGoiThauUpdateCommand, ToTrinhKetQuaGoiThau> {
    private readonly IRepository<ToTrinhKetQuaGoiThau, Guid> _repo;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IUnitOfWork _unitOfWork;

    public ToTrinhKetQuaGoiThauUpdateCommandHandler(IServiceProvider serviceProvider) {
        _repo = serviceProvider.GetRequiredService<IRepository<ToTrinhKetQuaGoiThau, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _unitOfWork = _repo.UnitOfWork;
    }

    public async Task<ToTrinhKetQuaGoiThau> Handle(ToTrinhKetQuaGoiThauUpdateCommand request,
        CancellationToken cancellationToken = default) {
        var trangThaiDuThao = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatMacDinh.DuThao && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);

        var entity = await _repo.GetQueryableSet()
            .Include(e => e.GoiThaus)
            .Include(e => e.TrangThai)
            .FirstOrDefaultAsync(e => e.Id == request.Dto.Id, cancellationToken);
        ManagedException.ThrowIf(entity == null, "Không tìm thấy dữ liệu.");

        if (entity.TrangThaiId != null && entity.TrangThaiId != trangThaiDuThao?.Id && entity.TrangThai?.Ma != "LEG") {
            throw new ManagedException("Chỉ có thể cập nhật khi trạng thái là dự thảo");
        }

        entity.DuAnId = request.Dto.DuAnId;
        entity.BuocId = request.Dto.BuocId;
        entity.So = request.Dto.So;
        entity.NgayTrinh = request.Dto.NgayTrinh;
        entity.TrichYeu = request.Dto.TrichYeu;
        entity.TrangThaiDangTaiId = request.Dto.TrangThaiDangTaiId;
        entity.SyncGoiThauIds(request.Dto.DanhSachGoiThau);

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await _repo.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return entity;
    }
}
