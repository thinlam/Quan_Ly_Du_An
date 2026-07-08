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

public record ThanhLyHopDongUpdateCommand(ThanhLyHopDongUpdateDto Dto) : IRequest<ThanhLyHopDong>;

internal class ThanhLyHopDongUpdateCommandHandler : IRequestHandler<ThanhLyHopDongUpdateCommand, ThanhLyHopDong>
{
    private readonly IRepository<ThanhLyHopDong, Guid> _thanhLy;
    private readonly IRepository<DuAn, Guid> _duAn;
    private readonly IRepository<NghiemThu, Guid> _nghiemThu;
    private readonly IRepository<HopDong, Guid> _hopDong;
    private readonly IRepository<DuAnBuoc, int> _buoc;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;

    public ThanhLyHopDongUpdateCommandHandler(IServiceProvider serviceProvider)
    {
        _thanhLy = serviceProvider.GetRequiredService<IRepository<ThanhLyHopDong, Guid>>();
        _duAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        _nghiemThu = serviceProvider.GetRequiredService<IRepository<NghiemThu, Guid>>();
        _hopDong = serviceProvider.GetRequiredService<IRepository<HopDong, Guid>>();
        _buoc = serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _unitOfWork = _thanhLy.UnitOfWork;
    }

    public async Task<ThanhLyHopDong> Handle(ThanhLyHopDongUpdateCommand request, CancellationToken cancellationToken = default)
    {
        await ValidateAsync(request, cancellationToken);

        // Load entity with junction tracked by EF — navigation collection is change-tracked
        var entity = await _thanhLy.GetQueryableSet()
            .Include(e => e.DanhSachNghiemThus)
            .FirstOrDefaultAsync(e => e.Id == request.Dto.Id, cancellationToken);
        ManagedException.ThrowIfNull(entity);

        await _auth.EnsureCanExecuteStepAsync(entity.BuocId, _authContext, cancellationToken);

        // Status guard: chỉ cho phép cập nhật khi status = DT hoặc TL (UC63)
        var statuses = await _statusRepository.GetByLoaiAsync(PheDuyetEntityNames.ThanhLyHopDong, cancellationToken);
        var statusDict = statuses
            .Where(x => !string.IsNullOrWhiteSpace(x.Ma))
            .ToDictionary(x => x.Ma!, x => x);
        var trangThaiDuThao = statusDict.GetValueOrDefault(TrangThaiPheDuyetCodes.ThanhLyHopDong.DuThao);
        var trangThaiTraLai = statusDict.GetValueOrDefault(TrangThaiPheDuyetCodes.ThanhLyHopDong.TraLai);

        if (entity.TrangThaiId != null
            && entity.TrangThaiId != trangThaiDuThao?.Id
            && entity.TrangThaiId != trangThaiTraLai?.Id)
        {
            throw new ManagedException("Chỉ có thể cập nhật khi trạng thái là Dự thảo hoặc Trả lại");
        }

        // Update scalar properties
        entity.DuAnId = request.Dto.DuAnId;
        entity.BuocId = request.Dto.BuocId;
        entity.HopDongId = request.Dto.HopDongId;
        entity.So = request.Dto.So;
        entity.Ngay = request.Dto.Ngay;
        entity.TrichYeu = request.Dto.TrichYeu;

        // Sync junction via navigation tracking: clear existing, add new
        entity.DanhSachNghiemThus?.Clear();
        if (request.Dto.NghiemThuIds != null)
        {
            entity.DanhSachNghiemThus = [.. request.Dto.NghiemThuIds
                .Select(nghiemThuId => new ThanhLyHopDongNghiemThu {
                    LeftId = entity.Id,
                    RightId = nghiemThuId
                })];
        }

        if (_unitOfWork.HasTransaction)
        {
            await _thanhLy.UpdateAsync(entity, cancellationToken);
        }
        else
        {
            using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
            await _thanhLy.UpdateAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }

        return entity;
    }

    #region Private helper methods

    private async Task ValidateAsync(ThanhLyHopDongUpdateCommand request, CancellationToken cancellationToken)
    {
        ManagedException.ThrowIf(
            !await _duAn.GetQueryableSet().AnyAsync(e => e.Id == request.Dto.DuAnId, cancellationToken: cancellationToken),
            "Không tồn tại dự án");

        if (request.Dto.BuocId.HasValue)
        {
            ManagedException.ThrowIf(
                !await _buoc.GetQueryableSet().AnyAsync(e => e.Id == request.Dto.BuocId && e.DuAnId == request.Dto.DuAnId, cancellationToken),
                "Bước không thuộc dự án");
        }

        if (request.Dto.HopDongId.HasValue)
        {
            ManagedException.ThrowIf(
                !await _hopDong.GetQueryableSet().AnyAsync(e => e.Id == request.Dto.HopDongId && e.DuAnId == request.Dto.DuAnId, cancellationToken),
                "Hợp đồng không thuộc dự án");
        }

        if (request.Dto.NghiemThuIds != null && request.Dto.NghiemThuIds.Count != 0)
        {
            var count = await _nghiemThu.GetQueryableSet()
                .CountAsync(e => request.Dto.NghiemThuIds.Contains(e.Id) && e.DuAnId == request.Dto.DuAnId, cancellationToken);
            ManagedException.ThrowIf(
                count != request.Dto.NghiemThuIds.Count,
                "Một hoặc nhiều đợt nghiệm thu không tồn tại hoặc không thuộc dự án");
        }
    }

    #endregion
}
