using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.HoSoMoiThauDienTus.DTOs;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.HoSoMoiThauDienTus.Commands;

public record HoSoMoiThauDienTuInsertCommand(HoSoMoiThauDienTuInsertDto Dto) : IRequest<HoSoMoiThauDienTu>;

internal class HoSoMoiThauDienTuInsertCommandHandler : IRequestHandler<HoSoMoiThauDienTuInsertCommand, HoSoMoiThauDienTu> {
    private readonly IRepository<HoSoMoiThauDienTu, Guid> HoSoMoiThauDienTu;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;

    public HoSoMoiThauDienTuInsertCommandHandler(IServiceProvider serviceProvider) {
        HoSoMoiThauDienTu = serviceProvider.GetRequiredService<IRepository<HoSoMoiThauDienTu, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _unitOfWork = HoSoMoiThauDienTu.UnitOfWork;
    }

    public async Task<HoSoMoiThauDienTu> Handle(HoSoMoiThauDienTuInsertCommand request, CancellationToken cancellationToken = default) {
        await _auth.EnsureCanExecuteStepAsync(request.Dto.BuocId, _authContext, cancellationToken);

        var trangThaiDuThao = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.HoSoMoiThauDienTu.DuThao && s.Loai == PheDuyetEntityNames.HoSoMoiThauDienTu, cancellationToken);

        var entity = request.Dto.ToEntity();
        entity.TrangThaiId = trangThaiDuThao?.Id;

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await HoSoMoiThauDienTu.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return entity;
    }
}