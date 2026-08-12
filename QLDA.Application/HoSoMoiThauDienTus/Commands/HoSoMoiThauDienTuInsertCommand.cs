using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.HoSoMoiThauDienTus.DTOs;
using QLDA.Domain.Constants;
using QLDA.Domain.Enums;

namespace QLDA.Application.HoSoMoiThauDienTus.Commands;

public record HoSoMoiThauDienTuInsertCommand(HoSoMoiThauDienTuInsertDto Dto) : IRequest<HoSoMoiThauDienTu>;

internal class HoSoMoiThauDienTuInsertCommandHandler : IRequestHandler<HoSoMoiThauDienTuInsertCommand, HoSoMoiThauDienTu>
{
    private readonly IRepository<HoSoMoiThauDienTu, Guid> HoSoMoiThauDienTu;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IRepository<ToTrinhQuyetDinh, long> _toTrinhQuyetDinhRepo;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationManager _authManager;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;

    public HoSoMoiThauDienTuInsertCommandHandler(IServiceProvider serviceProvider)
    {
        HoSoMoiThauDienTu = serviceProvider.GetRequiredService<IRepository<HoSoMoiThauDienTu, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _toTrinhQuyetDinhRepo = serviceProvider.GetRequiredService<IRepository<ToTrinhQuyetDinh, long>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _unitOfWork = HoSoMoiThauDienTu.UnitOfWork;
    }

    public async Task<HoSoMoiThauDienTu> Handle(HoSoMoiThauDienTuInsertCommand request, CancellationToken cancellationToken = default)
    {
        await _auth.EnsureCanExecuteStepAsync(request.Dto.BuocId, _authContext, cancellationToken);
        await _authManager.EnsureCanExecuteAsync(request.Dto.BuocId, request.Dto.DuAnId ?? Guid.Empty, _authContext, cancellationToken);

        var trangThaiDuThao = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.HoSoMoiThauDienTu.DuThao && s.Loai == PheDuyetEntityNames.HoSoMoiThauDienTu, cancellationToken);

        var entity = request.Dto.ToEntity();
        entity.TrangThaiId = trangThaiDuThao?.Id;

        // Tách ToTrinh/QuyetDinh ra khỏi entity trước khi Add — không còn là navigation EF (Issue #179).
        var toTrinh = entity.ToTrinh;
        var quyetDinh = entity.QuyetDinh;
        entity.ToTrinh = null;
        entity.QuyetDinh = null;

        await HoSoMoiThauDienTu.AddAsync(entity, cancellationToken);

        if (toTrinh != null)
        {
            toTrinh.EntityId = entity.Id;
            toTrinh.Loai = (int)ELoaiToTrinhQuyetDinh.HoSoMoiThauToTrinh;
            await _toTrinhQuyetDinhRepo.AddAsync(toTrinh, cancellationToken);
            entity.ToTrinh = toTrinh;
        }
        if (quyetDinh != null)
        {
            quyetDinh.EntityId = entity.Id;
            quyetDinh.Loai = (int)ELoaiToTrinhQuyetDinh.HoSoMoiThauQuyetDinh;
            await _toTrinhQuyetDinhRepo.AddAsync(quyetDinh, cancellationToken);
            entity.QuyetDinh = quyetDinh;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return entity;
    }
}