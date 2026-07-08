using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;
using QLDA.Application.Providers;
using QLDA.Application.ThanhLyHopDongs.DTOs;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.ThanhLyHopDongs.Commands;

public record ThanhLyHopDongInsertCommand(ThanhLyHopDongInsertDto Dto) : IRequest<ThanhLyHopDong>;

internal class ThanhLyHopDongInsertCommandHandler : IRequestHandler<ThanhLyHopDongInsertCommand, ThanhLyHopDong> {
    private readonly IRepository<ThanhLyHopDong, Guid> _thanhLy;
    private readonly IRepository<DuAn, Guid> _duAn;
    private readonly IRepository<NghiemThu, Guid> _nghiemThu;
    private readonly IRepository<HopDong, Guid> _hopDong;
    private readonly IRepository<DuAnBuoc, int> _buoc;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IAuthorizationManager _authManager;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;

    public ThanhLyHopDongInsertCommandHandler(IServiceProvider serviceProvider) {
        _thanhLy = serviceProvider.GetRequiredService<IRepository<ThanhLyHopDong, Guid>>();
        _duAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        _nghiemThu = serviceProvider.GetRequiredService<IRepository<NghiemThu, Guid>>();
        _hopDong = serviceProvider.GetRequiredService<IRepository<HopDong, Guid>>();
        _buoc = serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _unitOfWork = _thanhLy.UnitOfWork;
    }

    public async Task<ThanhLyHopDong> Handle(ThanhLyHopDongInsertCommand request, CancellationToken cancellationToken = default) {
        await ValidateAsync(request, cancellationToken);

        await _authManager.EnsureCanExecuteAsync(request.Dto.BuocId, request.Dto.DuAnId, _authContext, cancellationToken);

        var entity = request.Dto.ToEntity();

        // Auto-assign initial status = Dự thảo (UC63)
        var trangThaiDuThao = (await _statusRepository.GetByLoaiAsync(PheDuyetEntityNames.ThanhLyHopDong, cancellationToken))
            .FirstOrDefault(s => s.Ma == TrangThaiPheDuyetCodes.ThanhLyHopDong.DuThao);
        if (trangThaiDuThao != null) {
            entity.TrangThaiId = trangThaiDuThao.Id;
        }

        if (request.Dto.NghiemThuIds != null && request.Dto.NghiemThuIds.Count > 0) {
            entity.DanhSachNghiemThus = [.. request.Dto.NghiemThuIds
                .Select(nghiemThuId => new ThanhLyHopDongNghiemThu {
                    LeftId = entity.Id,
                    RightId = nghiemThuId
                })];
        }

        if (_unitOfWork.HasTransaction) {
            await _thanhLy.AddAsync(entity, cancellationToken);
        } else {
            using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
            await _thanhLy.AddAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }

        return entity;
    }

    #region Private helper methods

    private async Task ValidateAsync(ThanhLyHopDongInsertCommand request, CancellationToken cancellationToken) {
        ManagedException.ThrowIf(
            !await _duAn.GetQueryableSet().AnyAsync(e => e.Id == request.Dto.DuAnId, cancellationToken: cancellationToken),
            "Không tồn tại dự án");

        if (request.Dto.BuocId.HasValue) {
            ManagedException.ThrowIf(
                !await _buoc.GetQueryableSet().AnyAsync(e => e.Id == request.Dto.BuocId && e.DuAnId == request.Dto.DuAnId, cancellationToken),
                "Bước không thuộc dự án");
        }

        if (request.Dto.HopDongId.HasValue) {
            ManagedException.ThrowIf(
                !await _hopDong.GetQueryableSet().AnyAsync(e => e.Id == request.Dto.HopDongId && e.DuAnId == request.Dto.DuAnId, cancellationToken),
                "Hợp đồng không thuộc dự án");
        }

        if (request.Dto.NghiemThuIds != null && request.Dto.NghiemThuIds.Count != 0) {
            var count = await _nghiemThu.GetQueryableSet()
                .CountAsync(e => request.Dto.NghiemThuIds.Contains(e.Id) && e.DuAnId == request.Dto.DuAnId, cancellationToken);
            ManagedException.ThrowIf(
                count != request.Dto.NghiemThuIds.Count,
                "Một hoặc nhiều đợt nghiệm thu không tồn tại hoặc không thuộc dự án");
        }
    }

    #endregion
}
