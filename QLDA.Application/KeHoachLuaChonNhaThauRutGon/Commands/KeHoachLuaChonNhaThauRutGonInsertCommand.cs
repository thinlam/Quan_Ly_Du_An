using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.KeHoachLuaChonNhaThauRutGons.DTOs;
using QLDA.Application.Authorization;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.KeHoachLuaChonNhaThauRutGons.Commands;

public record KeHoachLuaChonNhaThauRutGonInsertCommand(KeHoachLuaChonNhaThauRutGon Dto) : IRequest<KeHoachLuaChonNhaThauRutGon>;

internal class KeHoachLuaChonNhaThauRutGonInsertCommandHandler : IRequestHandler<KeHoachLuaChonNhaThauRutGonInsertCommand, KeHoachLuaChonNhaThauRutGon>
{
    private readonly IRepository<KeHoachLuaChonNhaThauRutGon, Guid> _repo;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IRepository<DuAnBuoc, int> _duAnBuocRepo;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IUserProvider _userProvider;
    private readonly IUnitOfWork _unitOfWork;

    public KeHoachLuaChonNhaThauRutGonInsertCommandHandler(IServiceProvider serviceProvider)
    {
        _repo = serviceProvider.GetRequiredService<IRepository<KeHoachLuaChonNhaThauRutGon, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _duAnBuocRepo = serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _unitOfWork = _repo.UnitOfWork;
    }

    public async Task<KeHoachLuaChonNhaThauRutGon> Handle(KeHoachLuaChonNhaThauRutGonInsertCommand request, CancellationToken cancellationToken = default)
    {
        if (request.Dto.BuocId.HasValue)
        {
            var buoc = await _duAnBuocRepo.GetQueryableSet()
                .Include(e => e.DuAn)
                .Include(e => e.DuAnBuocPhongBanPhoiHops)
                .FirstOrDefaultAsync(e => e.Id == request.Dto.BuocId.Value, cancellationToken);
            if (buoc != null && !await _auth.CanExecuteStepAsync(buoc, _userProvider, cancellationToken))
                throw new ManagedException("Phòng ban không có quyền thao tác bước này");
        }

        // Auto-assign Dự thảo status
        try
        {
            var trangThaiDuThao = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
                       .FirstOrDefaultAsync(s => s.Ma == "DT" && s.Loai == PheDuyetEntityNames.KeHoachLuaChonNhaThauRutGon, cancellationToken);
            var entity = request.Dto;
            entity.TrangThaiId = trangThaiDuThao?.Id;
            await _repo.AddAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return entity;
        }
        catch (Exception ex)
        {

            throw;
        }

    }
}
