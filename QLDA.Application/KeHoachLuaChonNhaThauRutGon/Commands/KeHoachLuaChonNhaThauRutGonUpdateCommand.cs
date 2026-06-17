using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.KeHoachLuaChonNhaThauRutGons.DTOs;
using QLDA.Application.Authorization;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities;

namespace QLDA.Application.KeHoachLuaChonNhaThauRutGons.Commands;

public record KeHoachLuaChonNhaThauRutGonUpdateCommand(KeHoachLuaChonNhaThauRutGonDto Dto) : IRequest<KeHoachLuaChonNhaThauRutGon>;

internal class KeHoachLuaChonNhaThauRutGonUpdateCommandHandler : IRequestHandler<KeHoachLuaChonNhaThauRutGonUpdateCommand, KeHoachLuaChonNhaThauRutGon>
{
    private readonly IRepository<KeHoachLuaChonNhaThauRutGon, Guid> _repo;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;

    public KeHoachLuaChonNhaThauRutGonUpdateCommandHandler(IServiceProvider serviceProvider)
    {
        _repo = serviceProvider.GetRequiredService<IRepository<KeHoachLuaChonNhaThauRutGon, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _unitOfWork = _repo.UnitOfWork;
    }

    public async Task<KeHoachLuaChonNhaThauRutGon> Handle(KeHoachLuaChonNhaThauRutGonUpdateCommand request, CancellationToken cancellationToken = default)
    {
        // có 2 role Update là Người Trình và Phòng KHTC
        // if(TrangThai = "Dự thảo"  & user.UserId = entity.CreateBy ) -> allow
        // if(trangThai = "Đã trình" & isPhongKHTC ) -> cho phép

        var trangThaiDuThao = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.KeHoachLuaChonNhaThauRutGon.DuThao && s.Loai == PheDuyetEntityNames.KeHoachLuaChonNhaThauRutGon, cancellationToken);
        var trangThaiDaChuyen = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.KeHoachLuaChonNhaThauRutGon.DaChuyen && s.Loai == PheDuyetEntityNames.KeHoachLuaChonNhaThauRutGon, cancellationToken);

        var entity = await _repo.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Dto.Id, cancellationToken);
        ManagedException.ThrowIf(entity == null, "Không tìm thấy dữ liệu.");

        await _auth.EnsureCanExecuteStepAsync(request.Dto.BuocId, _authContext, cancellationToken);

        if (entity.TrangThaiId != null &&  entity.TrangThaiId != trangThaiDuThao?.Id && entity.TrangThaiId != trangThaiDaChuyen?.Id)
        {
            throw new ManagedException("Trạng thái không thể cập nhật.");
        }

        entity.NhaThauId = request.Dto.NhaThauId;
        entity.GoiThauId = request.Dto.GoiThauId;
        entity.KetQuaDanhGia = request.Dto.KetQuaDanhGia;

        entity.DuAnId = request.Dto.DuAnId;
        entity.BuocId = request.Dto.BuocId;

        await _repo.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return entity;
    }
}

