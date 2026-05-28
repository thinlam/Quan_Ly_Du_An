using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.ToTrinhThamDinhNhaThaus.DTOs;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities;

namespace QLDA.Application.ToTrinhThamDinhNhaThaus.Commands;

public record ToTrinhThamDinhNhaThauUpdateCommand(ToTrinhThamDinhNhaThau Dto) : IRequest<ToTrinhThamDinhNhaThau>;

internal class ToTrinhThamDinhNhaThauUpdateCommandHandler : IRequestHandler<ToTrinhThamDinhNhaThauUpdateCommand, ToTrinhThamDinhNhaThau> {
    private readonly IRepository<ToTrinhThamDinhNhaThau, Guid> _repo;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IUnitOfWork _unitOfWork;

    public ToTrinhThamDinhNhaThauUpdateCommandHandler(IServiceProvider serviceProvider) {
        _repo = serviceProvider.GetRequiredService<IRepository<ToTrinhThamDinhNhaThau, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _unitOfWork = _repo.UnitOfWork;
    }

    public async Task<ToTrinhThamDinhNhaThau> Handle(ToTrinhThamDinhNhaThauUpdateCommand request,
        CancellationToken cancellationToken = default) {
        var trangThaiDuThao = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatMacDinh.DuThao && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);

        var entity = await _repo.GetQueryableSet()
            .Include(e => e.NhaThaus)
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
        entity.TrangThaiThamDinhId = request.Dto.TrangThaiThamDinhId;
        entity.SyncNhaThauIds(request.Dto.Id,request.Dto.NhaThaus);
        // insert file cho TepDinhKem
        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await _repo.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return entity;
    }
}
