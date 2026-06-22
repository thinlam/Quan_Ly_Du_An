using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.ThoaThuanGiaoViecs.DTOs;
using QLDA.Domain.Constants;

namespace QLDA.Application.ThoaThuanGiaoViecs.Commands;

public record ThoaThuanGiaoViecUpdateCommand(ThoaThuanGiaoViecDto Dto) : IRequest<ThoaThuanGiaoViec>;

internal class ThoaThuanGiaoViecUpdateCommandHandler : IRequestHandler<ThoaThuanGiaoViecUpdateCommand, ThoaThuanGiaoViec>
{
    private readonly IRepository<ThoaThuanGiaoViec, Guid> _repo;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;

    public ThoaThuanGiaoViecUpdateCommandHandler(IServiceProvider serviceProvider)
    {
        _repo = serviceProvider.GetRequiredService<IRepository<ThoaThuanGiaoViec, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _unitOfWork = _repo.UnitOfWork;
    }

    public async Task<ThoaThuanGiaoViec> Handle(ThoaThuanGiaoViecUpdateCommand request, CancellationToken cancellationToken = default)
    {
        var trangThaiDuThao = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.ThoaThuanGiaoViec.DuThao && s.Loai == PheDuyetEntityNames.KeHoachLuaChonNhaThauRutGon, cancellationToken);

        var trangThaiDaChuyen = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.ThoaThuanGiaoViec.DaChuyen && s.Loai == PheDuyetEntityNames.KeHoachLuaChonNhaThauRutGon, cancellationToken);

        var entity = await _repo.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Dto.Id, cancellationToken);
        ManagedException.ThrowIf(entity == null, "Không tìm thấy dữ liệu.");

        await _auth.EnsureCanExecuteStepAsync(entity.BuocId, _authContext, cancellationToken);

        // Validate current status must be null (legacy), Dự thảo, or Migrated (LEG)
        if (entity.TrangThaiId != null && entity.TrangThaiId != trangThaiDaChuyen?.Id && entity.TrangThaiId != trangThaiDuThao?.Id)
        {
            throw new ManagedException("Trạng thái không thể cập nhật");
        }

        entity.ThoiGian = request.Dto.ThoiGian;
        entity.GiaTri = request.Dto.GiaTri;
        entity.ChatLuong = request.Dto.ChatLuong;
        entity.PhamVi = request.Dto.PhamVi;
        entity.NoiDung = request.Dto.NoiDung;
        entity.GoiThauId = request.Dto.GoiThauId;

        entity.DuAnId = request.Dto.DuAnId;
        entity.BuocId = request.Dto.BuocId;

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await _repo.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return entity;
    }
}

